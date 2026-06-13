using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using StickyNotesMcp.Models;
using StickyNotesMcp.Repositories;

namespace StickyNotesMcp.Tools;

/// <summary>
/// MCP protocol adapter. This type is intentionally thin: it owns no logic beyond translating a
/// tool call into a repository read and letting failures surface as a tool error. Dependencies are
/// injected per call from the request service provider.
/// </summary>
[McpServerToolType]
public static class StickyNotesTools
{
    // Wire name pinned to the MCP snake_case convention (this is also what the SDK derives from the
    // method name by default); the .NET method stays PascalCase. Pinned so it can't drift if the
    // SDK's naming default ever changes.
    [McpServerTool(Name = "get_sticky_notes")]
    [Description("Returns the user's current (non-deleted) Windows Sticky Notes as structured JSON " +
                 "(text + last-updated timestamp), most-recently-updated first. Read-only.")]
    public static async Task<IReadOnlyList<StickyNote>> GetStickyNotes(
        IStickyNoteRepository repository,
        ILogger<object> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            return await repository.GetActiveNotesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log to stderr (stdout is the JSON-RPC channel) and let the SDK report a tool error.
            logger.LogError(ex, "Failed to read sticky notes.");
            throw;
        }
    }
}
