

namespace WalletApi.Models.DBModels
{
    public class PaymentRequest
    {
        public int PaymentRequestId { get; set; } // Unique ID for the request

        public string FromAccountName { get; set; } // The account making the request
        public string ToAccountName { get; set; }   // The target account for the request

        public string ProjectName { get; set; } // The project the accounts belong to

        public decimal Amount { get; set; } // Amount requested to transfer

        public string RequestedByEmail { get; set; } // The email of the person who made the request
        public DateTime RequestedOn { get; set; } = DateTime.UtcNow; // Request date/time
        public string? requestToManger {  get; set; }
        public string? ActionByEmail { get; set; } // The email of the user who approved/rejected the request
        public DateTime? ActionOn { get; set; } // Date/time of approval/rejection

        public string Status { get; set; } = "Pending"; // Status: Pending, Approved, Rejected
    }
}
