
namespace ForaChallenge.Infrastructure.Data.Entities;

public class IncomeRecordEntity
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public int Year { get; set; }

    public decimal Income { get; set; }

    public string Form { get; set; } = string.Empty;

    public string Frame { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public CompanyEntity Company { get; set; } = null!;
}

