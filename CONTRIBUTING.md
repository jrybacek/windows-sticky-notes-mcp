# Contributing

Thanks for considering a contribution to StickyNotesMcp. This covers building from source, the dev workflow, and how to submit changes. For what the project does and how to *use* it, see [README.md](README.md).

## Prerequisites

- Windows, with the Microsoft Sticky Notes app installed (for manual testing against a real database).
- [.NET 10 SDK](https://dotnet.microsoft.com/download). The SDK version is pinned in [global.json](global.json); `dotnet --version` should report a compatible version once installed.

## Build & test

```powershell
git clone https://github.com/jrybacek/windows-sticky-notes-mcp.git
cd windows-sticky-notes-mcp

dotnet build src\StickyNotesMcp.slnx -c Release
dotnet test src\StickyNotesMcp.slnx -c Release
```

The solution file (`StickyNotesMcp.slnx`) lives in `src\`, not the repo root — always point `dotnet build`/`dotnet test` at it explicitly rather than running from the repo root.

The built exe lands at `src\bin\Release\net10.0\StickyNotesMcp.exe`. Point your MCP client at that path the same way the README points it at a downloaded release asset (see [README.md](README.md#2-connect-your-client)).

## Publishing a release build locally

To reproduce exactly what the release workflow ships:

```powershell
dotnet publish src\StickyNotesMcp.csproj -c Release -r win-x64 -o .\publish
```

This produces a self-contained, single-file `StickyNotesMcp.exe` in `.\publish`. The `-r <rid>` flag is what triggers the single-file/self-contained settings in [src/StickyNotesMcp.csproj](src/StickyNotesMcp.csproj) — a plain `dotnet build`/`dotnet publish` without a runtime identifier stays framework-dependent and multi-file, which is what you want for day-to-day development.

Supported RIDs for release assets: `win-x64`, `win-arm64`.

## Project layout

See [docs/architecture.md](docs/architecture.md) for the component responsibilities and the copy-then-read design. In short: `Program.cs` wires up DI, `Data/` handles the safe database snapshot, `Repositories/` and `Services/` do the read/clean logic, and `Tools/StickyNotesTools.cs` is the single MCP tool surface.

## Conventions

- **Interface-first / SOLID.** Consumers depend on interfaces (`IStickyNoteRepository`, `INoteTextCleaner`, `IDatabaseSnapshotFactory`), not concrete types, so each piece is independently testable. Follow this pattern for new components.
- **Nullable and implicit usings are enabled** project-wide — write code that's clean under both.
- **Never write to STDOUT.** STDOUT is the MCP JSON-RPC channel. A stray `Console.WriteLine` (or any library that logs to stdout by default) silently corrupts the protocol stream and the client will fail to see the server register, with no obvious error. All logging goes to STDERR — see the comment at the top of [src/Program.cs](src/Program.cs).
- **Add unit tests** for new behavior, following the existing patterns in `tests/` (xUnit, in-memory/temp SQLite for repository tests).

## Commit style

Commits use gitmoji + a Conventional Commits-style scope, e.g.:

```
✨ feat(data): implement copy-then-read strategy for Sticky Notes DB
🧪 test: add unit tests for NoteTextCleaner and StickyNoteRepository
📝 docs: add README, architecture, configuration, privacy, and troubleshooting documentation
```

## Submitting changes

1. Create a branch off `main`.
2. Make your change, with tests where it makes sense.
3. Make sure `dotnet test src\StickyNotesMcp.slnx -c Release` passes locally — the same command CI runs.
4. Open a pull request against `main`. CI (`.github/workflows/ci.yml`) builds and tests on Windows automatically.

## Cutting a release

See [docs/releasing.md](docs/releasing.md) — that's a maintainer task, not something contributors need for a normal PR.
