namespace StickyNotesMcp.Services;

/// <summary>
/// Normalizes a raw Sticky Notes body into readable plain text. Isolated behind an interface so
/// the formatting rules can evolve (or be swapped) without touching the repository.
/// </summary>
public interface INoteTextCleaner
{
    /// <summary>Strips formatting control words and normalizes whitespace.</summary>
    string Clean(string raw);
}
