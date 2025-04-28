namespace WalletApi.Models.DTOS
{
    public class DepositDto
    {
        public string ProjectName { get; set; } 
        public string AccountName { get; set; } 
        public decimal Amount { get; set; }
    }
}
