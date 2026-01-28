
using ForaChallenge.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ForaChallenge.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<CompanyEntity>
{
    public void Configure(EntityTypeBuilder<CompanyEntity> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Cik).IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(c => c.Cik).IsUnique();
        builder.HasIndex(c => c.Name);

        builder.HasMany(c => c.IncomeRecords)
            .WithOne(i => i.Company)
            .HasForeignKey(i => i.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

