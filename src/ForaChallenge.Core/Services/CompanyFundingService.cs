using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;

namespace ForaChallenge.Core.Services;

public class CompanyFundingService : ICompanyFundingService
{
    private readonly ICompanyRepository _repository;
    private readonly IFundingCalculator _calculator;

    public CompanyFundingService(ICompanyRepository repository, IFundingCalculator calculator)
    {
        _repository = repository;
        _calculator = calculator;
    }

    public async Task<List<CompanyFundingResult>> GetCompaniesWithFundingAsync(
        string? nameStartsWith = null,
        CancellationToken cancellationToken = default)
    {
        var companies = await _repository.GetCompaniesAsync(nameStartsWith, cancellationToken);

        return companies
            .OrderBy(c => c.Id)
            .Select(CalculateFunding)
            .ToList();
    }

    public CompanyFundingResult CalculateFunding(Company company)
    {
        var standardAmount = _calculator.CalculateStandardFunding(company);
        var specialAmount = _calculator.CalculateSpecialFunding(company, standardAmount);

        return new CompanyFundingResult
        {
            Id = company.Id,
            Name = company.Name,
            StandardFundableAmount = standardAmount,
            SpecialFundableAmount = specialAmount
        };
    }
}
