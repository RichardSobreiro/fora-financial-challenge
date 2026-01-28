
using ForaChallenge.Core.Enums;

namespace ForaChallenge.Core.Models;

/// <summary>
/// Tracks the status and progress of a data import operation.
/// Enables observability and supports distributed locking.
/// </summary>
public class ImportJob
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

