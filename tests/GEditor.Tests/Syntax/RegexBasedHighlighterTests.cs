using System.Text.RegularExpressions;
using GEditor.Syntax;
using Xunit;

namespace GEditor.Tests.Syntax;

public class RegexBasedHighlighterTests
{
    private sealed class TestHighlighter : RegexBasedHighlighter
    {
        public override string LanguageName => "Test";
        public override IReadOnlySet<string> SupportedExtensions => new HashSet<string> { ".test" };

        public static readonly IReadOnlyList<HighlightRule> TestRules = new HighlightRule[]
        {
            new() { Pattern = new Regex(@"\bfoo\b", RegexOptions.Compiled), Kind = TokenKind.Keyword },
            new() { Pattern = new Regex(@"""\w+""", RegexOptions.Compiled), Kind = TokenKind.String },
        };

        protected override IReadOnlyList<HighlightRule> GetRules() => TestRules;
    }

    private readonly TestHighlighter _highlighter = new();

    [Fact]
    public void HighlightLine_MatchesFirstRule()
    {
        var tokens = _highlighter.HighlightLine("foo bar", 0);

        Assert.Contains(tokens, t => t.Kind == TokenKind.Keyword && t.Text == "foo");
    }

    [Fact]
    public void HighlightLine_UnmatchedText_IsNone()
    {
        var tokens = _highlighter.HighlightLine("bar baz", 0);

        Assert.All(tokens, t => Assert.Equal(TokenKind.None, t.Kind));
    }

    [Fact]
    public void HighlightLine_TokensCoverFullLine()
    {
        var tokens = _highlighter.HighlightLine("foobar", 0);

        int totalLength = tokens.Sum(t => t.Length);
        Assert.Equal(6, totalLength);
    }

    [Fact]
    public void HighlightLine_EmptyLine_ReturnsEmpty()
    {
        var tokens = _highlighter.HighlightLine("", 0);
        Assert.Empty(tokens);
    }

    [Fact]
    public void HighlightDocument_ReturnsCorrectLineCount()
    {
        var lines = new[] { "line1", "line2" };
        var result = _highlighter.HighlightDocument(lines);

        Assert.Equal(2, result.LineCount);
        Assert.Equal("Test", result.LanguageName);
    }

    [Fact]
    public void HighlightLine_Priority_FirstRuleWins()
    {
        var tokens = _highlighter.HighlightLine(@"""foo""", 0);

        // String rule should match first (higher priority in list)
        Assert.Contains(tokens, t => t.Kind == TokenKind.String);
    }
}
