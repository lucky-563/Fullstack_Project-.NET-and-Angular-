namespace WalletApi.Models.DTOS
{
    public class TransferDto
    {
        public string ProjectName { get; set; }
        public string FromAccountName { get; set; }
        public string ToAccountName { get; set; }
        public decimal Amount { get; set; }
    }
}
