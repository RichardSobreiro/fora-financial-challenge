namespace ForaChallenge.Core.Services;

/// <summary>
/// Processes a single company (CIK): fetch from EDGAR and upsert into persistence.
/// Intended to be resolved as a scoped service (DbContext is not thread-safe).
/// </summary>
public interface ICompanyImportProcessor
{
    Task ProcessCompanyAsync(int cik, CancellationToken cancellationToken = default);
}

