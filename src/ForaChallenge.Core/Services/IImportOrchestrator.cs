
using ForaChallenge.Core.Models;

namespace ForaChallenge.Core.Services;

/// <summary>
/// Orchestrates EDGAR data import with distributed locking.
/// Ensures only one import runs at a time across all API instances.
/// </summary>
public interface IImportOrchestrator
{
    /// <summary>
    /// Starts a new import job. Returns job ID immediately (non-blocking).
    /// Throws <see cref="InvalidOperationException"/> if an import is already running.
    /// </summary>
    Task<Guid> StartImportAsync(bool forceReimport = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets status of a specific import job by ID.
    /// Returns null if job not found.
    /// </summary>
    Task<ImportJob?> GetImportStatusAsync(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent import job (current if running, or last completed).
    /// Returns null if no jobs exist.
    /// </summary>
    Task<ImportJob?> GetCurrentImportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any import is currently running.
    /// </summary>
    Task<bool> IsImportRunningAsync(CancellationToken cancellationToken = default);
}

