
namespace ForaChallenge.Core.Services;

/// <summary>
/// Represents a background import work item to be processed by a hosted service.
/// </summary>
/// <param name="JobId">The import job id persisted in the database.</param>
/// <param name="ForceReimport">Whether to force re-import behavior (future enhancement).</param>
public readonly record struct ImportWorkItem(Guid JobId, bool ForceReimport);

