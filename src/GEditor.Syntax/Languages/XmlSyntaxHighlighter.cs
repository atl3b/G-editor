namespace GEditor.Syntax;

public sealed class XmlSyntaxHighlighter : RegexBasedHighlighter
{
    public override string LanguageName => "XML";
    public override IReadOnlySet<string> SupportedExtensions => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".xml", ".xaml", ".html", ".htm", ".svg" };

    private static readonly IReadOnlyList<HighlightRule> Rules = new HighlightRule[]
    {
        new() { Pattern = Compile(@"<!--.*?-->"), Kind = TokenKind.Comment },
        new() { Pattern = Compile(@"""(?:[^""]|&quot;)*"""), Kind = TokenKind.String },
        new() { Pattern = Compile(@"'(?:[^']|&apos;)*'"), Kind = TokenKind.String },
        new() { Pattern = Compile(@"</?[a-zA-Z_][\w:.-]*"), Kind = TokenKind.Keyword },
        new() { Pattern = Compile(@"/?>"), Kind = TokenKind.Delimiter },
        new() { Pattern = Compile(@"\b[a-zA-Z_][\w:.-]*(?=\s*=)"), Kind = TokenKind.Attribute },
    };

    protected override IReadOnlyList<HighlightRule> GetRules() => Rules;
}
