using Microsoft.AspNetCore.Identity;

namespace WalletApi.Models.DBModels
{
    public class Account
    {
        public  int  AccountId { get; set; }
        public string AccountName { get; set; }
        public string ProjectName { get; set; }
        public string ManagerEmail { get; set; }
        public string ManagerName { get; set; }   
        public string ManagerRole { get; set; }
        public decimal Amount { get; set; }

     
    }
}
