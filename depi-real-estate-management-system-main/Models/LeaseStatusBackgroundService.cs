using depi_real_state_management_system.Models;

public class LeaseStatusBackgroundService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceScopeFactory _scopeFactory;

    public LeaseStatusBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Set the timer to trigger every 24 hours
        _timer = new Timer(CheckLeaseStatus, null, TimeSpan.Zero, TimeSpan.FromHours(24));
        return Task.CompletedTask;
    }

    private void CheckLeaseStatus(object state)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get all ongoing leases that have passed the EndDate
            var ongoingLeases = context.Leases
                .Where(l => l.Status == "Ongoing" && l.EndDate <= DateTime.Now)
                .ToList();

            // Update the status to "Completed" for those leases
            foreach (var lease in ongoingLeases)
            {
                lease.Status = "Completed";
                context.Leases.Update(lease);
            }

            context.SaveChanges();
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
