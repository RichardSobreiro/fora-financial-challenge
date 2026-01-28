using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;

namespace ForaChallenge.Core.Services;

/// <summary>
/// Service that orchestrates company queries and funding calculations.
/// This keeps business logic and data access orchestration out of the presentation layer.
/// </summary>
public interface ICompanyFundingService
{
    /// <summary>
    /// Queries companies and calculates funding eligibility for each.
    /// Returns a list of company funding results.
    /// </summary>
    Task<List<CompanyFundingResult>> GetCompaniesWithFundingAsync(
        string? nameStartsWith = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates funding eligibility for a company and returns the result DTO.
    /// </summary>
    CompanyFundingResult CalculateFunding(Company company);
}
