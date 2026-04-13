using GEditor.Syntax;
using Xunit;

namespace GEditor.Tests.Syntax;

public class JsonHighlighterTests
{
    private readonly JsonSyntaxHighlighter _highlighter = new();

    [Fact]
    public void HighlightLine_StringKeyAndValue()
    {
        var tokens = _highlighter.HighlightLine(@"""name"": ""John""", 0);

        var strings = tokens.Where(t => t.Kind == TokenKind.String).ToList();
        Assert.Equal(2, strings.Count);
    }

    [Fact]
    public void HighlightLine_Number()
    {
        var tokens = _highlighter.HighlightLine(@"""age"": 30", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Number && t.Text == "30");
    }

    [Fact]
    public void HighlightLine_Boolean()
    {
        var tokens = _highlighter.HighlightLine(@"""active"": true", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Keyword && t.Text == "true");
    }

    [Fact]
    public void HighlightLine_Null()
    {
        var tokens = _highlighter.HighlightLine(@"""value"": null", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Keyword && t.Text == "null");
    }

    [Fact]
    public void HighlightLine_Nested()
    {
        var tokens = _highlighter.HighlightLine(@"{""a"": {""b"": 1}}", 0);

        Assert.Equal("JSON", _highlighter.LanguageName);
    }

    [Fact]
    public void HighlightLine_EmptyLine_ReturnsEmpty()
    {
        var tokens = _highlighter.HighlightLine("", 0);
        Assert.Empty(tokens);
    }

    [Fact]
    public void SupportedExtensions()
    {
        Assert.Contains(".json", _highlighter.SupportedExtensions);
        Assert.Contains(".jsonc", _highlighter.SupportedExtensions);
    }
}
