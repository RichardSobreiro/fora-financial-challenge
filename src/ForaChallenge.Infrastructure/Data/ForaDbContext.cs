
using ForaChallenge.Infrastructure.Data.Configurations;
using ForaChallenge.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForaChallenge.Infrastructure.Data;

public class ForaDbContext : DbContext
{
    public ForaDbContext(DbContextOptions<ForaDbContext> options) : base(options)
    {
    }

    public DbSet<CompanyEntity> Companies => Set<CompanyEntity>();
    public DbSet<IncomeRecordEntity> IncomeRecords => Set<IncomeRecordEntity>();
    public DbSet<ImportJobEntity> ImportJobs => Set<ImportJobEntity>();
    public DbSet<ImportLockEntity> ImportLocks => Set<ImportLockEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new IncomeRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ImportJobConfiguration());
        modelBuilder.ApplyConfiguration(new ImportLockConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var companyEntries = ChangeTracker.Entries<CompanyEntity>();
        foreach (var entry in companyEntries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

