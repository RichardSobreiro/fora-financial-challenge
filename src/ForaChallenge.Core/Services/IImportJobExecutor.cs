
namespace ForaChallenge.Core.Services;

/// <summary>
/// Executes an import job that has already been created/persisted.
/// Intended to be invoked from a hosted background service.
/// </summary>
public interface IImportJobExecutor
{
    Task ExecuteAsync(Guid jobId, bool forceReimport, CancellationToken cancellationToken = default);
}

