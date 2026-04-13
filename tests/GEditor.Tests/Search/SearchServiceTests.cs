using GEditor.Core.Buffer;
using GEditor.Core.Search;
using Xunit;

namespace GEditor.Tests.Search;

public class SearchServiceTests
{
    private readonly SearchService _service = new();

    private EditorBuffer CreateBuffer(string text)
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText(text);
        return buffer;
    }

    [Fact]
    public void FindAll_BasicSearch_FindsMatches()
    {
        var buffer = CreateBuffer("hello world hello");
        var query = SearchQuery.Create("hello");
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public void FindAll_CaseSensitive_FindsExactMatch()
    {
        var buffer = CreateBuffer("Hello hello");
        var query = SearchQuery.Create("Hello", SearchOptions.CaseSensitive);
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
        Assert.Equal("Hello", matches[0].MatchedText);
    }

    [Fact]
    public void FindAll_CaseInsensitive_FindsAll()
    {
        var buffer = CreateBuffer("Hello hello HELLO");
        var query = SearchQuery.Create("hello");
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(3, matches.Count);
    }

    [Fact]
    public void FindAll_WholeWord_MatchesWholeWordsOnly()
    {
        var buffer = CreateBuffer("hello helloworld");
        var query = SearchQuery.Create("hello", SearchOptions.WholeWordOnly);
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
    }

    [Fact]
    public void FindAll_WholeWord_NoMatchForSubstring()
    {
        var buffer = CreateBuffer("helloworld");
        var query = SearchQuery.Create("hello", SearchOptions.WholeWordOnly);
        var matches = _service.FindAll(buffer, query);

        Assert.Empty(matches);
    }

    [Fact]
    public void FindAll_EmptyPattern_ReturnsEmpty()
    {
        var buffer = CreateBuffer("hello world");
        var query = SearchQuery.Create("");
        var matches = _service.FindAll(buffer, query);

        Assert.Empty(matches);
    }

    [Fact]
    public void FindAll_NoMatch_ReturnsEmpty()
    {
        var buffer = CreateBuffer("hello world");
        var query = SearchQuery.Create("xyz");
        var matches = _service.FindAll(buffer, query);

        Assert.Empty(matches);
    }

    [Fact]
    public void FindAll_MultiLine_FindsCorrectLine()
    {
        var buffer = CreateBuffer("line1\nline2\nfindme\nline4");
        var query = SearchQuery.Create("findme");
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
        Assert.Equal(2, matches[0].Line);
    }

    [Fact]
    public void FindAll_SameLineMultipleMatches()
    {
        var buffer = CreateBuffer("aa bb aa cc aa");
        var query = SearchQuery.Create("aa");
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(3, matches.Count);
        Assert.Equal(0, matches[0].Column);
        Assert.Equal(6, matches[1].Column);
        Assert.Equal(12, matches[2].Column);
    }

    [Fact]
    public void FindAll_MatchContainsLineText()
    {
        var buffer = CreateBuffer("hello world");
        var query = SearchQuery.Create("hello");
        var matches = _service.FindAll(buffer, query);

        Assert.Equal("hello world", matches[0].LineText);
    }

    [Fact]
    public void CountMatches_EqualsFindAllCount()
    {
        var buffer = CreateBuffer("hello world hello");
        var query = SearchQuery.Create("hello");

        var count = _service.CountMatches(buffer, query);
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(matches.Count, count);
    }

    [Fact]
    public void FindNext_FindsNextAfterPosition()
    {
        var buffer = CreateBuffer("aa bb aa cc aa");
        var query = SearchQuery.Create("aa");
        var match = _service.FindNext(buffer, query, 0, 1);

        Assert.NotNull(match);
        Assert.Equal(6, match!.Column);
    }

    [Fact]
    public void FindPrevious_FindsPreviousBeforePosition()
    {
        var buffer = CreateBuffer("aa bb aa cc aa");
        var query = SearchQuery.Create("aa");
        var match = _service.FindPrevious(buffer, query, 2, 0);

        Assert.NotNull(match);
        Assert.Equal(12, match!.Column); // Last "aa" before (2, 0) is at column 12
    }

    [Fact]
    public void FindAll_NullBuffer_ReturnsEmpty()
    {
        var matches = _service.FindAll(null!, SearchQuery.Create("test"));
        Assert.Empty(matches);
    }

    [Fact]
    public void CreateReplaceAllCommand_CreatesCompositeCommand()
    {
        var buffer = CreateBuffer("hello world hello");
        var query = SearchQuery.Create("hello");
        var command = _service.CreateReplaceAllCommand(buffer, query, "hi");

        Assert.NotNull(command);
        Assert.Contains("Replace All", command.Description);
    }
}
