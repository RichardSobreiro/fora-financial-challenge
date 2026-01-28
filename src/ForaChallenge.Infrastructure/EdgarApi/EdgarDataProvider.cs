
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;

namespace ForaChallenge.Infrastructure.EdgarApi;

/// <summary>
/// Implements <see cref="IEdgarDataProvider"/> by fetching from EDGAR API and mapping to domain model.
/// </summary>
public class EdgarDataProvider : IEdgarDataProvider
{
    private readonly IEdgarApiClient _apiClient;

    public EdgarDataProvider(IEdgarApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<Company?> GetCompanyDataAsync(int cik, CancellationToken cancellationToken = default)
    {
        var edgarData = await _apiClient.FetchCompanyFactsAsync(cik, cancellationToken);
        return edgarData == null ? null : EdgarToCompanyMapper.MapToDomain(edgarData);
    }
}

