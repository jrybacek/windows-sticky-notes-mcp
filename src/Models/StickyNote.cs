namespace StickyNotesMcp.Models;

/// <summary>
/// A single active sticky note, as surfaced to MCP clients. Serialized to JSON by the MCP SDK.
/// </summary>
/// <param name="Text">The note body with formatting control words stripped.</param>
/// <param name="UpdatedAt">When the note was last updated (UTC), or null if unknown.</param>
public sealed record StickyNote(string Text, DateTimeOffset? UpdatedAt);
