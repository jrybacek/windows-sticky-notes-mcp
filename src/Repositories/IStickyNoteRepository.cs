using StickyNotesMcp.Models;

namespace StickyNotesMcp.Repositories;

/// <summary>
/// Reads active sticky notes from the underlying store. Consumers (the MCP tool) depend only on
/// this abstraction, not on SQLite or the snapshot mechanism.
/// </summary>
public interface IStickyNoteRepository
{
    /// <summary>
    /// Returns the user's non-deleted notes with formatting stripped, most-recently-updated first.
    /// </summary>
    Task<IReadOnlyList<StickyNote>> GetActiveNotesAsync(CancellationToken cancellationToken = default);
}
