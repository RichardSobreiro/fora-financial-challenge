
using ForaChallenge.Core.Enums;

namespace ForaChallenge.Infrastructure.Data.Entities;

public class ImportJobEntity
{
    public Guid Id { get; set; }

    public ImportStatus Status { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int TotalCompanies { get; set; }

    public int ProcessedCompanies { get; set; }

    public int SuccessfulCompanies { get; set; }

    public int FailedCompanies { get; set; }

    public string? ErrorMessage { get; set; }

    public string? LockedByInstance { get; set; }

    public bool IsStartupImport { get; set; }
}

