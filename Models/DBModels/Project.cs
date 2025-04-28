namespace WalletApi.Models.DBModels
{
    public class Project
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string CreatedBy { get; set; }

        public string ManagerMail { get; set; }

        public decimal?  Budget { get; set; }
    }
}
