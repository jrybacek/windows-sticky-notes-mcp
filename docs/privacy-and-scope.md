# Privacy & scope

## Why it deliberately does **not** do some things

- **No writes.** Read-only by design. The Sticky Notes app holds note state in memory and flushes on its own schedule; writing to `plum.sqlite` while the app runs risks the app overwriting your change, corrupting its sync journal, or pushing a conflicting state to your Microsoft account. If you want a note gone, delete it in the app.
- **No cloud API.** Microsoft does not expose Sticky Notes through Microsoft Graph (the OneNote Graph API covers notebooks/sections/pages, not Sticky Notes). The sync backend is undocumented, so this server works against the **local** database only.

## Privacy

This server sends no data anywhere on its own. When your MCP client calls `get_sticky_notes`, the returned note text is sent to that client's LLM provider as part of the conversation — the same as any other tool output. Your notes leave your machine only to the extent your AI client sends tool results to its model.
