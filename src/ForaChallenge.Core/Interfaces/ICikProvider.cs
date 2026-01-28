namespace ForaChallenge.Core.Interfaces;

/// <summary>
/// Provides the list of CIKs to import. Implementations can be backed by configuration,
/// a database, or any other source without coupling Core to configuration concerns.
/// </summary>
public interface ICikProvider
{
    IReadOnlyList<int> GetCiks();
}

