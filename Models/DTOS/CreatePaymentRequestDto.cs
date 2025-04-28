namespace WalletApi.Models.DTOS
{
    public class CreatePaymentRequestDto
    {

        public string FromAccountName { get; set; }
        public string ToAccountName { get; set; }
        public string ProjectName { get; set; }
        public decimal Amount { get; set; }
    }
}
