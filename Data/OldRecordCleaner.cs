using Microsoft.EntityFrameworkCore;

namespace WalletApi.Data
{
    public class OldRecordCleaner : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OldRecordCleaner(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Start a timer that triggers cleanup once a day
            _timer = new Timer(CleanupOldRecords, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void CleanupOldRecords(object state)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                try
                {
                    // Define the cutoff date
                    var cutoffDate = DateTime.UtcNow.AddYears(-1);

                    // Delete old transactions
                    var oldTransactions = await dbContext.TransactionStatements
                        .Where(ts => ts.TransactionDate <= cutoffDate)
                        .ToListAsync();

                    if (oldTransactions.Any())
                    {
                        dbContext.TransactionStatements.RemoveRange(oldTransactions);
                    }

                    // Delete old payment requests
                    var oldPaymentRequests = await dbContext.PaymentRequests
                        .Where(pr => pr.RequestedOn <= cutoffDate)
                        .ToListAsync();

                    if (oldPaymentRequests.Any())
                    {
                        dbContext.PaymentRequests.RemoveRange(oldPaymentRequests);
                    }

                    // Save changes to the database
                    if (oldTransactions.Any() || oldPaymentRequests.Any())
                    {
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (use a logger instead of Console.WriteLine in production)
                    Console.WriteLine($"Error occurred during cleanup: {ex.Message}");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
