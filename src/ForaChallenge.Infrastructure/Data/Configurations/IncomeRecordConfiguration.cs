
using ForaChallenge.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ForaChallenge.Infrastructure.Data.Configurations;

public class IncomeRecordConfiguration : IEntityTypeConfiguration<IncomeRecordEntity>
{
    public void Configure(EntityTypeBuilder<IncomeRecordEntity> builder)
    {
        builder.ToTable("IncomeRecords");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.CompanyId).IsRequired();
        builder.Property(i => i.Year).IsRequired();

        builder.Property(i => i.Income)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.Form)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(i => i.Frame)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(i => new { i.CompanyId, i.Year }).IsUnique();
        builder.HasIndex(i => i.CompanyId);
    }
}

