
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Infrastructure.Data;
using ForaChallenge.Infrastructure.Data.Repositories;
using ForaChallenge.Infrastructure.EdgarApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ForaChallenge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ForaDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services.AddScoped<ICompanyRepository, CompanyRepository>();

        services.AddOptions<EdgarOptions>()
            .Bind(configuration.GetSection(EdgarOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), $"{EdgarOptions.SectionName}:BaseUrl is required")
            .ValidateOnStart();

        services.AddHttpClient<IEdgarApiClient, EdgarApiClient>();
        services.AddScoped<IEdgarDataProvider, EdgarDataProvider>();

        return services;
    }
}

