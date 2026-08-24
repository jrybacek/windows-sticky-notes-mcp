# Configuration

## Database location

The database **location** is configurable at runtime via environment variables, under the `StickyNotes` section:

| Setting | Default | Meaning |
| --- | --- | --- |
| `StickyNotes__SourceDirectory` | *(auto)* | Folder containing `plum.sqlite`. |
| `StickyNotes__DatabaseFileName` | `plum.sqlite` | Database file name. |

Set these in your MCP client's server config (an `env` block, e.g. in `.claude.json` or `config.toml`) if you're running a downloaded release exe — this is the primary way to override the database location without rebuilding.

If you're running a build from source, an `appsettings.json` next to the exe works too (content root resolves to the exe's directory, including under single-file publish).

## Customizing the schema (if your version differs)

The schema has changed across Sticky Notes versions. The table and column names are verified against 6.x and declared as Data Annotations on [`src/Entities/NoteEntity.cs`](../src/Entities/NoteEntity.cs) (`[Table("Note")]`, `[Column("Text")]`, etc.). If your install differs, edit those attributes and rebuild — this requires building from source; see [CONTRIBUTING.md](../CONTRIBUTING.md).

## Inspecting your own schema

Copy the DB out and open it in [DB Browser for SQLite](https://sqlitebrowser.org/):

```powershell
$src = "$env:LOCALAPPDATA\Packages\Microsoft.MicrosoftStickyNotes_8wekyb3d8bbwe\LocalState"
$dst = "$env:TEMP\sticky_inspect"
New-Item -ItemType Directory -Force -Path $dst | Out-Null
Copy-Item "$src\plum.sqlite","$src\plum.sqlite-wal","$src\plum.sqlite-shm" $dst -ErrorAction SilentlyContinue
```
