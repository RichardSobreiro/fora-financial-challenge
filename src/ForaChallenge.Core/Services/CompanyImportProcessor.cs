using ForaChallenge.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ForaChallenge.Core.Services;

/// <summary>
/// Default implementation of <see cref="ICompanyImportProcessor"/>.
/// </summary>
public sealed class CompanyImportProcessor : ICompanyImportProcessor
{
    private readonly ICompanyRepository _repository;
    private readonly IEdgarDataProvider _dataProvider;
    private readonly ILogger<CompanyImportProcessor> _logger;

    public CompanyImportProcessor(
        ICompanyRepository repository,
        IEdgarDataProvider dataProvider,
        ILogger<CompanyImportProcessor> logger)
    {
        _repository = repository;
        _dataProvider = dataProvider;
        _logger = logger;
    }

    public async Task ProcessCompanyAsync(int cik, CancellationToken cancellationToken = default)
    {
        var companyData = await _dataProvider.GetCompanyDataAsync(cik, cancellationToken);
        if (companyData == null)
        {
            _logger.LogWarning("No data returned for CIK {Cik}", cik);
            return;
        }

        var existingCompany = await _repository.GetCompanyByCikAsync(cik, cancellationToken);
        if (existingCompany == null)
        {
            await _repository.AddCompanyAsync(companyData, cancellationToken);
            return;
        }

        existingCompany.Name = companyData.Name;

        foreach (var newRecord in companyData.IncomeRecords)
        {
            var existingRecord = existingCompany.IncomeRecords.FirstOrDefault(r => r.Year == newRecord.Year);
            if (existingRecord == null)
            {
                existingCompany.IncomeRecords.Add(newRecord);
            }
            else
            {
                existingRecord.Income = newRecord.Income;
                existingRecord.Form = newRecord.Form;
                existingRecord.Frame = newRecord.Frame;
            }
        }

        await _repository.UpdateCompanyAsync(existingCompany, cancellationToken);
    }
}

