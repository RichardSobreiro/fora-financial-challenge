
using ForaChallenge.Core.Models;

namespace ForaChallenge.Infrastructure.EdgarApi;

/// <summary>
/// Maps external EDGAR API format to our internal domain model.
/// Filters data according to business rules (10-K forms, CY frames only).
/// </summary>
public static class EdgarToCompanyMapper
{
    /// <summary>
    /// Converts <see cref="EdgarCompanyInfo"/> to <see cref="Company"/>.
    /// Filters for 10-K forms and CY (calendar year) frames only.
    /// </summary>
    public static Company MapToDomain(EdgarCompanyInfo edgarData)
    {
        var company = new Company
        {
            Cik = edgarData.Cik,
            Name = edgarData.EntityName,
            IncomeRecords = []
        };

        var usdData = edgarData.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd
            ?? Array.Empty<EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd>();

        foreach (var item in usdData)
        {
            if (!string.Equals(item.Form, "10-K", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.Frame) || !item.Frame.StartsWith("CY", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var yearPart = item.Frame.Length >= 6 ? item.Frame[2..] : string.Empty;
            if (!int.TryParse(yearPart, out var year))
            {
                continue;
            }

            // Avoid duplicates for same year (EDGAR sometimes includes multiple entries).
            if (company.IncomeRecords.Any(r => r.Year == year))
            {
                continue;
            }

            company.IncomeRecords.Add(new IncomeRecord
            {
                CompanyId = 0, // set by persistence layer
                Year = year,
                Income = item.Val,
                Form = item.Form,
                Frame = item.Frame
            });
        }

        return company;
    }
}

