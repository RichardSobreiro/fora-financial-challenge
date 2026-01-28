
namespace ForaChallenge.Core.Models;

/// <summary>
/// Singleton record that acts as a distributed mutex across multiple API instances.
/// Ensures only one import can run at a time across all instances.
/// </summary>
public class ImportLock
{
    /// <summary>
    /// Always 1 (singleton pattern).
    /// </summary>
    public int Id { get; set; } = 1;

    public Guid? CurrentJobId { get; set; }

    public DateTime? LockedAt { get; set; }

    public string? LockedByInstance { get; set; }
}

