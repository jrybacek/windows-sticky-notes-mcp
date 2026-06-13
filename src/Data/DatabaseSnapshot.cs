namespace StickyNotesMcp.Data;

/// <summary>
/// A throwaway on-disk copy of the Sticky Notes database (plus its <c>-wal</c>/<c>-shm</c>
/// sidecars). The live database is never opened for querying — only copied. Disposing the
/// snapshot removes the temporary directory.
/// </summary>
public sealed class DatabaseSnapshot : IDisposable
{
    private readonly string _directory;

    public DatabaseSnapshot(string directory, string databaseFilePath)
    {
        _directory = directory;
        DatabaseFilePath = databaseFilePath;
    }

    /// <summary>Full path to the copied database file inside the temp directory.</summary>
    public string DatabaseFilePath { get; }

    /// <summary>True if the database file was actually copied (i.e. the source existed).</summary>
    public bool DatabaseExists => File.Exists(DatabaseFilePath);

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
        catch
        {
            // Best-effort cleanup; a leftover temp copy is harmless and the OS will reclaim it.
        }
    }
}
