
namespace ForaChallenge.Core.Models;

/// <summary>
/// API response DTO for company funding eligibility.
/// Matches the exact format specified in the challenge requirements.
/// </summary>
public class CompanyFundingResult
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal StandardFundableAmount { get; set; }

    public decimal SpecialFundableAmount { get; set; }
}

