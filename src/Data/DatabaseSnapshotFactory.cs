using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StickyNotesMcp.Configuration;

namespace StickyNotesMcp.Data;

/// <summary>
/// Copy-then-read implementation. The Sticky Notes database runs in WAL mode and the app keeps it
/// open while running; the most recent state often lives in the <c>-wal</c> file. To read a
/// consistent view without locking the live database, all three files are copied to a temp
/// directory and the copy is opened read-only.
/// </summary>
public sealed class DatabaseSnapshotFactory : IDatabaseSnapshotFactory
{
    private static readonly string[] SidecarSuffixes = ["", "-wal", "-shm"];

    private readonly StickyNotesOptions _options;
    private readonly ILogger<DatabaseSnapshotFactory> _logger;

    public DatabaseSnapshotFactory(IOptions<StickyNotesOptions> options, ILogger<DatabaseSnapshotFactory> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public DatabaseSnapshot CreateSnapshot()
    {
        var sourceDirectory = _options.ResolveSourceDirectory();
        var workDirectory = Path.Combine(Path.GetTempPath(), "sticky_mcp_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDirectory);

        foreach (var suffix in SidecarSuffixes)
        {
            var fileName = _options.DatabaseFileName + suffix;
            var source = Path.Combine(sourceDirectory, fileName);
            if (File.Exists(source))
                File.Copy(source, Path.Combine(workDirectory, fileName), overwrite: true);
        }

        var databaseFilePath = Path.Combine(workDirectory, _options.DatabaseFileName);
        _logger.LogDebug("Created Sticky Notes snapshot from {Source} at {Work}", sourceDirectory, workDirectory);
        return new DatabaseSnapshot(workDirectory, databaseFilePath);
    }
}
