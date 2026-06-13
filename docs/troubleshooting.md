# Troubleshooting

- **Output looks like garbage / has `\b`, `\ul`, `\id=` markers.** The body isn't clean plaintext in 6.x. Expand `NoteTextCleaner` in [`src/Services/NoteTextCleaner.cs`](../src/Services/NoteTextCleaner.cs) to strip whatever wrapping your version uses.
- **Finished notes keep showing up.** Your soft-delete column is wrong for your version — edit the `[Column("DeletedAt")]` attribute on [`src/Entities/NoteEntity.cs`](../src/Entities/NoteEntity.cs) and rebuild.
- **Server never registers / silently fails.** Never write to STDOUT — stdio is the JSON-RPC channel. All logging goes to STDERR (already configured). Debug with the [MCP Inspector](https://github.com/modelcontextprotocol/inspector).
- **Server doesn't appear in Claude Desktop after editing the config.** You must *fully* quit Claude Desktop (tray icon → **Quit**, or kill `Claude.exe`) — not just close the window — for it to reload the config. Also confirm you edited the MSIX virtualized path (`%LOCALAPPDATA%\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\`), not `%APPDATA%\Claude`. See [KNOWN_ISSUES.md](../KNOWN_ISSUES.md).
- **"Database not found."** Confirm the package folder exists; the GUID `8wekyb3d8bbwe` is the standard Microsoft Store publisher ID, but verify the path, or set `StickyNotes__SourceDirectory`.
