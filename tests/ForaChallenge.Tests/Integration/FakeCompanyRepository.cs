using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;

namespace ForaChallenge.Tests.Integration;

/// <summary>
/// Minimal test double to allow API boot without a real database.
/// </summary>
public sealed class FakeCompanyRepository : ICompanyRepository
{
    public Task<List<Company>> GetCompaniesAsync(string? nameStartsWith = null, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<Company>());

    public Task<Company?> GetCompanyByCikAsync(int cik, CancellationToken cancellationToken = default)
        => Task.FromResult<Company?>(null);

    public Task<int> GetCompanyCountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(0);

    public Task AddCompanyAsync(Company company, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task UpdateCompanyAsync(Company company, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<ImportJob?> GetImportJobAsync(Guid jobId, CancellationToken cancellationToken = default)
        => Task.FromResult<ImportJob?>(null);

    public Task<ImportJob?> GetLatestImportJobAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<ImportJob?>(null);

    public Task SaveImportJobAsync(ImportJob job, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task UpdateImportJobAsync(ImportJob job, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<ImportLock?> GetImportLockAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<ImportLock?>(null);

    public Task UpdateImportLockAsync(ImportLock importLock, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

