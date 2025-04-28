namespace WalletApi.Models.Constants
{
   
    public enum PaymentStatus
    {
        Pending,
        Approved,
        Rejected,
        Expired
    }


    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        DirectTransfer,
        ApprovedPaymentRequest
    }
}
