
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Services;
using Microsoft.Extensions.Hosting;

namespace ForaChallenge.Api.BackgroundServices;

/// <summary>
/// Hosted service that automatically starts an import on first application startup if no data exists.
/// Runs once and completes.
/// </summary>
public class StartupImportService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StartupImportService> _logger;

    public StartupImportService(IServiceProvider serviceProvider, ILogger<StartupImportService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Delay to ensure database migrations have completed
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        _logger.LogInformation("Checking if initial data import is needed...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
            var orchestrator = scope.ServiceProvider.GetRequiredService<IImportOrchestrator>();

            var existingCount = await repository.GetCompanyCountAsync(cancellationToken);
            if (existingCount == 0)
            {
                _logger.LogInformation("No data found. Starting automatic import...");
                var jobId = await orchestrator.StartImportAsync(forceReimport: false, cancellationToken);
                _logger.LogInformation("Automatic import enqueued with Job ID: {JobId}", jobId);
            }
            else
            {
                _logger.LogInformation("Database already contains {Count} companies. Skipping automatic import.", existingCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check or start automatic import on startup");
            // Don't rethrow - we don't want startup failures to crash the application
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to clean up - this service completes immediately after StartAsync
        return Task.CompletedTask;
    }
}

