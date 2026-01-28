
using ForaChallenge.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ForaChallenge.Infrastructure.Data.Configurations;

public class ImportLockConfiguration : IEntityTypeConfiguration<ImportLockEntity>
{
    public void Configure(EntityTypeBuilder<ImportLockEntity> builder)
    {
        builder.ToTable("ImportLocks");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.LockedByInstance).HasMaxLength(255);

        // Ensure singleton row exists (Id=1).
        builder.HasData(new ImportLockEntity { Id = 1 });
    }
}

