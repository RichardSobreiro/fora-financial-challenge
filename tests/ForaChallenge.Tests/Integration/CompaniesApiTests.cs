
using FluentAssertions;
using ForaChallenge.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ForaChallenge.Tests.Integration;

public class CompaniesApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CompaniesApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCompanies_Returns200_AndJsonArray()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Replace DB-backed repository for tests.
                services.AddSingleton<ICompanyRepository, FakeCompanyRepository>();
            });
        });

        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/companies");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.TrimStart().Should().StartWith("[");
    }
}

