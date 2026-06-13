using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StickyNotesMcp.Entities;

/// <summary>
/// Read-only projection of a row in the Sticky Notes note table. Mapped as a keyless EF entity
/// (no key, no tracking, no writes — matching the read-only contract). The table and column names
/// are the defaults verified against Sticky Notes 6.x; if a future version differs, retarget them
/// by editing these attributes and rebuilding.
/// </summary>
[Keyless]
[Table("Note")]
public sealed class NoteEntity
{
    [Column("Text")]
    public string? Text { get; set; }

    /// <summary>Last-updated time as .NET <see cref="DateTime"/> ticks.</summary>
    [Column("UpdatedAt")]
    public long? UpdatedAt { get; set; }

    /// <summary>Soft-delete marker; null for active notes.</summary>
    [Column("DeletedAt")]
    public long? DeletedAt { get; set; }
}
