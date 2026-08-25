# StickyNotesMcp

A read-only [Model Context Protocol](https://modelcontextprotocol.io) server that exposes your **actual Windows Sticky Notes** to MCP clients like Claude Desktop, Claude Code, and Codex, so an AI assistant can read your notes and help you act on them.

> **This reads the real app.** Unlike most "sticky notes MCP" demos — which store notes in their own text file — this server reads the live `plum.sqlite` database that the Microsoft Sticky Notes app actually uses. The notes synced to your Microsoft account are what get surfaced.

## Features

- Exposes a single read-only tool, **`get_sticky_notes`**, over stdio.
- Reads the **real** Microsoft Sticky Notes database (`plum.sqlite`), not a mock store.
- Returns your current, non-deleted notes as **structured JSON** (`text` + `updatedAt`), most-recently-updated first, with the app's formatting markers stripped to clean text.
- **Safe by design:** copy-then-read (never opens the live DB except to copy it) and strictly read-only.
- SOLID .NET 10 codebase with unit tests.

## Requirements

- Windows with the Microsoft Sticky Notes app installed (schema verified against version **6.x**).
- An MCP client (Claude Desktop, Claude Code, Codex CLI, etc.).

No .NET runtime install is required — the prebuilt release below is self-contained.

## Installation

### 1. Download

Grab the latest `StickyNotesMcp-win-x64.exe` (or `StickyNotesMcp-win-arm64.exe` for ARM64 Windows devices) from the [Releases page](https://github.com/jrybacek/windows-sticky-notes-mcp/releases).

Put it somewhere stable — it stays in place after setup, so a temp/Downloads folder isn't a good fit:

```powershell
New-Item -ItemType Directory -Force "$env:LOCALAPPDATA\Programs\StickyNotesMcp" | Out-Null
Move-Item ~\Downloads\StickyNotesMcp-win-x64.exe "$env:LOCALAPPDATA\Programs\StickyNotesMcp\"
```

Then verify and unblock it:

```powershell
$exe = "$env:LOCALAPPDATA\Programs\StickyNotesMcp\StickyNotesMcp-win-x64.exe"

# Compare against the .sha256 file published alongside the release asset
Get-FileHash -Algorithm SHA256 $exe

# The exe is downloaded from the internet, and unsigned - Windows will otherwise
# block or SmartScreen-warn on first run.
Unblock-File $exe
```

> **Unsigned binary:** these release assets are not code-signed. SmartScreen may warn on
> first run — verify the SHA-256 against the published `.sha256` file before trusting it,
> then choose *More info → Run anyway*. See [Troubleshooting](docs/troubleshooting.md) if
> Windows blocks it outright.

Prefer to build it yourself instead? See [CONTRIBUTING.md](CONTRIBUTING.md).

### 2. Connect your client

Pick your client below. `sticky-notes` is just a suggested server name — use whatever you like, as long as it's consistent within one config.

#### Claude Code

```powershell
claude mcp add --scope user --transport stdio sticky-notes -- "$env:LOCALAPPDATA\Programs\StickyNotesMcp\StickyNotesMcp-win-x64.exe"
```

`--scope user` makes the server available in every project, and writes the entry to `%USERPROFILE%\.claude.json`. The `--` before the path is required — without it, Claude Code tries to parse the path as its own flags.

Equivalent manual edit to `%USERPROFILE%\.claude.json`, under the top-level `mcpServers` object:

```json
{
  "mcpServers": {
    "sticky-notes": {
      "type": "stdio",
      "command": "C:\\Users\\you\\AppData\\Local\\Programs\\StickyNotesMcp\\StickyNotesMcp-win-x64.exe",
      "args": [],
      "env": {}
    }
  }
}
```

> Note: MCP servers go in `.claude.json`, **not** `settings.json` — the settings schema has no MCP server field. Restart Claude Code, then run `/mcp` to confirm the `sticky-notes` server is connected.

#### Claude Desktop

> **Microsoft Store / MSIX build:** the in-app **Settings → Edit Config** button opens `%APPDATA%\Claude\claude_desktop_config.json`, but the Store build does **not** read that file — it reads a *virtualized* copy. Edit the virtualized path below, or your server will silently never appear.

1. Open (creating it if needed) the config file at the path the MSIX build actually reads:

   ```text
   %LOCALAPPDATA%\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\claude_desktop_config.json
   ```

   Save it as valid JSON, **UTF-8 without a BOM**. (If you use the standalone, non-Store `.exe` build instead, its config lives at `%APPDATA%\Claude\claude_desktop_config.json`.)

2. Add the server, pointing `command` at the exe you downloaded:

   ```json
   {
     "mcpServers": {
       "sticky-notes": {
         "command": "C:\\Users\\you\\AppData\\Local\\Programs\\StickyNotesMcp\\StickyNotesMcp-win-x64.exe"
       }
     }
   }
   ```

3. **Fully quit Claude Desktop, then relaunch it.** Closing the window is *not* enough — Claude Desktop keeps running in the system tray and will not reload the config. Right-click the tray icon → **Quit** (or end every `Claude.exe` in Task Manager, or run `taskkill /IM Claude.exe /F`), then start it again. Only a full restart makes it re-read the config and launch the stdio server.

4. **Verify:** `Settings → Developer` lists `sticky-notes`, and a `mcp-server-sticky-notes.log` file appears. Then ask *"What's on my sticky notes?"*

#### Codex CLI

```powershell
codex mcp add sticky-notes -- "$env:LOCALAPPDATA\Programs\StickyNotesMcp\StickyNotesMcp-win-x64.exe"
```

Equivalent manual edit to `%USERPROFILE%\.codex\config.toml`:

```toml
[mcp_servers.sticky-notes]
command = 'C:\Users\you\AppData\Local\Programs\StickyNotesMcp\StickyNotesMcp-win-x64.exe'
args = []
```

> Use a single-quoted TOML string (`'...'`) for the path. TOML's single-quoted "literal"
> strings don't process backslash escapes, so a Windows path doesn't need doubled
> backslashes there — a double-quoted string would need `\\` for every `\`.

Restart Codex, then confirm the server is connected (`/mcp` in the Codex CLI, or check its MCP status output).

### 3. Verify

Ask your assistant *"What's on my sticky notes?"* — it should call `get_sticky_notes` and return your current notes.

## Documentation

- [Architecture](docs/architecture.md) — the safe copy-then-read strategy and the SOLID component design.
- [Configuration](docs/configuration.md) — database location options and customizing the schema for other Sticky Notes versions.
- [Privacy & scope](docs/privacy-and-scope.md) — what the server deliberately does *not* do, and how your note data flows.
- [Troubleshooting](docs/troubleshooting.md) — garbled output, resurfacing notes, registration failures, "database not found".
- [Releasing](docs/releasing.md) — maintainer runbook for cutting a new release.

## Contributing

Want to build from source, run the tests, or open a PR? See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

MIT — see [LICENSE](LICENSE).

Not affiliated with or endorsed by Microsoft. Reads an undocumented local database whose format may change without notice in any Sticky Notes update. Provided as-is; verify against your own data before depending on it.
