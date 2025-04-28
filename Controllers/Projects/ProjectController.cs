using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using WalletApi.Data;
using WalletApi.Models.DBModels;
using WalletApi.Models.DTOS;

namespace WalletApi.Controllers.Projects
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("NewProject")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto projectDto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(projectDto.ProjectName))
            {
                return BadRequest(new { Message = "Project name is required." });
            }

            // Check if a project with the same name already exists
            var existingProject = await _context.Projects
      .FirstOrDefaultAsync(p => p.ProjectName.ToLower() == projectDto.ProjectName.ToLower());


            if (existingProject != null)
            {
                return BadRequest(new { Message = "A project with the same name already exists." });
            }

            // Get the authenticated user's name and role
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            var userRole = "Project Admin";

            // Create the project
            var project = new Project
            {
                ProjectName = projectDto.ProjectName,
                CreatedBy = userName,
                ManagerMail = email,
                Budget = projectDto.Budget
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Automatically create the Master Account
            var masterAccount = new Account
            {
                AccountName = "Master",
                ProjectName = project.ProjectName,
                ManagerEmail = email,
                ManagerName = userName,
                ManagerRole = userRole,
                Amount = (decimal)project.Budget
            };

            _context.Accounts.Add(masterAccount);
            await _context.SaveChangesAsync();
            var transaction = new TransactionStatement
            {
                ProjectName = projectDto.ProjectName,
                FromAccount = "External", // External source of funds
                ToAccount = "Master",
                TransactionType = "Deposit",
                Amount = (decimal)projectDto.Budget,
                PerformedBy = email,
                TransactionDate = DateTime.UtcNow,
                Remarks = "Budget Allocation"
            };

            _context.TransactionStatements.Add(transaction);
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(new
            {
                Message = "Project created successfully.",
                Project = new
                {
                    ProjectId = project.ProjectId,
                    ProjectName = project.ProjectName,
                    CreatedBy = project.CreatedBy
                },
                MasterAccount = new
                {
                    AccountId = masterAccount.AccountId,
                    AccountName = masterAccount.AccountName,
                    ManagerEmail = masterAccount.ManagerEmail,
                    ManagerName = masterAccount.ManagerName,
                    ManagerRole = masterAccount.ManagerRole,
                    Amount = masterAccount.Amount
                }
            });
        }


        [HttpPut("UpdateProjectName")]
        public async Task<IActionResult> UpdateProjectName(int projectId, string newProjectName)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(newProjectName))
            {
                return BadRequest("New project name cannot be empty.");
            }

            // Retrieve the project
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            // Start by updating the Accounts table
            var accountsToUpdate = _context.Accounts.Where(a => a.ProjectName == project.ProjectName).ToList();
            foreach (var account in accountsToUpdate)
            {
                account.ProjectName = newProjectName;
            }

            // Save changes for Accounts table
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating Accounts table: {ex.Message}");
            }

            // Update the PaymentRequests table
            var paymentRequestsToUpdate = _context.PaymentRequests.Where(pr => pr.ProjectName == project.ProjectName).ToList();
            foreach (var paymentRequest in paymentRequestsToUpdate)
            {
                paymentRequest.ProjectName = newProjectName;
            }

            // Save changes for PaymentRequests table
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating PaymentRequests table: {ex.Message}");
            }

            // Update the TransactionStatements table
            var transactionsToUpdate = _context.TransactionStatements.Where(ts => ts.ProjectName == project.ProjectName).ToList();
            foreach (var transaction in transactionsToUpdate)
            {
                transaction.ProjectName = newProjectName;
            }

            // Save changes for TransactionStatements table
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating TransactionStatements table: {ex.Message}");
            }

            // Finally, update the Projects table
            project.ProjectName = newProjectName;

            // Save changes for Projects table
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating Projects table: {ex.Message}");
            }

            return Ok($"Project name updated successfully to '{newProjectName}' in all related tables.");
        }




        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            // Find the project
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound(new { Message = "Project not found." });
            }

            // Get the authenticated user's email
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            // Check if the authenticated user is the creator of the project
            if (!string.Equals(project.ManagerMail, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { Message = "Only the project creator can delete this project." });
            }

            // Find all accounts related to the project
            var accounts = _context.Accounts.Where(a => a.ProjectName == project.ProjectName).ToList();
            var transactionStatements = await _context.TransactionStatements
                 .Where(ts =>  ts.ProjectName == project.ProjectName) // Ensure the projectName matches the project of the account
                 .ToListAsync();
            // Calculate the total amount in all accounts
            var totalAmount = accounts.Sum(a => a.Amount);

            if (totalAmount > 0)
            {
                return BadRequest(new { Message = "Cannot delete the project. Total amount in associated accounts must be zero." });
            }

            // Delete all accounts related to the project
            _context.Accounts.RemoveRange(accounts);
            _context.TransactionStatements.RemoveRange(transactionStatements);
            // Delete the project
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Project deleted successfully." });
        }


        [HttpGet("ProjectDetails")]
        public async Task<IActionResult> GetProjectDetails()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { Message = "User is not authenticated" });
            }

            // Fetch all accounts associated with the user
            var userAccounts = await _context.Accounts
                .Where(a => a.ManagerName == userName)
                .ToListAsync();

            if (!userAccounts.Any())
            {
                return NotFound(new { Message = "No accounts found for the user" });
            }

            // Get the project names from the accounts
            var projectNames = userAccounts.Select(a => a.ProjectName).Distinct().ToList();

            // Prepare response data
            var response = new List<object>();

            foreach (var projectName in projectNames)
            {
                // Fetch project details
                var project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.ProjectName == projectName);

                if (project == null)
                    continue;

                // Fetch accounts related to this project
                var relatedAccounts = await _context.Accounts
                    .Where(a => a.ProjectName == projectName)
                    .ToListAsync();

                // Fetch the latest 10 transactions for this project
                var recentTransactions = await _context.TransactionStatements
                    .Where(t => t.ProjectName == projectName)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                // Add to response
                response.Add(new
                {
                    ProjectDetails = project,
                    RelatedAccounts = relatedAccounts,
                    RecentTransactions = recentTransactions
                });
            }

            return Ok(response);
        }

        [HttpGet("GetAccountstatement")]
        public async Task<IActionResult> GetManagerDetails(string managerName, string accountName, string projectName)
        {
            if (string.IsNullOrEmpty(managerName) || string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(projectName))
                return BadRequest("ManagerName, AccountName, and ProjectName are required.");

            try
            {
                // Fetch the account managed by the given manager
                var account = await _context.Accounts
                    .Where(a => a.ManagerName == managerName && a.AccountName == accountName && a.ProjectName == projectName)
                    .FirstOrDefaultAsync();

                if (account == null)
                    return NotFound("Account not found for the given manager and project.");

                // Fetch associated transactions where the accountName matches FromAccount or ToAccount, 
                // and the projectName matches the account's projectName
                var transactionStatements = await _context.TransactionStatements
                    .Where(ts => (ts.FromAccount == account.AccountName || ts.ToAccount == account.AccountName) &&
                                 (ts.ProjectName == projectName)) // Ensure the projectName matches the project of the account
                    .ToListAsync();

                // Fetch project details based on the provided projectName
                var project = await _context.Projects
                    .Where(p => p.ProjectName == projectName)
                    .FirstOrDefaultAsync();

                if (project == null)
                    return NotFound("Project not found.");

                // Prepare the result with project details and associated transactions for the account
                var result = new
                {
                    ProjectId = project.ProjectId,
                    ProjectName = project.ProjectName,
                    CreatedBy = project.CreatedBy,
                    ManagerMail = project.ManagerMail,
                    Budget = project.Budget,
                    Transactions = transactionStatements
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }





    }
}
