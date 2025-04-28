namespace WalletApi.Models.DTOS
{
    public class WithdrawDto
    {
        public string AccountName { get; set; }   // Account name from which money is being withdrawn
        public decimal Amount { get; set; }        // Amount to withdraw
        public string ProjectName { get; set; }

        public string Remarks { get; set; }
    }
}
