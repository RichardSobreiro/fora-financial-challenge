
using ForaChallenge.Core.Models;
using ForaChallenge.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForaChallenge.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin");

        group.MapPost("/import", StartImport)
            .Produces<object>(StatusCodes.Status202Accepted)
            .Produces<object>(StatusCodes.Status409Conflict)
            .WithSummary("Start a new EDGAR data import")
            .WithDescription("Starts background import of company data from SEC EDGAR API. Returns 202 with job ID, 409 if import already running.");

        group.MapGet("/import/{jobId:guid}", GetImportStatus)
            .Produces<ImportJob>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound)
            .WithSummary("Get status of a specific import job");

        group.MapGet("/import/current", GetCurrentImport)
            .Produces<ImportJob>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound)
            .WithSummary("Get status of the current or most recent import job");

        static async Task<IResult> StartImport(
            [FromServices] IImportOrchestrator orchestrator,
            [FromQuery] bool force = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var jobId = await orchestrator.StartImportAsync(force, cancellationToken);

                return Results.Accepted(
                    $"/api/admin/import/{jobId}",
                    new
                    {
                        jobId,
                        message = "Import started successfully",
                        statusUrl = $"/api/admin/import/{jobId}"
                    });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        }

        static async Task<IResult> GetImportStatus(
            Guid jobId,
            [FromServices] IImportOrchestrator orchestrator,
            CancellationToken cancellationToken = default)
        {
            var job = await orchestrator.GetImportStatusAsync(jobId, cancellationToken);

            return job == null
                ? Results.NotFound(new { error = "Import job not found" })
                : Results.Ok(job);
        }

        static async Task<IResult> GetCurrentImport(
            [FromServices] IImportOrchestrator orchestrator,
            CancellationToken cancellationToken = default)
        {
            var job = await orchestrator.GetCurrentImportAsync(cancellationToken);

            return job == null
                ? Results.NotFound(new { error = "No import jobs found" })
                : Results.Ok(job);
        }
    }
}

