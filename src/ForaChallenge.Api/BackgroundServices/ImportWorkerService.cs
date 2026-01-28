
using ForaChallenge.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ForaChallenge.Api.BackgroundServices;

/// <summary>
/// Hosted background service that processes queued import jobs.
/// </summary>
public sealed class ImportWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IImportJobQueue _queue;
    private readonly ILogger<ImportWorkerService> _logger;

    public ImportWorkerService(
        IServiceScopeFactory scopeFactory,
        IImportJobQueue queue,
        ILogger<ImportWorkerService> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Import worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            ImportWorkItem item;
            try
            {
                item = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dequeue import work item");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var executor = scope.ServiceProvider.GetRequiredService<IImportJobExecutor>();

                _logger.LogInformation("Executing import job {JobId}", item.JobId);
                await executor.ExecuteAsync(item.JobId, item.ForceReimport, stoppingToken);
                _logger.LogInformation("Finished import job {JobId}", item.JobId);
            }
            catch (OperationCanceledException)
            {
                // Shutdown requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception executing import job {JobId}", item.JobId);
            }
        }

        _logger.LogInformation("Import worker stopped");
    }
}

