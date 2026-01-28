
namespace ForaChallenge.Core.Models;

/// <summary>
/// Represents a company in our domain model.
/// This is our internal representation, independent of external APIs or database structure.
/// </summary>
public class Company
{
    public int Id { get; set; }

    public int Cik { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<IncomeRecord> IncomeRecords { get; set; } = new();
}

