using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StickyNotesMcp.Configuration;
using StickyNotesMcp.Data;
using StickyNotesMcp.Models;
using StickyNotesMcp.Services;

namespace StickyNotesMcp.Repositories;

/// <summary>
/// EF Core-backed repository. Takes a read-only snapshot of the live database, queries the note
/// table with LINQ (no raw SQL), and maps rows to <see cref="StickyNote"/>. A fresh context is
/// created per call because each snapshot lives at a different temp path.
/// </summary>
public sealed class StickyNoteRepository : IStickyNoteRepository
{
    private readonly IDatabaseSnapshotFactory _snapshotFactory;
    private readonly INoteTextCleaner _cleaner;
    private readonly IOptions<StickyNotesOptions> _options;
    private readonly ILogger<StickyNoteRepository> _logger;

    public StickyNoteRepository(
        IDatabaseSnapshotFactory snapshotFactory,
        INoteTextCleaner cleaner,
        IOptions<StickyNotesOptions> options,
        ILogger<StickyNoteRepository> logger)
    {
        _snapshotFactory = snapshotFactory;
        _cleaner = cleaner;
        _options = options;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StickyNote>> GetActiveNotesAsync(CancellationToken cancellationToken = default)
    {
        using var snapshot = _snapshotFactory.CreateSnapshot();
        if (!snapshot.DatabaseExists)
        {
            var dir = _options.Value.ResolveSourceDirectory();
            _logger.LogError("Sticky Notes database not found under {Dir}.", dir);
            throw new FileNotFoundException(
                $"Sticky Notes database '{_options.Value.DatabaseFileName}' not found under '{dir}'. " +
                "Is the Sticky Notes app installed for this user?");
        }

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = snapshot.DatabaseFilePath,
            Mode = SqliteOpenMode.ReadOnly
        }.ToString();

        var contextOptions = new DbContextOptionsBuilder<StickyNotesDbContext>()
            .UseSqlite(connectionString)
            .Options;

        await using var context = new StickyNotesDbContext(contextOptions);

        // Active (not soft-deleted) notes with a body, newest first. Blank-after-cleaning rows are
        // dropped below, so we only filter out the obvious nulls in the query.
        var rows = await context.Notes
            .Where(note => note.DeletedAt == null && note.Text != null)
            .OrderByDescending(note => note.UpdatedAt)
            .ToListAsync(cancellationToken);

        var notes = new List<StickyNote>(rows.Count);
        foreach (var row in rows)
        {
            var text = _cleaner.Clean(row.Text ?? string.Empty);
            if (string.IsNullOrWhiteSpace(text))
                continue;

            notes.Add(new StickyNote(text, TicksToDateTimeOffset(row.UpdatedAt)));
        }

        _logger.LogInformation("Read {Count} active sticky note(s).", notes.Count);
        return notes;
    }

    /// <summary>
    /// Sticky Notes stores timestamps as .NET <see cref="DateTime"/> ticks (100-ns intervals since
    /// 0001-01-01 UTC). Values outside the representable range are treated as unknown.
    /// </summary>
    private static DateTimeOffset? TicksToDateTimeOffset(long? ticks)
    {
        if (ticks is null or <= 0 || ticks > DateTime.MaxValue.Ticks)
            return null;
        return new DateTimeOffset(ticks.Value, TimeSpan.Zero);
    }
}
