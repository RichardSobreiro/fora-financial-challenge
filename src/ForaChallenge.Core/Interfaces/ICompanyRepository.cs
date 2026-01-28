
using ForaChallenge.Core.Models;

namespace ForaChallenge.Core.Interfaces;

/// <summary>
/// Repository for company data operations.
/// Abstraction over data persistence layer.
/// </summary>
public interface ICompanyRepository
{
    Task<List<Company>> GetCompaniesAsync(string? nameStartsWith = null, CancellationToken cancellationToken = default);

    Task<Company?> GetCompanyByCikAsync(int cik, CancellationToken cancellationToken = default);

    Task<int> GetCompanyCountAsync(CancellationToken cancellationToken = default);

    Task AddCompanyAsync(Company company, CancellationToken cancellationToken = default);

    Task UpdateCompanyAsync(Company company, CancellationToken cancellationToken = default);

    Task<ImportJob?> GetImportJobAsync(Guid jobId, CancellationToken cancellationToken = default);

    Task<ImportJob?> GetLatestImportJobAsync(CancellationToken cancellationToken = default);

    Task SaveImportJobAsync(ImportJob job, CancellationToken cancellationToken = default);

    Task UpdateImportJobAsync(ImportJob job, CancellationToken cancellationToken = default);

    Task<ImportLock?> GetImportLockAsync(CancellationToken cancellationToken = default);

    Task UpdateImportLockAsync(ImportLock importLock, CancellationToken cancellationToken = default);
}

