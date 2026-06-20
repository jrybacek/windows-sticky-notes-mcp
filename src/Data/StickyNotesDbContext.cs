using Microsoft.EntityFrameworkCore;
using StickyNotesMcp.Entities;

namespace StickyNotesMcp.Data;

/// <summary>
/// EF Core context over a read-only snapshot of the Sticky Notes database. The note table is mapped
/// as a keyless query type (no tracking, no writes — matching the read-only contract) via the
/// attributes on <see cref="NoteEntity"/>.
/// </summary>
public sealed class StickyNotesDbContext(DbContextOptions<StickyNotesDbContext> options) : DbContext(options)
{
    public DbSet<NoteEntity> Notes => Set<NoteEntity>();
}
