using GEditor.Syntax;
using Xunit;

namespace GEditor.Tests.Syntax;

public class XmlHighlighterTests
{
    private readonly XmlSyntaxHighlighter _highlighter = new();

    [Fact]
    public void HighlightLine_OpenTag()
    {
        var tokens = _highlighter.HighlightLine("<root>", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Keyword);
    }

    [Fact]
    public void HighlightLine_ClosingTag()
    {
        var tokens = _highlighter.HighlightLine("</root>", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Keyword);
    }

    [Fact]
    public void HighlightLine_Comment()
    {
        var tokens = _highlighter.HighlightLine("<!-- comment -->", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Comment);
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
        Assert.Contains(".xml", _highlighter.SupportedExtensions);
        Assert.Contains(".xaml", _highlighter.SupportedExtensions);
        Assert.Contains(".html", _highlighter.SupportedExtensions);
    }
}
