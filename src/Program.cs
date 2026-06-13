using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StickyNotesMcp.Configuration;
using StickyNotesMcp.Data;
using StickyNotesMcp.Repositories;
using StickyNotesMcp.Services;

var builder = Host.CreateApplicationBuilder(args);

// STDOUT is the stdio JSON-RPC channel. Every log line MUST go to STDERR or it corrupts the
// protocol stream and the server silently fails to register. Clear the default providers and add a
// console logger whose threshold routes all levels to stderr.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

// Schema/path mapping is bindable from the "StickyNotes" configuration section (Open/Closed seam).
builder.Services.Configure<StickyNotesOptions>(
    builder.Configuration.GetSection(StickyNotesOptions.SectionName));

builder.Services.AddSingleton<IDatabaseSnapshotFactory, DatabaseSnapshotFactory>();
builder.Services.AddSingleton<INoteTextCleaner, NoteTextCleaner>();
builder.Services.AddSingleton<IStickyNoteRepository, StickyNoteRepository>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
