# Architecture

## How it reads safely

The Sticky Notes DB runs in WAL mode and the app keeps it open. To avoid lock contention or inconsistent reads, the server **copies** `plum.sqlite` (plus its `-wal` and `-shm` sidecars — the most recent edits often live in the WAL) to a temp directory, opens the copy **read-only**, queries it, and deletes the copy. It never opens the live database for anything but a file copy.

## Design

The server is structured around small, single-responsibility pieces wired together by DI in [`src/Program.cs`](../src/Program.cs):

| Piece | Responsibility |
| --- | --- |
| `StickyNotesOptions` | Database location (source dir / file name), bindable from config. |
| `NoteEntity` | Keyless EF entity; table/column names declared via Data Annotations. |
| `IDatabaseSnapshotFactory` / `DatabaseSnapshot` | Copy-then-read policy and temp-copy lifecycle. |
| `INoteTextCleaner` | Strip Sticky Notes formatting control words. |
| `IStickyNoteRepository` / `StickyNotesDbContext` | Query the snapshot read-only with EF Core (LINQ, no raw SQL) and map rows to notes. |
| `StickyNotesTools` | Thin MCP adapter — exposes `get_sticky_notes` (the .NET method is `GetStickyNotes`). |

Consumers depend only on interfaces, so each piece is independently testable (see [`tests/`](../tests/)).
