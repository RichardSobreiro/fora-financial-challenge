
using ForaChallenge.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ForaChallenge.Infrastructure.Data.Configurations;

public class ImportJobConfiguration : IEntityTypeConfiguration<ImportJobEntity>
{
    public void Configure(EntityTypeBuilder<ImportJobEntity> builder)
    {
        builder.ToTable("ImportJobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(j => j.StartedAt).IsRequired();

        builder.Property(j => j.ErrorMessage).HasMaxLength(4000);
        builder.Property(j => j.LockedByInstance).HasMaxLength(255);

        builder.Property(j => j.IsStartupImport)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(j => j.StartedAt).IsDescending();
    }
}

