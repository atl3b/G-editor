using GEditor.Core.Buffer;
using GEditor.Core.Editing;
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

    #region ReplaceAll 执行验证

    [Fact]
    public void ReplaceAll_Execute_ReplacesAllOccurrences()
    {
        var buffer = CreateBuffer("hello world hello hello");
        var query = SearchQuery.Create("hello");
        var command = _service.CreateReplaceAllCommand(buffer, query, "hi");

        command.Execute(buffer);

        Assert.Equal("hi world hi hi", buffer.GetAllText("\n"));
    }

    [Fact]
    public void ReplaceAll_Execute_SingleOccurrence()
    {
        var buffer = CreateBuffer("hello world");
        var query = SearchQuery.Create("hello");
        var command = _service.CreateReplaceAllCommand(buffer, query, "hi");

        command.Execute(buffer);

        Assert.Equal("hi world", buffer.GetAllText("\n"));
    }

    [Fact]
    public void ReplaceAll_Execute_NoMatch_DoesNothing()
    {
        var buffer = CreateBuffer("hello world");
        var query = SearchQuery.Create("xyz");
        var command = _service.CreateReplaceAllCommand(buffer, query, "replacement");

        command.Execute(buffer);

        Assert.Equal("hello world", buffer.GetAllText("\n"));
    }

    [Fact]
    public void ReplaceAll_Undo_RestoresOriginalContent()
    {
        var buffer = CreateBuffer("hello world hello");
        var originalText = buffer.GetAllText("\n");
        var query = SearchQuery.Create("hello");
        var command = _service.CreateReplaceAllCommand(buffer, query, "hi");

        command.Execute(buffer);
        // Verify replacement happened
        Assert.NotEqual(originalText, buffer.GetAllText("\n"));

        command.Undo(buffer);
        // Verify undo restores original
        Assert.Equal(originalText, buffer.GetAllText("\n"));
    }

    [Fact]
    public void ReplaceAll_MultiLine_ReplacesOnMultipleLines()
    {
        var buffer = CreateBuffer("find me\nfind me here\nand find me too");
        var query = SearchQuery.Create("find me");
        var command = _service.CreateReplaceAllCommand(buffer, query, "found it");

        command.Execute(buffer);

        Assert.Equal("found it\nfound it here\nand found it too", buffer.GetAllText("\n"));
    }

    [Fact]
    public void ReplaceAll_CaseSensitive_OnlyReplacesExactCase()
    {
        var buffer = CreateBuffer("Hello hello HELLO");
        var query = SearchQuery.Create("Hello", SearchOptions.CaseSensitive);
        var command = _service.CreateReplaceAllCommand(buffer, query, "Hi");

        command.Execute(buffer);

        Assert.Equal("Hi hello HELLO", buffer.GetAllText("\n"));
    }

    [Fact]
    public void ReplaceAll_WholeWord_DoesNotReplaceSubstring()
    {
        var buffer = CreateBuffer("hello helloworld hellothere");
        var query = SearchQuery.Create("hello", new SearchOptions { WholeWord = true });
        var command = _service.CreateReplaceAllCommand(buffer, query, "hi");

        command.Execute(buffer);

        Assert.Equal("hi helloworld hellothere", buffer.GetAllText("\n"));
    }

    #endregion

    #region 正则模式 + 特殊字符转义

    [Fact]
    public void RegexMode_SimplePattern_FindsMatches()
    {
        var buffer = CreateBuffer("abc123 def456");
        var query = SearchQuery.Create(@"\d+", new SearchOptions { UseRegex = true });
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(2, matches.Count);
        Assert.Equal("123", matches[0].MatchedText);
        Assert.Equal("456", matches[1].MatchedText);
    }

    [Fact]
    public void RegexMode_DotWildcard_MatchesAnyChar()
    {
        var buffer = CreateBuffer("abc Xyc aZc");
        var query = SearchQuery.Create(@"a.c", new SearchOptions { UseRegex = true });
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(2, matches.Count);
        Assert.Equal("abc", matches[0].MatchedText);
        Assert.Equal("aZc", matches[1].MatchedText);
    }

    [Fact]
    public void RegexMode_StarQuantifier_MatchesZeroOrMore()
    {
        var buffer = CreateBuffer("ac abc abbc");
        var query = SearchQuery.Create(@"ab*c", new SearchOptions { UseRegex = true });
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(3, matches.Count);
    }

    [Fact]
    public void RegexMode_CaseInsensitiveFlag_ObeysOption()
    {
        var buffer = CreateBuffer("Hello HELLO hello");
        // UseRegex + MatchCase=false（默认）→ 不区分大小写
        var query = SearchQuery.Create(@"hello", new SearchOptions { UseRegex = true });
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(3, matches.Count);
    }

    [Fact]
    public void RegexMode_CaseSensitiveFlag_OnlyExactCase()
    {
        var buffer = CreateBuffer("Hello HELLO hello");
        var query = SearchQuery.Create(@"hello", new SearchOptions { UseRegex = true, MatchCase = true });
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
        Assert.Equal("hello", matches[0].MatchedText);
    }

    [Fact]
    public void NonRegexMode_SpecialChars_EscapedAutomatically()
    {
        var buffer = CreateBuffer("price: $10.00 cost: $5.50 (discount)");
        // 非正则模式下，$.() 等应被当作字面量
        var query = SearchQuery.Create("$10.00");  // 不启用 UseRegex
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
        Assert.Equal("$10.00", matches[0].MatchedText);
    }

    [Fact]
    public void NonRegexMode_Dot_IsLiteralDot()
    {
        var buffer = CreateBuffer("file.txt fileXtxt file.txt");
        var query = SearchQuery.Create("file.txt");
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(2, matches.Count); // 只匹配 "file.txt"，不匹配 "fileXtxt"
    }

    [Fact]
    public void NonRegexMode_Parentheses_AreLiteral()
    {
        var buffer = CreateBuffer("(hello) and (world)");
        var query = SearchQuery.Create("(hello)");
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
        Assert.Equal("(hello)", matches[0].MatchedText);
    }

    [Fact]
    public void NonRegexMode_Brackets_AreLiteral()
    {
        var buffer = CreateBuffer("arr[0] arr[1] arr[x]");
        var query = SearchQuery.Create("[0]");
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
    }

    [Fact]
    public void NonRegexMode_PlusAndStar_AndQuestionMark_AreLiteral()
    {
        var buffer = CreateBuffer("1+1=2 2*3=6 3?5 unknown");
        var query = SearchQuery.Create("1+1");
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
        Assert.Equal("1+1", matches[0].MatchedText);
    }

    [Fact]
    public void NonRegexMode_Backslash_IsLiteralBackslash()
    {
        var buffer = CreateBuffer(@"path\to\file path/to/file");
        var query = SearchQuery.Create(@"path\to");
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
    }

    [Fact]
    public void NonRegexMode_CaretDollar_AreLiteralAtLineStartEnd()
    {
        var buffer = CreateBuffer("^start line^ end$ here$");
        var query = SearchQuery.Create("^start");
        var matches = _service.FindAll(buffer, query);

        Assert.Single(matches);
        Assert.Equal("^start", matches[0].MatchedText);
    }

    [Fact]
    public void RegexMode_PipeAlternation_WorksCorrectly()
    {
        var buffer = CreateBuffer("cat dog fish catfish");
        var query = SearchQuery.Create(@"cat|dog", new SearchOptions { UseRegex = true });
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(3, matches.Count);
    }

    [Fact]
    public void RegexMode_CharacterClass_WorksCorrectly()
    {
        var buffer = CreateBuffer("a1 b2 c3 x y z");
        var query = SearchQuery.Create(@"[a-c][0-3]", new SearchOptions { UseRegex = true });
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(3, matches.Count);
    }

    [Fact]
    public void RegexMode_Anchor_StartOfLine()
    {
        var buffer = CreateBuffer("line1\nstart here\nstart again");
        var query = SearchQuery.Create(@"^start", new SearchOptions { UseRegex = true });
        var matches = _service.FindAll(buffer, query);

        Assert.Equal(2, matches.Count); // 匹配第 2、3 行开头的 start
    }

    #endregion
}
