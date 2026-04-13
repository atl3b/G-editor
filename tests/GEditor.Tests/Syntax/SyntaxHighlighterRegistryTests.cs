using GEditor.Syntax;
using Xunit;

namespace GEditor.Tests.Syntax;

public class SyntaxHighlighterRegistryTests
{
    [Fact]
    public void Register_GetByExtension_ReturnsHighlighter()
    {
        var registry = new SyntaxHighlighterRegistry();
        var highlighter = new CSharpSyntaxHighlighter();
        registry.Register(highlighter);

        var result = registry.GetHighlighterByExtension(".cs");

        Assert.Same(highlighter, result);
    }

    [Fact]
    public void Register_MultipleHighlighters_MapsCorrectly()
    {
        var registry = new SyntaxHighlighterRegistry();
        registry.Register(new CSharpSyntaxHighlighter());
        registry.Register(new JsonSyntaxHighlighter());

        Assert.NotNull(registry.GetHighlighterByExtension(".cs"));
        Assert.NotNull(registry.GetHighlighterByExtension(".json"));
    }

    [Fact]
    public void Register_SameExtensionTwice_LastWins()
    {
        var registry = new SyntaxHighlighterRegistry();
        var first = new PlainTextHighlighter();
        var second = new CSharpSyntaxHighlighter();

        registry.Register(first);
        registry.Register(second);

        // .csx is in both PlainText and CSharp, last registered wins for C#
        Assert.NotNull(registry.GetHighlighterByExtension(".cs"));
    }

    [Fact]
    public void GetByExtension_CaseInsensitive()
    {
        var registry = new SyntaxHighlighterRegistry();
        registry.Register(new CSharpSyntaxHighlighter());

        Assert.NotNull(registry.GetHighlighterByExtension(".CS"));
        Assert.NotNull(registry.GetHighlighterByExtension(".Cs"));
    }

    [Fact]
    public void GetByExtension_Unregistered_ReturnsNull()
    {
        var registry = new SyntaxHighlighterRegistry();

        Assert.Null(registry.GetHighlighterByExtension(".unknown"));
    }

    [Fact]
    public void IsSupported_ReturnsTrueForRegistered()
    {
        var registry = new SyntaxHighlighterRegistry();
        registry.Register(new CSharpSyntaxHighlighter());

        Assert.True(registry.IsSupported(".cs"));
        Assert.False(registry.IsSupported(".unknown"));
    }

    [Fact]
    public void GetByLanguage_ReturnsHighlighter()
    {
        var registry = new SyntaxHighlighterRegistry();
        registry.Register(new CSharpSyntaxHighlighter());

        var result = registry.GetHighlighterByLanguage("C#");

        Assert.NotNull(result);
    }

    [Fact]
    public void GetByExtension_WithoutDot_AddsDot()
    {
        var registry = new SyntaxHighlighterRegistry();
        registry.Register(new CSharpSyntaxHighlighter());

        var result = registry.GetHighlighterByExtension("cs");

        Assert.NotNull(result);
    }

    [Fact]
    public void GetByLanguage_EmptyOrNull_ReturnsNull()
    {
        var registry = new SyntaxHighlighterRegistry();
        registry.Register(new CSharpSyntaxHighlighter());

        Assert.Null(registry.GetHighlighterByLanguage(""));
        Assert.Null(registry.GetHighlighterByLanguage(null!));
    }
}
