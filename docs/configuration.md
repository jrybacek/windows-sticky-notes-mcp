# Configuration

## Customizing the schema (if your version differs)

The schema has changed across Sticky Notes versions. The table and column names are verified against 6.x and declared as Data Annotations on [`src/Entities/NoteEntity.cs`](../src/Entities/NoteEntity.cs) (`[Table("Note")]`, `[Column("Text")]`, etc.). If your install differs, edit those attributes and rebuild.

## Database location

The database **location** is configurable at runtime — set it via environment variable (or an `appsettings.json` next to the exe) under the `StickyNotes` section:

| Setting | Default | Meaning |
| --- | --- | --- |
| `StickyNotes__SourceDirectory` | *(auto)* | Folder containing `plum.sqlite`. |
| `StickyNotes__DatabaseFileName` | `plum.sqlite` | Database file name. |

## Inspecting your own schema

Copy the DB out and open it in [DB Browser for SQLite](https://sqlitebrowser.org/):

```powershell
$src = "$env:LOCALAPPDATA\Packages\Microsoft.MicrosoftStickyNotes_8wekyb3d8bbwe\LocalState"
$dst = "$env:TEMP\sticky_inspect"
New-Item -ItemType Directory -Force -Path $dst | Out-Null
Copy-Item "$src\plum.sqlite","$src\plum.sqlite-wal","$src\plum.sqlite-shm" $dst -ErrorAction SilentlyContinue
```
