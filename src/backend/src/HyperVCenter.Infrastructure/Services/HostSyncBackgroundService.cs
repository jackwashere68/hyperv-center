using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Application.Features.HyperVHosts.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HyperVCenter.Infrastructure.Services;

public class HostSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HostSyncBackgroundService> _logger;
    private readonly int _intervalSeconds;

    public HostSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<HostSyncBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalSeconds = int.TryParse(configuration["HyperV:SyncIntervalSeconds"], out var interval) ? interval : 60;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HostSyncBackgroundService started. Interval: {Interval}s", _intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var hostIds = await context.HyperVHosts
                    .AsNoTracking()
                    .Select(h => h.Id)
                    .ToListAsync(stoppingToken);

                foreach (var hostId in hostIds)
                {
                    try
                    {
                        await mediator.Send(new SyncHyperVHostCommand(hostId), stoppingToken);
                        _logger.LogDebug("Synced host {HostId}", hostId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to sync host {HostId}", hostId);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in host sync loop");
            }
        }
    }
}
