
using ForaChallenge.Core.Models;
using ForaChallenge.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForaChallenge.Api.Endpoints;

public static class CompaniesEndpoints
{
    public static void MapCompaniesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies")
            .WithTags("Companies");

        group.MapGet("/", GetCompanies)
            .Produces<List<CompanyFundingResult>>(StatusCodes.Status200OK)
            .WithSummary("Get companies with funding eligibility")
            .WithDescription("Retrieves all companies with calculated funding amounts. Optionally filter by company name prefix.");

        static async Task<IResult> GetCompanies(
            [FromServices] ICompanyFundingService fundingService,
            [FromQuery] string? startsWith = null,
            CancellationToken cancellationToken = default)
        {
            var results = await fundingService.GetCompaniesWithFundingAsync(startsWith, cancellationToken);
            return Results.Ok(results);
        }
    }
}

