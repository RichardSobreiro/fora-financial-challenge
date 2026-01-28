
using ForaChallenge.Core.Models;

namespace ForaChallenge.Core.Interfaces;

/// <summary>
/// Abstraction for fetching company data from external sources.
/// Domain doesn't care where data comes from - could be EDGAR, a file, a mock, etc.
/// </summary>
public interface IEdgarDataProvider
{
    /// <summary>
    /// Fetches company data by CIK and maps it to our domain model.
    /// </summary>
    Task<Company?> GetCompanyDataAsync(int cik, CancellationToken cancellationToken = default);
}

