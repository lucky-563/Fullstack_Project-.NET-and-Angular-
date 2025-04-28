using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    public class AccountController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

      

        public AccountController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto accountDto)
        {
            // Get the authenticated user's name
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }
            var user = await _userManager.FindByNameAsync(accountDto.ManagerName);
            var useremailid = user.Email;

         

            // Validate the input for account creation
            if (string.IsNullOrWhiteSpace(accountDto.ProjectName) || string.IsNullOrWhiteSpace(accountDto.AccountName))
            {
                return BadRequest(new { Message = "Account name and project name are required." });
            }

            // Get the project by name
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectName == accountDto.ProjectName);

            if (project == null)
            {
                return NotFound(new { Message = "Project not found." });
            }

            // Check if the logged-in user is the creator of the project
            if (project.CreatedBy != userName)
            {
                return Unauthorized(new { Message = "You are not authorized to create accounts for this project." });
            }

            // Create the account
            var account = new Account
            {
                AccountName = accountDto.AccountName,
                ProjectName = accountDto.ProjectName,
                ManagerEmail = useremailid,
                ManagerName = accountDto.ManagerName,
                ManagerRole = "Account Manager",  // Default role (can be adjusted if needed)
                Amount = 0  // If no amount is specified, default to 0
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Account created successfully.",
                Account = new
                {
                    AccountId = account.AccountId,
                    AccountName = account.AccountName,
                    ManagerEmail = account.ManagerEmail,
                    ManagerName = account.ManagerName,
                    ManagerRole = account.ManagerRole,
                    Amount = account.Amount,
                    ProjectName = account.ProjectName
                }
            });
        }


        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _context.Accounts.ToListAsync();
            return Ok(accounts);
        }

       

        [HttpPut("{accountId}")]
        public async Task<IActionResult> UpdateAccount(int accountId, [FromBody] Account accountDto)
        {
            // Get the authenticated user's name
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            // Find the account
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
            {
                return NotFound(new { Message = "Account not found." });
            }

            // Get the project associated with the account
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectName == account.ProjectName);

            if (project == null)
            {
                return NotFound(new { Message = "Project not found." });
            }

            // Check if the logged-in user is the creator of the project
            if (project.CreatedBy != userName)
            {
                return Unauthorized(new { Message = "You are not authorized to update this account." });
            }

            // Update the account properties
            account.AccountName = accountDto.AccountName;
            account.Amount = accountDto.Amount;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Account updated successfully." });
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            // Get the authenticated user's name
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            // Find the account by its id
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == id);

            // Check if the account exists
            if (account == null)
            {
                return NotFound(new { Message = "Account not found." });
            }

            // Check if the account's amount is zero before deletion
            if (account.Amount != 0)
            {
                return BadRequest(new { Message = "Cannot delete account because the Amount is not zero (0) in the Account." });
            }

            // Get the project associated with this account
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectName == account.ProjectName);

            if (project == null)
            {
                return NotFound(new { Message = "Project associated with this account not found." });
            }

            // Ensure that the user who created the project is the one attempting to delete the account
            if (project.CreatedBy != userName)
            {
                return Unauthorized(new { Message = "You are not authorized to delete accounts for this project." });
            }

            // Remove the account from the database
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("ChangeManagerDetails")]
        public async Task<IActionResult> ChangeManagerDetails(int accountId, string newManagerName)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(newManagerName))
            {
                return BadRequest("New manager name cannot be empty.");
            }

            // Retrieve the account
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                return NotFound($"Account with ID {accountId} not found.");
            }

            // Retrieve the new manager's details from the Identity table
            var newManagerUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == newManagerName);
            if (newManagerUser == null)
            {
                return NotFound($"User with username '{newManagerName}' not found.");
            }

            // Update the manager details
            account.ManagerName = newManagerName;
            account.ManagerEmail = newManagerUser.Email;

            // Save changes to the Accounts table
            try
            {
                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating manager details: {ex.Message}");
            }

            return Ok($"Manager details updated successfully. New Name: '{newManagerName}', New Email: '{newManagerUser.Email}' for Account ID {accountId}.");
        }



    }
}



