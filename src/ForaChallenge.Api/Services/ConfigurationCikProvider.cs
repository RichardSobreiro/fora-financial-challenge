using ForaChallenge.Api.Options;
using ForaChallenge.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace ForaChallenge.Api.Services;

public sealed class ConfigurationCikProvider : ICikProvider
{
    private readonly ImportOptions _options;

    public ConfigurationCikProvider(IOptions<ImportOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyList<int> GetCiks()
    {
        // Keep behavior simple/predictable: return whatever is configured.
        // Validation ensures this is non-empty in production.
        return _options.Ciks;
    }
}

