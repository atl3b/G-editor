namespace GEditor.Syntax;

public sealed class CSharpSyntaxHighlighter : RegexBasedHighlighter
{
    public override string LanguageName => "C#";
    public override IReadOnlySet<string> SupportedExtensions => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cs", ".csx" };

    private static readonly IReadOnlyList<HighlightRule> Rules = new HighlightRule[]
    {
        new() { Pattern = Compile(@"//.*$"), Kind = TokenKind.Comment },
        new() { Pattern = Compile(@"""(?:[^""\\]|\\.)*"""), Kind = TokenKind.String },
        new() { Pattern = Compile(@"'(?:[^'\\]|\\.)*'"), Kind = TokenKind.String },
        new() { Pattern = Compile(@"\$""(?:[^""\\]|\\.)*"""), Kind = TokenKind.String },
        new() { Pattern = Compile(@"\b(?:using|namespace|class|struct|interface|enum|public|private|protected|internal|static|void|int|string|bool|double|float|long|var|new|return|if|else|for|foreach|while|do|switch|case|break|continue|try|catch|finally|throw|in|is|as|out|ref|readonly|abstract|virtual|override|sealed|async|await|true|false|null|this|base|const|volatile|unsafe|fixed|sizeof|typeof|where|get|set|partial|global|record|init|with|not|and|or|nint|nuint)\b"),
            Kind = TokenKind.Keyword },
        new() { Pattern = Compile(@"\b\d+(?:\.\d+)?(?:[fFdDmM])?\b"), Kind = TokenKind.Number },
        new() { Pattern = Compile(@"\b0x[0-9a-fA-F]+\b"), Kind = TokenKind.Number },
        new() { Pattern = Compile(@"\[[\w()]+\]"), Kind = TokenKind.Attribute },
        new() { Pattern = Compile(@"\b[A-Z]\w*\b"), Kind = TokenKind.Type },
    };

    protected override IReadOnlyList<HighlightRule> GetRules() => Rules;
}
