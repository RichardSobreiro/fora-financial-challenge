using ForaChallenge.Core.Models;

namespace ForaChallenge.Core.Services;

public class FundingCalculator : IFundingCalculator
{
    private const decimal TenBillion = 10_000_000_000m;
    private const decimal HighIncomePercentage = 0.1233m; // 12.33%
    private const decimal LowIncomePercentage = 0.2151m;  // 21.51%
    private const decimal VowelBonus = 0.15m;              // 15%
    private const decimal DeclinePenalty = 0.25m;          // 25%

    public decimal CalculateStandardFunding(Company company)
    {
        var requiredYears = new[] { 2018, 2019, 2020, 2021, 2022 };
        var availableYears = company.IncomeRecords.Select(r => r.Year).ToHashSet();

        if (!requiredYears.All(year => availableYears.Contains(year)))
        {
            return 0m;
        }

        var income2021 = company.IncomeRecords.FirstOrDefault(r => r.Year == 2021)?.Income ?? 0m;
        var income2022 = company.IncomeRecords.FirstOrDefault(r => r.Year == 2022)?.Income ?? 0m;

        if (income2021 <= 0m || income2022 <= 0m)
        {
            return 0m;
        }

        var highestIncome = company.IncomeRecords
            .Where(r => r.Year is >= 2018 and <= 2022)
            .Max(r => r.Income);

        var percentage = highestIncome >= TenBillion ? HighIncomePercentage : LowIncomePercentage;

        return highestIncome * percentage;
    }

    public decimal CalculateSpecialFunding(Company company, decimal standardAmount)
    {
        var specialAmount = standardAmount;

        if (StartsWithVowel(company.Name))
        {
            specialAmount += standardAmount * VowelBonus;
        }

        var income2021 = company.IncomeRecords.FirstOrDefault(r => r.Year == 2021)?.Income ?? 0m;
        var income2022 = company.IncomeRecords.FirstOrDefault(r => r.Year == 2022)?.Income ?? 0m;

        if (income2022 < income2021)
        {
            specialAmount -= standardAmount * DeclinePenalty;
        }

        return specialAmount;
    }

    private static bool StartsWithVowel(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var trimmed = name.TrimStart();
        if (trimmed.Length == 0)
        {
            return false;
        }

        var firstChar = char.ToUpperInvariant(trimmed[0]);
        return firstChar is 'A' or 'E' or 'I' or 'O' or 'U';
    }
}
