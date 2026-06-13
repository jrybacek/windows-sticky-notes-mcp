using System.Text.RegularExpressions;

namespace StickyNotesMcp.Services;

/// <summary>
/// Sticky Notes 6.x stores the body with RTF-like control words rather than clean plaintext, e.g.:
/// <code>\id=&lt;guid&gt; \b bold \b0 \ul underline \ul0 \i italic \i0 \l list-bullet</code>
/// Real newlines separate content blocks. This cleaner removes the control words and collapses the
/// whitespace they leave behind, while preserving line breaks.
/// </summary>
public sealed partial class NoteTextCleaner : INoteTextCleaner
{
    public string Clean(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return string.Empty;

        var text = raw.Replace("\r\n", "\n").Replace('\r', '\n');
        text = ControlWordRegex().Replace(text, " ");

        // Collapse runs of spaces/tabs within a line and trim each line, but keep the newlines
        // that separate note blocks.
        var lines = text
            .Split('\n')
            .Select(line => SpaceRunRegex().Replace(line, " ").Trim());

        return string.Join("\n", lines).Trim();
    }

    // A control word: backslash, letters, optional trailing digits (\b0), optional =value (\id=guid).
    // The value stops at the next backslash so back-to-back control words (\id=guid\b) are handled.
    [GeneratedRegex(@"\\[a-zA-Z]+\d*(?:=[^\s\\]+)?")]
    private static partial Regex ControlWordRegex();

    [GeneratedRegex(@"[ \t]{2,}")]
    private static partial Regex SpaceRunRegex();
}
