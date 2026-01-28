
namespace ForaChallenge.Infrastructure.EdgarApi;

/// <summary>
/// Low-level HTTP client for EDGAR API.
/// Infrastructure concern - handles HTTP communication.
/// </summary>
public interface IEdgarApiClient
{
    Task<EdgarCompanyInfo?> FetchCompanyFactsAsync(int cik, CancellationToken cancellationToken = default);
}

