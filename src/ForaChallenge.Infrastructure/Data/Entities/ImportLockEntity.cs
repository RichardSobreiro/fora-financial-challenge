
namespace ForaChallenge.Infrastructure.Data.Entities;

public class ImportLockEntity
{
    public int Id { get; set; }

    public Guid? CurrentJobId { get; set; }

    public DateTime? LockedAt { get; set; }

    public string? LockedByInstance { get; set; }
}

