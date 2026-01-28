
using ForaChallenge.Api.BackgroundServices;
using ForaChallenge.Api.Endpoints;
using ForaChallenge.Api.Middleware;
using ForaChallenge.Api.Options;
using ForaChallenge.Api.Services;
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Services;
using ForaChallenge.Infrastructure;
using ForaChallenge.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddScoped<IFundingCalculator, FundingCalculator>();
builder.Services.AddScoped<ICompanyFundingService, CompanyFundingService>();
builder.Services.AddScoped<IImportOrchestrator, ImportOrchestrator>();
builder.Services.AddScoped<ICompanyImportProcessor, CompanyImportProcessor>();
builder.Services.AddScoped<IImportJobExecutor, ParallelImportJobExecutor>();

builder.Services.AddOptions<ImportOptions>()
    .Bind(builder.Configuration.GetSection(ImportOptions.SectionName))
    .Validate(o => o.Ciks is { Length: > 0 }, $"{ImportOptions.SectionName}:Ciks is required")
    .Validate(o => o.MaxDegreeOfParallelism >= 1 && o.MaxDegreeOfParallelism <= 16, $"{ImportOptions.SectionName}:MaxDegreeOfParallelism must be between 1 and 16")
    .Validate(o => o.ProgressUpdateIntervalSeconds >= 1 && o.ProgressUpdateIntervalSeconds <= 60, $"{ImportOptions.SectionName}:ProgressUpdateIntervalSeconds must be between 1 and 60")
    .ValidateOnStart();
builder.Services.AddSingleton<ICikProvider, ConfigurationCikProvider>();

builder.Services.AddSingleton<IImportJobQueue>(_ => new ImportJobQueue());
if (!isTesting)
{
    builder.Services.AddHostedService<ImportWorkerService>();
    builder.Services.AddHostedService<StartupImportService>();
}

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks().AddDbContextCheck<ForaDbContext>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ForaDbContext>();

    var strategy = dbContext.Database.CreateExecutionStrategy();
    await strategy.ExecuteAsync(async () =>
    {
        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (SqlException ex) when (ex.Number == 1801)
        {
            // CREATE DATABASE can race/retry and return "already exists" (1801).
            // In that case, just continue and apply migrations.
            await dbContext.Database.MigrateAsync();
        }
    });
}

app.MapCompaniesEndpoints();
app.MapAdminEndpoints();

app.Run();

// Expose Program class for integration testing (WebApplicationFactory).
public partial class Program { }

