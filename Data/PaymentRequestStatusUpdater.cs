using Microsoft.EntityFrameworkCore;

namespace WalletApi.Data
{
    
        public class PaymentRequestStatusUpdater : IHostedService, IDisposable
        {
            private Timer _timer;
            private readonly IServiceScopeFactory _serviceScopeFactory;

            public PaymentRequestStatusUpdater(IServiceScopeFactory serviceScopeFactory)
            {
                _serviceScopeFactory = serviceScopeFactory;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                // Start a timer that checks every 1 minute
                _timer = new Timer(UpdateExpiredRequests, null, TimeSpan.Zero, TimeSpan.FromMinutes(0.5));
                return Task.CompletedTask;
            }

            private async void UpdateExpiredRequests(object state)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Find requests that are Pending and older than 30 minutes
                    var requestsToExpire = await dbContext.PaymentRequests
                        .Where(r => r.Status == "Pending" && r.RequestedOn <= DateTime.UtcNow.AddMinutes(-0.5))
                        .ToListAsync();

                    // Update status to "Expired"
                    foreach (var request in requestsToExpire)
                    {
                        request.Status = "Expired";
                    }

                    // Save changes to the database
                    if (requestsToExpire.Any())
                    {
                        dbContext.PaymentRequests.UpdateRange(requestsToExpire);
                        await dbContext.SaveChangesAsync();
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
