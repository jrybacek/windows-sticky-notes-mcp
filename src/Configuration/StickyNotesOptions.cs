namespace StickyNotesMcp.Configuration;

/// <summary>
/// Locates the Sticky Notes database. Bindable from the "StickyNotes" configuration section /
/// environment variables so the path can be overridden per machine without recompiling. The note
/// table and column names are mapped via attributes on <c>NoteEntity</c>, not here.
/// </summary>
public sealed class StickyNotesOptions
{
    public const string SectionName = "StickyNotes";

    /// <summary>
    /// Directory containing <c>plum.sqlite</c>. When null/empty, the default per-user Sticky Notes
    /// LocalState path is used.
    /// </summary>
    public string? SourceDirectory { get; set; }

    /// <summary>File name of the Sticky Notes database.</summary>
    public string DatabaseFileName { get; set; } = "plum.sqlite";

    /// <summary>Resolves the effective source directory, falling back to the default install path.</summary>
    public string ResolveSourceDirectory() =>
        string.IsNullOrWhiteSpace(SourceDirectory)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages", "Microsoft.MicrosoftStickyNotes_8wekyb3d8bbwe", "LocalState")
            : SourceDirectory;
}
