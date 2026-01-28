
using ForaChallenge.Core.Models;
using ForaChallenge.Core.Services;
using FluentAssertions;

namespace ForaChallenge.Tests.Unit;

public class FundingCalculatorTests
{
    private readonly IFundingCalculator _calculator = new FundingCalculator();

    [Fact]
    public void CalculateStandardFunding_WithMissingYear_ReturnsZero()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2018, Income = 5_000_000 },
                new() { Year = 2019, Income = 5_000_000 },
                new() { Year = 2020, Income = 5_000_000 },
                new() { Year = 2021, Income = 5_000_000 }
            ]
        };

        _calculator.CalculateStandardFunding(company).Should().Be(0m);
    }

    [Fact]
    public void CalculateStandardFunding_WithNonPositive2021Income_ReturnsZero()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2018, Income = 5_000_000 },
                new() { Year = 2019, Income = 5_000_000 },
                new() { Year = 2020, Income = 5_000_000 },
                new() { Year = 2021, Income = 0m },
                new() { Year = 2022, Income = 5_000_000 }
            ]
        };

        _calculator.CalculateStandardFunding(company).Should().Be(0m);
    }

    [Fact]
    public void CalculateStandardFunding_WithIncomeLessThan10B_Uses21Point51Percent()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2018, Income = 1_000_000_000 },
                new() { Year = 2019, Income = 2_000_000_000 },
                new() { Year = 2020, Income = 3_000_000_000 },
                new() { Year = 2021, Income = 4_000_000_000 },
                new() { Year = 2022, Income = 5_000_000_000 }
            ]
        };

        _calculator.CalculateStandardFunding(company).Should().Be(1_075_500_000m);
    }

    [Fact]
    public void CalculateStandardFunding_WithIncomeGreaterThanOrEqualTo10B_Uses12Point33Percent()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2018, Income = 8_000_000_000 },
                new() { Year = 2019, Income = 9_000_000_000 },
                new() { Year = 2020, Income = 10_000_000_000 },
                new() { Year = 2021, Income = 11_000_000_000 },
                new() { Year = 2022, Income = 12_000_000_000 }
            ]
        };

        _calculator.CalculateStandardFunding(company).Should().Be(1_479_600_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithVowelStart_Adds15Percent()
    {
        var company = new Company
        {
            Name = "Apple Inc.",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 1m },
                new() { Year = 2022, Income = 1m }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(1_150_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithDecliningIncome_Subtracts25Percent()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 6_000_000_000 },
                new() { Year = 2022, Income = 5_000_000_000 }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(750_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithVowelAndDeclining_AppliesBothModifiers()
    {
        var company = new Company
        {
            Name = "Uber Technologies",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 6_000_000_000 },
                new() { Year = 2022, Income = 5_000_000_000 }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(900_000m);
    }

    [Fact]
    public void CalculateStandardFunding_WithNonPositive2022Income_ReturnsZero()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2018, Income = 5_000_000 },
                new() { Year = 2019, Income = 5_000_000 },
                new() { Year = 2020, Income = 5_000_000 },
                new() { Year = 2021, Income = 5_000_000 },
                new() { Year = 2022, Income = -1_000_000 }
            ]
        };

        _calculator.CalculateStandardFunding(company).Should().Be(0m);
    }

    [Fact]
    public void CalculateStandardFunding_WithNegative2021Income_ReturnsZero()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2018, Income = 5_000_000 },
                new() { Year = 2019, Income = 5_000_000 },
                new() { Year = 2020, Income = 5_000_000 },
                new() { Year = 2021, Income = -1_000_000 },
                new() { Year = 2022, Income = 5_000_000 }
            ]
        };

        _calculator.CalculateStandardFunding(company).Should().Be(0m);
    }

    [Fact]
    public void CalculateStandardFunding_WithExactly10B_Uses12Point33Percent()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2018, Income = 8_000_000_000 },
                new() { Year = 2019, Income = 9_000_000_000 },
                new() { Year = 2020, Income = 10_000_000_000 },
                new() { Year = 2021, Income = 11_000_000_000 },
                new() { Year = 2022, Income = 12_000_000_000 }
            ]
        };

        // Highest is 12B, but threshold is >= 10B, so should use 12.33%
        var result = _calculator.CalculateStandardFunding(company);
        result.Should().Be(12_000_000_000m * 0.1233m);
    }

    [Fact]
    public void CalculateStandardFunding_WithHighestIncomeExactlyAtThreshold_Uses12Point33Percent()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2018, Income = 5_000_000_000 },
                new() { Year = 2019, Income = 6_000_000_000 },
                new() { Year = 2020, Income = 7_000_000_000 },
                new() { Year = 2021, Income = 8_000_000_000 },
                new() { Year = 2022, Income = 10_000_000_000 }
            ]
        };

        var result = _calculator.CalculateStandardFunding(company);
        result.Should().Be(10_000_000_000m * 0.1233m);
    }

    [Fact]
    public void CalculateStandardFunding_WithHighestIncomeJustBelowThreshold_Uses21Point51Percent()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2018, Income = 5_000_000_000 },
                new() { Year = 2019, Income = 6_000_000_000 },
                new() { Year = 2020, Income = 7_000_000_000 },
                new() { Year = 2021, Income = 8_000_000_000 },
                new() { Year = 2022, Income = 9_999_999_999 }
            ]
        };

        var result = _calculator.CalculateStandardFunding(company);
        result.Should().Be(9_999_999_999m * 0.2151m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithNoModifiers_ReturnsStandardAmount()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 5_000_000_000 },
                new() { Year = 2022, Income = 6_000_000_000 }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(1_000_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithVowelStartButNoDecline_OnlyAppliesVowelBonus()
    {
        var company = new Company
        {
            Name = "Apple Inc.",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 5_000_000_000 },
                new() { Year = 2022, Income = 6_000_000_000 }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(1_150_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithDeclineButNoVowel_OnlyAppliesDeclinePenalty()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 6_000_000_000 },
                new() { Year = 2022, Income = 5_000_000_000 }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(750_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithEqual2021And2022Income_NoDeclinePenalty()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 5_000_000_000 },
                new() { Year = 2022, Income = 5_000_000_000 }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(1_000_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithVowelStartCaseInsensitive_AppliesBonus()
    {
        var company = new Company
        {
            Name = "amazon technologies",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 1m },
                new() { Year = 2022, Income = 1m }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(1_150_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithVowelStartAfterWhitespace_AppliesBonus()
    {
        var company = new Company
        {
            Name = "  Apple Inc.",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 1m },
                new() { Year = 2022, Income = 1m }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(1_150_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithEmptyName_NoVowelBonus()
    {
        var company = new Company
        {
            Name = "",
            IncomeRecords =
            [
                new() { Year = 2021, Income = 1m },
                new() { Year = 2022, Income = 1m }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(1_000_000m);
    }

    [Fact]
    public void CalculateSpecialFunding_WithNullName_NoVowelBonus()
    {
        var company = new Company
        {
            Name = null!,
            IncomeRecords =
            [
                new() { Year = 2021, Income = 1m },
                new() { Year = 2022, Income = 1m }
            ]
        };

        _calculator.CalculateSpecialFunding(company, 1_000_000m).Should().Be(1_000_000m);
    }

    [Fact]
    public void CalculateStandardFunding_WithExtraYears_OnlyUses2018To2022()
    {
        var company = new Company
        {
            Name = "Test Corp",
            IncomeRecords =
            [
                new() { Year = 2017, Income = 20_000_000_000 },
                new() { Year = 2018, Income = 1_000_000_000 },
                new() { Year = 2019, Income = 2_000_000_000 },
                new() { Year = 2020, Income = 3_000_000_000 },
                new() { Year = 2021, Income = 4_000_000_000 },
                new() { Year = 2022, Income = 5_000_000_000 },
                new() { Year = 2023, Income = 20_000_000_000 }
            ]
        };

        // Should use highest from 2018-2022 (5B), not 2017 or 2023
        var result = _calculator.CalculateStandardFunding(company);
        result.Should().Be(5_000_000_000m * 0.2151m);
    }
}

