
using ForaChallenge.Core.Enums;
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;
using Microsoft.Extensions.Logging;

namespace ForaChallenge.Core.Services;

/// <summary>
/// Orchestrates EDGAR data import: creates/persists a job + acquires the distributed lock,
/// then enqueues work to be executed by a hosted background service.
/// </summary>
public class ImportOrchestrator : IImportOrchestrator
{
    private readonly ICompanyRepository _repository;
    private readonly IImportJobQueue _queue;
    private readonly ILogger<ImportOrchestrator> _logger;

    public ImportOrchestrator(
        ICompanyRepository repository,
        IImportJobQueue queue,
        ILogger<ImportOrchestrator> logger)
    {
        _repository = repository;
        _queue = queue;
        _logger = logger;
    }

    public async Task<Guid> StartImportAsync(bool forceReimport = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to start import (force={ForceReimport})", forceReimport);

        var jobId = await AcquireDistributedLockAndCreateJobAsync(cancellationToken);

        try
        {
            await _queue.EnqueueAsync(new ImportWorkItem(jobId, forceReimport), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue import job {JobId}", jobId);

            var job = await _repository.GetImportJobAsync(jobId, cancellationToken);
            if (job != null)
            {
                job.Status = ImportStatus.Failed;
                job.CompletedAt = DateTime.UtcNow;
                job.ErrorMessage = "Failed to enqueue import work item.";
                await _repository.UpdateImportJobAsync(job, cancellationToken);
            }

            // Best-effort unlock if enqueue fails.
            var lockRecord = await _repository.GetImportLockAsync(cancellationToken);
            if (lockRecord?.CurrentJobId == jobId)
            {
                lockRecord.CurrentJobId = null;
                lockRecord.LockedAt = null;
                lockRecord.LockedByInstance = null;
                await _repository.UpdateImportLockAsync(lockRecord, cancellationToken);
            }

            throw;
        }

        _logger.LogInformation("Import job {JobId} enqueued successfully", jobId);
        return jobId;
    }

    public async Task<ImportJob?> GetImportStatusAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetImportJobAsync(jobId, cancellationToken);
    }

    public async Task<ImportJob?> GetCurrentImportAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetLatestImportJobAsync(cancellationToken);
    }

    public async Task<bool> IsImportRunningAsync(CancellationToken cancellationToken = default)
    {
        var lockRecord = await _repository.GetImportLockAsync(cancellationToken);
        return lockRecord?.CurrentJobId.HasValue ?? false;
    }

    private async Task<Guid> AcquireDistributedLockAndCreateJobAsync(CancellationToken cancellationToken)
    {
        var lockRecord = await _repository.GetImportLockAsync(cancellationToken) ?? new ImportLock { Id = 1 };

        if (lockRecord.CurrentJobId.HasValue)
        {
            var runningJob = await _repository.GetImportJobAsync(lockRecord.CurrentJobId.Value, cancellationToken);
            if (runningJob is { Status: ImportStatus.Running or ImportStatus.Queued })
            {
                throw new InvalidOperationException(
                    $"An import is already running (Job ID: {lockRecord.CurrentJobId}). Only one import can run at a time.");
            }

            _logger.LogWarning("Detected stale lock from Job ID: {JobId}. Clearing and proceeding.", lockRecord.CurrentJobId);
        }

        var jobId = Guid.NewGuid();
        lockRecord.CurrentJobId = jobId;
        lockRecord.LockedAt = DateTime.UtcNow;
        lockRecord.LockedByInstance = Environment.MachineName;

        var job = new ImportJob
        {
            Id = jobId,
            Status = ImportStatus.Queued,
            StartedAt = DateTime.UtcNow,
            TotalCompanies = 0,
            ProcessedCompanies = 0,
            SuccessfulCompanies = 0,
            FailedCompanies = 0,
            LockedByInstance = Environment.MachineName,
            IsStartupImport = false
        };

        await _repository.UpdateImportLockAsync(lockRecord, cancellationToken);
        await _repository.SaveImportJobAsync(job, cancellationToken);

        return jobId;
    }
}

