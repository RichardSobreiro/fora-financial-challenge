
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;
using ForaChallenge.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ForaChallenge.Infrastructure.Data.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly ForaDbContext _context;

    public CompanyRepository(ForaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Company>> GetCompaniesAsync(string? nameStartsWith = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Companies
            .Include(c => c.IncomeRecords)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nameStartsWith))
        {
            query = query.Where(c => c.Name.StartsWith(nameStartsWith));
        }

        var entities = await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<Company?> GetCompanyByCikAsync(int cik, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Companies
            .Include(c => c.IncomeRecords)
            .FirstOrDefaultAsync(c => c.Cik == cik, cancellationToken);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<int> GetCompanyCountAsync(CancellationToken cancellationToken = default)
        => await _context.Companies.CountAsync(cancellationToken);

    public async Task AddCompanyAsync(Company company, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(company);
        _context.Companies.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCompanyAsync(Company company, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Companies
            .Include(c => c.IncomeRecords)
            .FirstOrDefaultAsync(c => c.Id == company.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Company {company.Id} not found");
        }

        entity.Name = company.Name;
        entity.UpdatedAt = DateTime.UtcNow;

        foreach (var record in company.IncomeRecords)
        {
            var existing = entity.IncomeRecords.FirstOrDefault(r => r.Year == record.Year);
            if (existing == null)
            {
                entity.IncomeRecords.Add(new IncomeRecordEntity
                {
                    Year = record.Year,
                    Income = record.Income,
                    Form = record.Form,
                    Frame = record.Frame,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Income = record.Income;
                existing.Form = record.Form;
                existing.Frame = record.Frame;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ImportJob?> GetImportJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ImportJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<ImportJob?> GetLatestImportJobAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _context.ImportJobs
            .AsNoTracking()
            .OrderByDescending(j => j.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task SaveImportJobAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(job);
        _context.ImportJobs.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateImportJobAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ImportJobs
            .FirstOrDefaultAsync(j => j.Id == job.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Import job {job.Id} not found");
        }

        entity.Status = job.Status;
        entity.CompletedAt = job.CompletedAt;
        entity.TotalCompanies = job.TotalCompanies;
        entity.ProcessedCompanies = job.ProcessedCompanies;
        entity.SuccessfulCompanies = job.SuccessfulCompanies;
        entity.FailedCompanies = job.FailedCompanies;
        entity.ErrorMessage = job.ErrorMessage;
        entity.LockedByInstance = job.LockedByInstance;
        entity.IsStartupImport = job.IsStartupImport;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ImportLock?> GetImportLockAsync(CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var entity = await _context.ImportLocks
                    .FirstOrDefaultAsync(l => l.Id == 1, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return entity == null ? null : MapToDomain(entity);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task UpdateImportLockAsync(ImportLock importLock, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var entity = await _context.ImportLocks
                    .FirstOrDefaultAsync(l => l.Id == 1, cancellationToken);

                if (entity == null)
                {
                    entity = new ImportLockEntity { Id = 1 };
                    _context.ImportLocks.Add(entity);
                }

                entity.CurrentJobId = importLock.CurrentJobId;
                entity.LockedAt = importLock.LockedAt;
                entity.LockedByInstance = importLock.LockedByInstance;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private static Company MapToDomain(CompanyEntity entity)
        => new()
        {
            Id = entity.Id,
            Cik = entity.Cik,
            Name = entity.Name,
            IncomeRecords = entity.IncomeRecords
                .Select(r => new IncomeRecord
                {
                    Id = r.Id,
                    CompanyId = r.CompanyId,
                    Year = r.Year,
                    Income = r.Income,
                    Form = r.Form,
                    Frame = r.Frame
                })
                .ToList()
        };

    private static CompanyEntity MapToEntity(Company domain)
        => new()
        {
            Id = domain.Id,
            Cik = domain.Cik,
            Name = domain.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IncomeRecords = domain.IncomeRecords
                .Select(r => new IncomeRecordEntity
                {
                    Id = r.Id,
                    Year = r.Year,
                    Income = r.Income,
                    Form = r.Form,
                    Frame = r.Frame,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList()
        };

    private static ImportJob MapToDomain(ImportJobEntity entity)
        => new()
        {
            Id = entity.Id,
            Status = entity.Status,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            TotalCompanies = entity.TotalCompanies,
            ProcessedCompanies = entity.ProcessedCompanies,
            SuccessfulCompanies = entity.SuccessfulCompanies,
            FailedCompanies = entity.FailedCompanies,
            ErrorMessage = entity.ErrorMessage,
            LockedByInstance = entity.LockedByInstance,
            IsStartupImport = entity.IsStartupImport
        };

    private static ImportJobEntity MapToEntity(ImportJob domain)
        => new()
        {
            Id = domain.Id,
            Status = domain.Status,
            StartedAt = domain.StartedAt,
            CompletedAt = domain.CompletedAt,
            TotalCompanies = domain.TotalCompanies,
            ProcessedCompanies = domain.ProcessedCompanies,
            SuccessfulCompanies = domain.SuccessfulCompanies,
            FailedCompanies = domain.FailedCompanies,
            ErrorMessage = domain.ErrorMessage,
            LockedByInstance = domain.LockedByInstance,
            IsStartupImport = domain.IsStartupImport
        };

    private static ImportLock MapToDomain(ImportLockEntity entity)
        => new()
        {
            Id = entity.Id,
            CurrentJobId = entity.CurrentJobId,
            LockedAt = entity.LockedAt,
            LockedByInstance = entity.LockedByInstance
        };
}

