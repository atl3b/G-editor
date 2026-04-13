namespace GEditor.Syntax;

public sealed class JsonSyntaxHighlighter : RegexBasedHighlighter
{
    public override string LanguageName => "JSON";
    public override IReadOnlySet<string> SupportedExtensions => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".json", ".jsonc" };

    private static readonly IReadOnlyList<HighlightRule> Rules = new HighlightRule[]
    {
        new() { Pattern = Compile(@"//.*$"), Kind = TokenKind.Comment },
        new() { Pattern = Compile(@"""(?:[^""\\]|\\.)*"""), Kind = TokenKind.String },
        new() { Pattern = Compile(@"\b(?:true|false|null)\b"), Kind = TokenKind.Keyword },
        new() { Pattern = Compile(@"-?\b\d+(?:\.\d+)?(?:[eE][+-]?\d+)?\b"), Kind = TokenKind.Number },
    };

    protected override IReadOnlyList<HighlightRule> GetRules() => Rules;
}
