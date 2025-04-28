namespace WalletApi.Models.DBModels
{
    public class TransactionStatement
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string PerformedBy { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Remarks { get; set; }
    }
}

