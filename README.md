# StickyNotesMcp

A read-only [Model Context Protocol](https://modelcontextprotocol.io) server that exposes your **actual Windows Sticky Notes** to MCP clients like Claude Desktop, so an AI assistant can read your notes and help you act on them.

> **This reads the real app.** Unlike most "sticky notes MCP" demos — which store notes in their own text file — this server reads the live `plum.sqlite` database that the Microsoft Sticky Notes app actually uses. The notes synced to your Microsoft account are what get surfaced.

## Features

- Exposes a single read-only tool, **`get_sticky_notes`**, over stdio.
- Reads the **real** Microsoft Sticky Notes database (`plum.sqlite`), not a mock store.
- Returns your current, non-deleted notes as **structured JSON** (`text` + `updatedAt`), most-recently-updated first, with the app's formatting markers stripped to clean text.
- **Safe by design:** copy-then-read (never opens the live DB except to copy it) and strictly read-only.
- SOLID .NET 10 codebase with unit tests.

## Software requirements

- Windows with the Microsoft Sticky Notes app installed (schema verified against version **6.x**).
- [.NET 10 SDK](https://dotnet.microsoft.com/download).
- An MCP client (Claude Desktop, Claude Code, etc.).

## Installation

### Build

```powershell
# 1. Clone
git clone https://github.com/<you>/windows-sticky-notes-mcp.git
cd windows-sticky-notes-mcp
cd src

# 2. Build (produces src\bin\Release\net10.0\StickyNotesMcp.exe)
dotnet build -c Release

# 3. (optional) Run the tests
dotnet test -c Release
```

Then point your MCP client at the built exe.

### Configure Claude Desktop

> **Microsoft Store / MSIX build:** the in-app **Settings → Edit Config** button opens `%APPDATA%\Claude\claude_desktop_config.json`, but the Store build does **not** read that file — it reads a *virtualized* copy. Edit the virtualized path below, or your server will silently never appear.

1. Open (creating it if needed) the config file at the path the MSIX build actually reads:

   ```text
   %LOCALAPPDATA%\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\claude_desktop_config.json
   ```

   Save it as valid JSON, **UTF-8 without a BOM**. (If you use the standalone, non-Store `.exe` build instead, its config lives at `%APPDATA%\Claude\claude_desktop_config.json`.)

2. Add the server, pointing `command` at the absolute path of the exe you built:

   ```json
   {
     "mcpServers": {
       "sticky-notes": {
         "command": "C:\\path\\to\\windows-sticky-notes-mcp\\src\\bin\\Release\\net10.0\\StickyNotesMcp.exe"
       }
     }
   }
   ```

3. **Fully quit Claude Desktop, then relaunch it.** Closing the window is *not* enough — Claude Desktop keeps running in the system tray and will not reload the config. Right-click the tray icon → **Quit** (or end every `Claude.exe` in Task Manager, or run `taskkill /IM Claude.exe /F`), then start it again. Only a full restart makes it re-read the config and launch the stdio server.

4. **Verify:** `Settings → Developer` lists `sticky-notes`, and a `mcp-server-sticky-notes.log` file appears. Then ask *"What's on my sticky notes?"*

### Configure Claude Code

Edit your user config at `%USERPROFILE%\.claude.json` (i.e. `C:\Users\<you>\.claude.json`) and add a `sticky-notes` entry under the top-level `mcpServers` object:

```json
{
  "mcpServers": {
    "sticky-notes": {
      "type": "stdio",
      "command": "C:\\path\\to\\windows-sticky-notes-mcp\\src\\bin\\Release\\net10.0\\StickyNotesMcp.exe",
      "args": [],
      "env": {}
    }
  }
}
```

> Note: MCP servers go in `.claude.json`, **not** `settings.json` — the settings schema has no MCP server field. Restart Claude Code, then run `/mcp` to confirm the `sticky-notes` server is connected.

## Documentation

- [Architecture](docs/architecture.md) — the safe copy-then-read strategy and the SOLID component design.
- [Configuration](docs/configuration.md) — database location options and customizing the schema for other Sticky Notes versions.
- [Privacy & scope](docs/privacy-and-scope.md) — what the server deliberately does *not* do, and how your note data flows.
- [Troubleshooting](docs/troubleshooting.md) — garbled output, resurfacing notes, registration failures, "database not found".

## License

MIT — see [LICENSE](LICENSE).

Not affiliated with or endorsed by Microsoft. Reads an undocumented local database whose format may change without notice in any Sticky Notes update. Provided as-is; verify against your own data before depending on it.
