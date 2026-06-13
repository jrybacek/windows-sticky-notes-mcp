namespace StickyNotesMcp.Data;

/// <summary>
/// Creates a safe, read-only copy of the live Sticky Notes database. Abstracting this lets the
/// repository depend on the copy-then-read policy without knowing how it is implemented.
/// </summary>
public interface IDatabaseSnapshotFactory
{
    /// <summary>
    /// Copies the live database and its WAL/SHM sidecars into a fresh temporary directory and
    /// returns a disposable snapshot pointing at the copy.
    /// </summary>
    DatabaseSnapshot CreateSnapshot();
}
