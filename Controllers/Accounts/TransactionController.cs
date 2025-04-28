using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WalletApi.Data;
using WalletApi.Models.DBModels;
using WalletApi.Models.DTOS;

namespace WalletApi.Controllers.Accounts
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositDto depositDto)
        {
            // Validate DTO
            if (string.IsNullOrWhiteSpace(depositDto.ProjectName) ||
                string.IsNullOrWhiteSpace(depositDto.AccountName) ||
                depositDto.Amount <= 0)
            {
                return BadRequest(new { Message = "Invalid input data." });
            }

            // Get the authenticated user's email and role
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            // Fetch the account to deposit into
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.ProjectName == depositDto.ProjectName && a.AccountName == depositDto.AccountName);

            if (account == null)
            {
                return NotFound(new { Message = "Account not found in the specified project." });
            }

            // Check if the account is a Master account
            if (account.AccountName != "Master")
            {
                return BadRequest(new { Message = "Deposits can only be made into the Master account." });
            }

            // Check if the user is the Project Admin for the given project
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectName == depositDto.ProjectName && p.CreatedBy == userName);
            if (project == null || project.CreatedBy != userName)
            {
                return BadRequest(new { Message = "You are not authorized to deposit into the Master account. Only the Project Admin who created the project can perform this action." });
            }
            project.Budget += depositDto.Amount;

            // Perform the deposit
            account.Amount += depositDto.Amount;

            // Log the transaction
            var transaction = new TransactionStatement
            {
                ProjectName = depositDto.ProjectName,
                FromAccount = "External", // External source of funds
                ToAccount = depositDto.AccountName,
                TransactionType = "Deposit",
                Amount = depositDto.Amount,
                PerformedBy = userEmail,
                TransactionDate = DateTime.UtcNow,
                Remarks = "Budget Increased"
            };
            _context.Projects.Update(project);
            _context.TransactionStatements.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Deposit successful.",
                Account = new
                {
                    AccountName = account.AccountName,
                    ProjectName = account.ProjectName,
                    Amount = account.Amount
                },
                Transaction = transaction
            });
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDto transferDto)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            var fromAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountName == transferDto.FromAccountName && a.ProjectName == transferDto.ProjectName);

            var toAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountName == transferDto.ToAccountName && a.ProjectName == transferDto.ProjectName);

            // Validate accounts
            if (fromAccount == null || toAccount == null)
            {
                return NotFound(new { Message = "One or both accounts not found in the specified project." });
            }

            if (fromAccount.Amount < transferDto.Amount)
            {
                return BadRequest(new { Message = "Insufficient balance in the source account." });
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectName == transferDto.ProjectName);

            // Authorization check
            if (fromAccount.ManagerEmail != userEmail && toAccount.ManagerEmail != userEmail && project?.CreatedBy != userEmail)
            {
                return Unauthorized(new { Message = "Only account managers or the project admin can perform transfers." });
            }

            // Perform the transfer
            fromAccount.Amount -= transferDto.Amount;
            toAccount.Amount += transferDto.Amount;

            await _context.SaveChangesAsync();
            var Addedremarks="";

            if(fromAccount.AccountName== "Master")
            { Addedremarks = "Budget Allocation";
            }
            else
            {
                Addedremarks = "Party Budget";
            }
            // Log the transfer in the TransactionStatement table
            var transaction = new TransactionStatement
            {
                ProjectName = transferDto.ProjectName,
                FromAccount = transferDto.FromAccountName,
                ToAccount = transferDto.ToAccountName,
                TransactionType = "Internal Transfer",
                Amount = transferDto.Amount,
                PerformedBy = userEmail,
                TransactionDate = DateTime.UtcNow,
                
                Remarks = Addedremarks
            };

            _context.TransactionStatements.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Transfer successful.",
                Transaction = new
                {
                    ProjectName = transaction.ProjectName,
                    FromAccount = transaction.FromAccount,
                    ToAccount = transaction.ToAccount,
                    Amount = transaction.Amount,
                    PerformedBy = transaction.PerformedBy,
                    TransactionDate = transaction.TransactionDate,
                    Remarks = transaction.Remarks
                }
            });
        }


        [HttpGet("transaction-statements")]
        public async Task<IActionResult> GetTransactionStatements()
        {
            // Get the authenticated user's name and email from claims
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            // Check if the user is authenticated properly
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            // Get the account associated with the user (either by ManagerName or ManagerEmail)
            var userAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => (a.ManagerName == userName || a.ManagerEmail == userEmail) && !string.IsNullOrEmpty(a.ProjectName));

            // Check if the user has an associated account with a valid project
            if (userAccount == null)
            {
                return NotFound(new { Message = "No accounts associated with the user found." });
            }

            // Fetch all transaction statements associated with the same ProjectName as the user's account
            var transactions = await _context.TransactionStatements
                .Where(ts => ts.ProjectName == userAccount.ProjectName)
                .ToListAsync();

            // Check if any transactions were found
            if (transactions == null || transactions.Count == 0)
            {
                return NotFound(new { Message = "No transaction statements found for the associated project." });
            }

            // Return the list of transactions
            return Ok(transactions);
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawDto withdrawDto)
        {
            // Validate input
            if (withdrawDto == null || string.IsNullOrEmpty(withdrawDto.AccountName) || withdrawDto.Amount <= 0)
            {
                return BadRequest(new { Message = "Invalid withdrawal request." });
            }

            // Get the authenticated user's email
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { Message = "User is not authenticated." });
            }

            // Retrieve the account details from the database based on the account name and project name
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountName == withdrawDto.AccountName && a.ProjectName == withdrawDto.ProjectName);

            if (account == null)
            {
                return NotFound(new { Message = "Account not found in the specified project." });
            }

            // Check if the account manager is making the request or the Project Admin
            if (account.ManagerEmail != userEmail)
            {
                // Retrieve the project and check if the authenticated user is the project admin
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectName == withdrawDto.ProjectName);
                if (project == null || project.CreatedBy != userEmail)
                {
                    return BadRequest(new { Message = "Only the Account Manager or Project Admin can withdraw from this account." });
                }
            }

            // Ensure that the account being withdrawn from is not a Master account
            //if (account.AccountName == "Master")
            //{
            //    return BadRequest(new { Message = "Master account cannot be withdrawn from." });
            //}

            // Check if the account has enough balance for the withdrawal
            if (account.Amount < withdrawDto.Amount)
            {
                return BadRequest(new { Message = "Insufficient funds." });
            }

            // Perform the withdrawal
            account.Amount -= withdrawDto.Amount;

            // Create a transaction statement for the withdrawal
            var transactionStatement = new TransactionStatement
            {
                PerformedBy = userEmail,
                ProjectName = account.ProjectName,
                TransactionType = "Withdrawal",
                Amount = withdrawDto.Amount,
                FromAccount = account.AccountName,
                ToAccount = "Expenditure", 
                TransactionDate = DateTime.UtcNow,
                Remarks = withdrawDto.Remarks
            };

            

        _context.TransactionStatements.Add(transactionStatement);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Withdrawal successful.",
                AccountName = account.AccountName,
                NewBalance = account.Amount
            });
        }


    }
}
