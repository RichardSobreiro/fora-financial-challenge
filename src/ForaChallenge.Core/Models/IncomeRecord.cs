
namespace ForaChallenge.Core.Models;

/// <summary>
/// Represents a single year's income data for a company.
/// Filtered from EDGAR API to include only 10-K forms with CY (calendar year) frames.
/// </summary>
public class IncomeRecord
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public int Year { get; set; }

    public decimal Income { get; set; }

    public string Form { get; set; } = string.Empty;

    public string Frame { get; set; } = string.Empty;
}

