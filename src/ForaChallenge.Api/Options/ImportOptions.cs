namespace ForaChallenge.Api.Options;

public sealed class ImportOptions
{
    public const string SectionName = "Import";

    /// <summary>
    /// CIKs to import from SEC EDGAR.
    /// </summary>
    public int[] Ciks { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Maximum number of companies to process in parallel during an import.
    /// Kept intentionally conservative to avoid hammering the SEC endpoint.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 4;

    /// <summary>
    /// How often to persist job progress updates to the database, in seconds.
    /// </summary>
    public int ProgressUpdateIntervalSeconds { get; set; } = 2;
}

