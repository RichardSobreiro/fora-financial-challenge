
namespace ForaChallenge.Core.Services;

/// <summary>
/// Background work queue abstraction for import jobs.
/// API layer should implement this with a hosted service (Channel-based, Hangfire, etc).
/// </summary>
public interface IImportJobQueue
{
    ValueTask EnqueueAsync(ImportWorkItem item, CancellationToken cancellationToken = default);

    ValueTask<ImportWorkItem> DequeueAsync(CancellationToken cancellationToken = default);
}

