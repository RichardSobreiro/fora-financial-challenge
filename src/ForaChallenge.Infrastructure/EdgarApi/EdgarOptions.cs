
namespace ForaChallenge.Infrastructure.EdgarApi;

/// <summary>
/// Strongly typed configuration options for the SEC EDGAR API integration.
/// </summary>
public class EdgarOptions
{
    public const string SectionName = "Edgar";

    /// <summary>
    /// Base URL for EDGAR Company Facts endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://data.sec.gov/api/xbrl/companyfacts/";

    /// <summary>
    /// EDGAR requires a User-Agent header. PDF recommends PostmanRuntime/7.34.0.
    /// </summary>
    public string UserAgent { get; set; } = "PostmanRuntime/7.34.0";

    /// <summary>
    /// Accept header value. PDF recommends */*.
    /// </summary>
    public string Accept { get; set; } = "*/*";

    /// <summary>
    /// Number of retry attempts for transient failures.
    /// </summary>
    public int RetryCount { get; set; } = 3;
}

