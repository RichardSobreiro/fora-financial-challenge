
namespace ForaChallenge.Infrastructure.Data.Entities;

/// <summary>
/// EF Core entity for Companies table.
/// Separate from domain model to allow independent evolution.
/// </summary>
public class CompanyEntity
{
    public int Id { get; set; }

    public int Cik { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<IncomeRecordEntity> IncomeRecords { get; set; } = new List<IncomeRecordEntity>();
}

