using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StickyNotesMcp.Configuration;
using StickyNotesMcp.Data;
using StickyNotesMcp.Repositories;
using StickyNotesMcp.Services;
using Xunit;

namespace StickyNotesMcp.Tests;

public class StickyNoteRepositoryTests
{
    // 2024-01-01 and 2024-06-01 (UTC) as .NET ticks, matching how Sticky Notes stores timestamps.
    private static readonly long OlderTicks = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
    private static readonly long NewerTicks = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

    [Fact]
    public async Task GetActiveNotesAsync_ReturnsOnlyActiveNotes_NewestFirst()
    {
        using var db = SyntheticDatabase.Create(
            ("older active", OlderTicks, deletedTicks: null),
            ("newer active", NewerTicks, deletedTicks: null),
            ("deleted note", NewerTicks, deletedTicks: NewerTicks),
            ("   ", NewerTicks, deletedTicks: null)); // blank body — filtered out

        var repository = CreateRepository(db);

        var notes = await repository.GetActiveNotesAsync();

        Assert.Collection(notes,
            n => Assert.Equal("newer active", n.Text),
            n => Assert.Equal("older active", n.Text));
    }

    [Fact]
    public async Task GetActiveNotesAsync_MapsTicksToUpdatedAtUtc()
    {
        using var db = SyntheticDatabase.Create(("only note", NewerTicks, deletedTicks: null));

        var notes = await CreateRepository(db).GetActiveNotesAsync();

        var note = Assert.Single(notes);
        Assert.Equal(new DateTimeOffset(NewerTicks, TimeSpan.Zero), note.UpdatedAt);
    }

    [Fact]
    public async Task GetActiveNotesAsync_StripsFormattingFromBody()
    {
        using var db = SyntheticDatabase.Create(
            (@"\id=abc\b Buy milk\b0", NewerTicks, deletedTicks: null));

        var notes = await CreateRepository(db).GetActiveNotesAsync();

        Assert.Equal("Buy milk", Assert.Single(notes).Text);
    }

    [Fact]
    public async Task GetActiveNotesAsync_MissingDatabase_Throws()
    {
        var factory = new StubSnapshotFactory(
            Path.Combine(Path.GetTempPath(), "sticky_missing_" + Guid.NewGuid().ToString("N")));

        var repository = new StickyNoteRepository(
            factory, new NoteTextCleaner(), Options.Create(new StickyNotesOptions()),
            NullLogger<StickyNoteRepository>.Instance);

        await Assert.ThrowsAsync<FileNotFoundException>(() => repository.GetActiveNotesAsync());
    }

    private static StickyNoteRepository CreateRepository(SyntheticDatabase db) =>
        new(new StubSnapshotFactory(db.Directory),
            new NoteTextCleaner(),
            Options.Create(new StickyNotesOptions()),
            NullLogger<StickyNoteRepository>.Instance);

    /// <summary>Returns a snapshot that points directly at a pre-built directory (no copying).</summary>
    private sealed class StubSnapshotFactory(string directory) : IDatabaseSnapshotFactory
    {
        public DatabaseSnapshot CreateSnapshot() =>
            new(directory, Path.Combine(directory, "plum.sqlite"));
    }

    /// <summary>Builds a throwaway SQLite file with a Note table matching the real schema.</summary>
    private sealed class SyntheticDatabase : IDisposable
    {
        public string Directory { get; }

        private SyntheticDatabase(string directory) => Directory = directory;

        public static SyntheticDatabase Create(params (string? text, long updatedTicks, long? deletedTicks)[] rows)
        {
            var directory = Path.Combine(Path.GetTempPath(), "sticky_test_" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            var dbPath = Path.Combine(directory, "plum.sqlite");

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var create = connection.CreateCommand())
                {
                    create.CommandText = "CREATE TABLE Note (Text TEXT, UpdatedAt INTEGER, DeletedAt INTEGER);";
                    create.ExecuteNonQuery();
                }

                foreach (var (text, updatedTicks, deletedTicks) in rows)
                {
                    using var insert = connection.CreateCommand();
                    insert.CommandText =
                        "INSERT INTO Note (Text, UpdatedAt, DeletedAt) VALUES ($text, $updated, $deleted);";
                    insert.Parameters.AddWithValue("$text", (object?)text ?? DBNull.Value);
                    insert.Parameters.AddWithValue("$updated", updatedTicks);
                    insert.Parameters.AddWithValue("$deleted", (object?)deletedTicks ?? DBNull.Value);
                    insert.ExecuteNonQuery();
                }
            }

            // Release the file handle so the read-only repository (and cleanup) can access it.
            SqliteConnection.ClearAllPools();
            return new SyntheticDatabase(directory);
        }

        public void Dispose()
        {
            try { System.IO.Directory.Delete(Directory, recursive: true); } catch { /* best effort */ }
        }
    }
}
