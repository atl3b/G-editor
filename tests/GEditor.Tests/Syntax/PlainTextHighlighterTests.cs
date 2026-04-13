using GEditor.Syntax;
using Xunit;

namespace GEditor.Tests.Syntax;

public class PlainTextHighlighterTests
{
    private readonly PlainTextHighlighter _highlighter = new();

    [Fact]
    public void HighlightLine_ReturnsSinglePlainTextToken()
    {
        var tokens = _highlighter.HighlightLine("hello world", 0);

        Assert.Single(tokens);
        Assert.Equal(TokenKind.PlainText, tokens[0].Kind);
        Assert.Equal("hello world", tokens[0].Text);
        Assert.Equal(11, tokens[0].Length);
    }

    [Fact]
    public void HighlightLine_EmptyLine_ReturnsEmpty()
    {
        var tokens = _highlighter.HighlightLine("", 0);
        Assert.Empty(tokens);
    }

    [Fact]
    public void HighlightDocument_MultipleLines()
    {
        var lines = new[] { "line1", "line2", "line3" };
        var result = _highlighter.HighlightDocument(lines);

        Assert.Equal(3, result.LineCount);
        Assert.All(result.LineTokens, lineTokens =>
        {
            Assert.Single(lineTokens);
            Assert.Equal(TokenKind.PlainText, lineTokens[0].Kind);
        });
    }

    [Fact]
    public void SupportedExtensions()
    {
        Assert.Contains(".txt", _highlighter.SupportedExtensions);
        Assert.Contains(".log", _highlighter.SupportedExtensions);
        Assert.Contains(".md", _highlighter.SupportedExtensions);
    }

    [Fact]
    public void LanguageName()
    {
        Assert.Equal("Plain Text", _highlighter.LanguageName);
    }
}
