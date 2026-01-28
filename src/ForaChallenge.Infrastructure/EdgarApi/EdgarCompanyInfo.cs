using System.Text.Json.Serialization;

namespace ForaChallenge.Infrastructure.EdgarApi;

/// <summary>
/// Data Transfer Object matching SEC EDGAR API response format.
/// This is an external representation - belongs in Infrastructure, not Core.
/// </summary>
public class EdgarCompanyInfo
{
    /// <summary>
    /// CIK is sometimes returned by SEC as a JSON number (e.g. 1543151) and sometimes as a JSON string
    /// (e.g. "0001853630"). We allow both forms for robustness.
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Cik { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public InfoFact Facts { get; set; } = new();

    public class InfoFact
    {
        [JsonPropertyName("us-gaap")]
        public InfoFactUsGaap UsGaap { get; set; } = new();
    }

    public class InfoFactUsGaap
    {
        public InfoFactUsGaapNetIncomeLoss NetIncomeLoss { get; set; } = new();
    }

    public class InfoFactUsGaapNetIncomeLoss
    {
        public InfoFactUsGaapIncomeLossUnits Units { get; set; } = new();
    }

    public class InfoFactUsGaapIncomeLossUnits
    {
        [JsonPropertyName("USD")]
        public InfoFactUsGaapIncomeLossUnitsUsd[] Usd { get; set; } = Array.Empty<InfoFactUsGaapIncomeLossUnitsUsd>();
    }

    public class InfoFactUsGaapIncomeLossUnitsUsd
    {
        /// <summary>
        /// Possibilities include 10-Q, 10-K, 8-K, 20-F, 40-F, 6-K, and their variants.
        /// We are interested only in 10-K data.
        /// </summary>
        public string Form { get; set; } = string.Empty;

        /// <summary>
        /// For yearly information, the format is CY followed by the year number.
        /// For example: CY2021. We are interested only in yearly information which follows this format.
        /// </summary>
        public string Frame { get; set; } = string.Empty;

        /// <summary>
        /// The income/loss amount.
        /// </summary>
        public decimal Val { get; set; }
    }
}

