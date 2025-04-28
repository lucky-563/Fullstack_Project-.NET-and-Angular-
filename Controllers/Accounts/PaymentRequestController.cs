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
    public class PaymentRequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

      


        [HttpGet("GetAllRequestsWithProjectDetails")]
        public async Task<IActionResult> GetAllRequestsWithProjectDetails()
        {
            try
            {
                var result = await _context.Set<Project>()
                    .Select(project => new
                    {
                        Project = new
                        {
                            project.ProjectId,
                            project.ProjectName,
                            project.CreatedBy,
                            project.ManagerMail,
                            project.Budget
                        },
                        PaymentRequests = _context.Set<PaymentRequest>()
                            .Where(r => r.ProjectName == project.ProjectName)
                            .Select(request => new
                            {
                                request.PaymentRequestId,
                                request.FromAccountName,
                                request.ToAccountName,
                                request.Amount,
                                request.RequestedByEmail,
                                request.RequestedOn,
                                request.ActionByEmail,
                                request.requestToManger,
                                request.ActionOn,
                                request.Status
                            }).ToList()
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPost("CreatePaymentRequest")]
        public async Task<IActionResult> CreatePaymentRequest([FromBody] CreatePaymentRequestDto requestDto)
        {
            // Validate input
            if (requestDto.Amount <= 0)
            {
                return BadRequest(new { Message = "Amount must be greater than zero." });
            }

            // Validate FromAccount and ToAccount
            var fromAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountName == requestDto.FromAccountName && a.ProjectName == requestDto.ProjectName);

            var toAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountName == requestDto.ToAccountName && a.ProjectName == requestDto.ProjectName);

            
            if (fromAccount == null || toAccount == null)
            {
                return NotFound(new { Message = "One or both accounts do not exist in the specified project." });
            }

            if (fromAccount.AccountName == toAccount.AccountName)
            {
                return BadRequest(new { Message = "FromAccount and ToAccount cannot be the same." });
            }
            var requestToManager = fromAccount.ManagerName;
            // Get the authenticated user's email
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            // Ensure the user is the manager of the FromAccount
            if (toAccount.ManagerEmail != userEmail)
            {
                return BadRequest(new { Message = "You are not authorized to make payment requests from this account." });
            }

            // Create the payment request
            var paymentRequest = new PaymentRequest
            {
                FromAccountName = requestDto.FromAccountName,
                ToAccountName = requestDto.ToAccountName,
                ProjectName = requestDto.ProjectName,
                Amount = requestDto.Amount,
                RequestedByEmail = userEmail,
                requestToManger= requestToManager,
                RequestedOn = DateTime.UtcNow,
                Status = "Pending"
            };

            // Save the payment request to the database
            _context.PaymentRequests.Add(paymentRequest);
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(new
            {
                Message = "Payment request created successfully.",
                PaymentRequest = new
                {
                    PaymentRequestId = paymentRequest.PaymentRequestId,
                    FromAccountName = paymentRequest.FromAccountName,
                    ToAccountName = paymentRequest.ToAccountName,
                    ProjectName = paymentRequest.ProjectName,
                    Amount = paymentRequest.Amount,
                    RequestedByEmail = paymentRequest.RequestedByEmail,
                    RequestedOn = paymentRequest.RequestedOn,
                    Status = paymentRequest.Status
                }
            });
        }




        [HttpPost("ApprovePaymentRequest")]
        public async Task<IActionResult> ApprovePaymentRequest([FromBody] int paymentRequestId)
        {
            var paymentRequest = await _context.PaymentRequests
                .FirstOrDefaultAsync(pr => pr.PaymentRequestId == paymentRequestId);

            if (paymentRequest == null)
            {
                return NotFound(new { Message = "Payment request not found." });
            }

            if (paymentRequest.Status != "Pending")
            {
                return BadRequest(new { Message = "Only pending payment requests can be approved." });
            }

            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            var toAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountName == paymentRequest.ToAccountName && a.ProjectName == paymentRequest.ProjectName);

            var fromAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountName == paymentRequest.FromAccountName && a.ProjectName == paymentRequest.ProjectName);

            if (toAccount == null || fromAccount == null)
            {
                return NotFound(new { Message = "Associated accounts not found." });
            }

            if (fromAccount.Amount < paymentRequest.Amount)
            {
                return BadRequest(new { Message = "Insufficient funds in the FromAccount to fulfill the request." });
            }

            fromAccount.Amount -= paymentRequest.Amount;
            toAccount.Amount += paymentRequest.Amount;

            paymentRequest.Status = "Approved";
            paymentRequest.ActionByEmail = userEmail;
            paymentRequest.ActionOn = DateTime.UtcNow;

            _context.PaymentRequests.Update(paymentRequest);

            var transactionStatement = new TransactionStatement
            {
                ProjectName = paymentRequest.ProjectName,
                FromAccount = paymentRequest.FromAccountName,
                ToAccount = paymentRequest.ToAccountName,
                TransactionType = "PaymentRequestApproval",
                Amount = paymentRequest.Amount,
                PerformedBy = userEmail,
                TransactionDate = DateTime.UtcNow,
                Remarks = $"Payment request approved by {userEmail}."
            };

            _context.TransactionStatements.Add(transactionStatement);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Payment request approved successfully.",
                TransactionStatement = new
                {
                    Id = transactionStatement.Id,
                    ProjectName = transactionStatement.ProjectName,
                    FromAccount = transactionStatement.FromAccount,
                    ToAccount = transactionStatement.ToAccount,
                    TransactionType = transactionStatement.TransactionType,
                    Amount = transactionStatement.Amount,
                    PerformedBy = transactionStatement.PerformedBy,
                    TransactionDate = transactionStatement.TransactionDate,
                    Remarks = transactionStatement.Remarks
                }
            });
        }



        [HttpPost("RejectPaymentRequest")]
        public async Task<IActionResult> RejectPaymentRequest([FromBody] int paymentRequestId)
        {
            var paymentRequest = await _context.PaymentRequests
                .FirstOrDefaultAsync(pr => pr.PaymentRequestId == paymentRequestId);

            if (paymentRequest == null)
            {
                return NotFound(new { Message = "Payment request not found." });
            }

            if (paymentRequest.Status != "Pending")
            {
                return BadRequest(new { Message = "Only pending payment requests can be rejected." });
            }

            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { Message = "User is not authorized." });
            }

            paymentRequest.Status = "Rejected";
            paymentRequest.ActionByEmail = userEmail;
            paymentRequest.ActionOn = DateTime.UtcNow;

            _context.PaymentRequests.Update(paymentRequest);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Payment request rejected successfully.",
                PaymentRequest = new
                {
                    paymentRequest.PaymentRequestId,
                    paymentRequest.FromAccountName,
                    paymentRequest.ToAccountName,
                    paymentRequest.Amount,
                    paymentRequest.Status,
                    paymentRequest.ActionByEmail,
                    paymentRequest.ActionOn,
                }
            });
        }

    }
}
