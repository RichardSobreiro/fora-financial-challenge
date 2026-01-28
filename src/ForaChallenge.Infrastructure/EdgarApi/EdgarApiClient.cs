
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ForaChallenge.Infrastructure.EdgarApi;

/// <summary>
/// HTTP client for SEC EDGAR API with retry logic and proper error handling.
/// </summary>
public class EdgarApiClient : IEdgarApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EdgarApiClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly EdgarOptions _options;

    public EdgarApiClient(
        HttpClient httpClient,
        IOptions<EdgarOptions> options,
        ILogger<EdgarApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // EDGAR requires a User-Agent. The PDF explicitly recommends this value.
        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse(_options.UserAgent));

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_options.Accept));

        if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl, UriKind.Absolute);
        }

        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            // Retry only transient HTTP failures (not expected permanent ones like 404 NoSuchKey).
            .OrResult(ShouldRetry)
            .WaitAndRetryAsync(
                retryCount: Math.Max(0, _options.RetryCount),
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, delay, retryCount, _) =>
                {
                    _logger.LogWarning(
                        "EDGAR retry {RetryCount} after {DelaySeconds}s due to {Reason}",
                        retryCount,
                        delay.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
                });
    }

    private static bool ShouldRetry(HttpResponseMessage response)
    {
        // SEC EDGAR returns 404 for "NoSuchKey" (no companyfacts) which is expected for some CIKs.
        // Only retry transient conditions.
        return response.StatusCode == HttpStatusCode.TooManyRequests
               || response.StatusCode == HttpStatusCode.RequestTimeout
               || (int)response.StatusCode >= 500;
    }

    public async Task<EdgarCompanyInfo?> FetchCompanyFactsAsync(int cik, CancellationToken cancellationToken = default)
    {
        var cikString = cik.ToString("D10");
        var url = _httpClient.BaseAddress == null
            ? $"{_options.BaseUrl}CIK{cikString}.json"
            : $"CIK{cikString}.json";

        try
        {
            _logger.LogDebug("Fetching EDGAR data for CIK {Cik}", cik);

            var response = await _retryPolicy.ExecuteAsync(
                async ct => await _httpClient.GetAsync(url, ct),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch CIK {Cik}: {StatusCode}", cik, response.StatusCode);
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = await response.Content.ReadFromJsonAsync<EdgarCompanyInfo>(options, cancellationToken);
            _logger.LogDebug("Successfully fetched EDGAR data for CIK {Cik}", cik);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching EDGAR data for CIK {Cik}", cik);
            throw;
        }
    }
}

