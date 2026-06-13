using StickyNotesMcp.Services;
using Xunit;

namespace StickyNotesMcp.Tests;

public class NoteTextCleanerTests
{
    private readonly NoteTextCleaner _cleaner = new();

    [Fact]
    public void Clean_StripsControlWordsAndBlockId_KeepsContent()
    {
        // \id=<guid> block marker, \b/\b0 bold, \ul/\ul0 underline — all should be removed.
        var raw = @"\id=c03705aa-2166-4faa-b653-2db074e21636\b\ul Today\b0\ul0";

        Assert.Equal("Today", _cleaner.Clean(raw));
    }

    [Fact]
    public void Clean_PreservesNewlinesBetweenBlocks_AndStripsBullets()
    {
        var raw = "\\id=aaaa\\b Today\\b0\n\\id=bbbb\\l buy milk";

        Assert.Equal("Today\nbuy milk", _cleaner.Clean(raw));
    }

    [Fact]
    public void Clean_NormalizesCarriageReturnsAndCollapsesSpaces()
    {
        var raw = "\\b first\\b0\r\n\\i  second   word\\i0";

        Assert.Equal("first\nsecond word", _cleaner.Clean(raw));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\\b\\b0")]
    public void Clean_BlankOrMarkupOnly_ReturnsEmpty(string raw)
    {
        Assert.Equal(string.Empty, _cleaner.Clean(raw));
    }
}
