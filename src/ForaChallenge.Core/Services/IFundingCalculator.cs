
using ForaChallenge.Core.Models;

namespace ForaChallenge.Core.Services;

/// <summary>
/// Calculates funding eligibility based on business rules.
/// Pure business logic - no I/O, highly testable.
/// </summary>
public interface IFundingCalculator
{
    /// <summary>
    /// Calculates standard fundable amount based on income history.
    /// Rules:
    /// - Must have data for 2018-2022 (all 5 years)
    /// - Must have positive income in 2021 AND 2022
    /// - If highest income >= $10B: 12.33% of highest
    /// - If highest income < $10B: 21.51% of highest
    /// </summary>
    decimal CalculateStandardFunding(Company company);

    /// <summary>
    /// Calculates special fundable amount with modifiers.
    /// Rules:
    /// - Start with standard amount
    /// - If name starts with vowel (A,E,I,O,U): +15%
    /// - If 2022 income < 2021 income: -25%
    /// </summary>
    decimal CalculateSpecialFunding(Company company, decimal standardAmount);
}

