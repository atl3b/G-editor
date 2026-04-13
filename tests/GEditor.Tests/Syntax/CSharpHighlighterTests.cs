using GEditor.Syntax;
using Xunit;

namespace GEditor.Tests.Syntax;

public class CSharpHighlighterTests
{
    private readonly CSharpSyntaxHighlighter _highlighter = new();

    [Fact]
    public void HighlightLine_Keywords()
    {
        var tokens = _highlighter.HighlightLine("public class Foo", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Keyword && t.Text == "public");
        Assert.Contains(tokens, t => t.Kind == TokenKind.Keyword && t.Text == "class");
    }

    [Fact]
    public void HighlightLine_StringLiteral()
    {
        var tokens = _highlighter.HighlightLine(@"string s = ""hello"";", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.String && t.Text == @"""hello""");
    }

    [Fact]
    public void HighlightLine_Comment()
    {
        var tokens = _highlighter.HighlightLine("// this is comment", 0);

        Assert.Single(tokens);
        Assert.Equal(TokenKind.Comment, tokens[0].Kind);
    }

    [Fact]
    public void HighlightLine_Number()
    {
        var tokens = _highlighter.HighlightLine("int x = 42;", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Number && t.Text == "42");
    }

    [Fact]
    public void HighlightLine_Attribute()
    {
        var tokens = _highlighter.HighlightLine("[Serializable]", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Attribute);
    }

    [Fact]
    public void HighlightLine_TypeName()
    {
        var tokens = _highlighter.HighlightLine("List<int> list = new();", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Type && t.Text == "List");
    }

    [Fact]
    public void HighlightLine_EmptyLine_ReturnsEmpty()
    {
        var tokens = _highlighter.HighlightLine("", 0);

        Assert.Empty(tokens);
    }

    [Fact]
    public void HighlightLine_PlainText_IsNone()
    {
        var tokens = _highlighter.HighlightLine("abc xyz", 0);

        Assert.All(tokens, t => Assert.Equal(TokenKind.None, t.Kind));
    }

    [Fact]
    public void HighlightDocument_MultipleLines()
    {
        var lines = new[] { "public class Foo", "{", "    public void Bar() { }", "}" };
        var result = _highlighter.HighlightDocument(lines);

        Assert.Equal(4, result.LineCount);
        Assert.Equal("C#", result.LanguageName);
    }

    [Fact]
    public void SupportedExtensions()
    {
        Assert.Contains(".cs", _highlighter.SupportedExtensions);
        Assert.Contains(".csx", _highlighter.SupportedExtensions);
    }
}
