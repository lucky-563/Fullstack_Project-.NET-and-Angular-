
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WalletApi.Models.DBModels;

namespace WalletApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<TransactionStatement> TransactionStatements { get; set; }

        public DbSet<PaymentRequest> PaymentRequests { get; set; }
    }
}
