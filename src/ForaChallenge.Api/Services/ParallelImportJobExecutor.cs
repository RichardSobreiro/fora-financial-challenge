using ForaChallenge.Api.Options;
using ForaChallenge.Core.Enums;
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;
using ForaChallenge.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ForaChallenge.Api.Services;

/// <summary>
/// Executes an import job using bounded parallelism. Each company is processed in its own DI scope
/// to avoid sharing EF Core DbContext across threads.
/// </summary>
public sealed class ParallelImportJobExecutor : IImportJobExecutor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICikProvider _cikProvider;
    private readonly ImportOptions _importOptions;
    private readonly ILogger<ParallelImportJobExecutor> _logger;

    public ParallelImportJobExecutor(
        IServiceScopeFactory scopeFactory,
        ICikProvider cikProvider,
        IOptions<ImportOptions> importOptions,
        ILogger<ParallelImportJobExecutor> logger)
    {
        _scopeFactory = scopeFactory;
        _cikProvider = cikProvider;
        _importOptions = importOptions.Value;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid jobId, bool forceReimport, CancellationToken cancellationToken = default)
    {
        _ = forceReimport; // reserved for future enhancement (clear existing before import)

        await using var managementScope = _scopeFactory.CreateAsyncScope();
        var repository = managementScope.ServiceProvider.GetRequiredService<ICompanyRepository>();

        var job = await repository.GetImportJobAsync(jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} not found");

        var ciks = _cikProvider.GetCiks();
        if (ciks.Count == 0)
        {
            throw new InvalidOperationException("No CIKs configured for import. Configure Import:Ciks in appsettings.json.");
        }

        var maxDop = Math.Max(1, _importOptions.MaxDegreeOfParallelism);
        var progressEvery = TimeSpan.FromSeconds(Math.Max(1, _importOptions.ProgressUpdateIntervalSeconds));

        var processed = 0;
        var success = 0;
        var failed = 0;

        using var progressCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        async Task PersistProgressAsync()
        {
            try
            {
                using var timer = new PeriodicTimer(progressEvery);
                while (await timer.WaitForNextTickAsync(progressCts.Token))
                {
                    job.ProcessedCompanies = Volatile.Read(ref processed);
                    job.SuccessfulCompanies = Volatile.Read(ref success);
                    job.FailedCompanies = Volatile.Read(ref failed);

                    await repository.UpdateImportJobAsync(job, progressCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Progress persistence loop failed for job {JobId}", jobId);
            }
        }

        var progressTask = PersistProgressAsync();

        try
        {
            job.Status = ImportStatus.Running;
            job.TotalCompanies = ciks.Count;
            job.ErrorMessage = null;
            job.CompletedAt = null;
            job.ProcessedCompanies = 0;
            job.SuccessfulCompanies = 0;
            job.FailedCompanies = 0;
            await repository.UpdateImportJobAsync(job, cancellationToken);

            await Parallel.ForEachAsync(
                ciks,
                new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = maxDop
                },
                async (cik, ct) =>
                {
                    try
                    {
                        await using var itemScope = _scopeFactory.CreateAsyncScope();
                        var processor = itemScope.ServiceProvider.GetRequiredService<ICompanyImportProcessor>();
                        await processor.ProcessCompanyAsync(cik, ct);
                        Interlocked.Increment(ref success);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failed);
                        _logger.LogError(ex, "Failed to process CIK {Cik}", cik);
                    }
                    finally
                    {
                        Interlocked.Increment(ref processed);
                    }
                });

            job.Status = ImportStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            job.ProcessedCompanies = Volatile.Read(ref processed);
            job.SuccessfulCompanies = Volatile.Read(ref success);
            job.FailedCompanies = Volatile.Read(ref failed);
            await repository.UpdateImportJobAsync(job, cancellationToken);

            _logger.LogInformation(
                "Import job {JobId} completed. Success: {Success}, Failed: {Failed}",
                jobId, job.SuccessfulCompanies, job.FailedCompanies);
        }
        catch (OperationCanceledException)
        {
            job.Status = ImportStatus.Cancelled;
            job.CompletedAt = DateTime.UtcNow;
            job.ErrorMessage = "Import cancelled.";
            job.ProcessedCompanies = Volatile.Read(ref processed);
            job.SuccessfulCompanies = Volatile.Read(ref success);
            job.FailedCompanies = Volatile.Read(ref failed);
            await repository.UpdateImportJobAsync(job, CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            job.Status = ImportStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            job.ErrorMessage = ex.Message;
            job.ProcessedCompanies = Volatile.Read(ref processed);
            job.SuccessfulCompanies = Volatile.Read(ref success);
            job.FailedCompanies = Volatile.Read(ref failed);
            await repository.UpdateImportJobAsync(job, CancellationToken.None);
            throw;
        }
        finally
        {
            progressCts.Cancel();
            await progressTask;
            await ReleaseDistributedLockAsync(repository, jobId);
        }
    }

    private async Task ReleaseDistributedLockAsync(ICompanyRepository repository, Guid jobId)
    {
        try
        {
            var lockRecord = await repository.GetImportLockAsync(CancellationToken.None);
            if (lockRecord?.CurrentJobId == jobId)
            {
                lockRecord.CurrentJobId = null;
                lockRecord.LockedAt = null;
                lockRecord.LockedByInstance = null;

                await repository.UpdateImportLockAsync(lockRecord, CancellationToken.None);
                _logger.LogInformation("Released distributed lock for Job ID: {JobId}", jobId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release lock for Job ID: {JobId}", jobId);
        }
    }
}

