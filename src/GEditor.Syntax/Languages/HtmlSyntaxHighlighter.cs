namespace GEditor.Syntax;

public sealed class HtmlSyntaxHighlighter : RegexBasedHighlighter
{
    public override string LanguageName => "HTML";
    public override IReadOnlySet<string> SupportedExtensions => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".html", ".htm", ".xhtml", ".vue", ".svelte" };

    private static readonly IReadOnlyList<HighlightRule> Rules = new HighlightRule[]
    {
        // 注释
        new() { Pattern = Compile(@"<!--[\s\S]*?-->"), Kind = TokenKind.Comment },
        
        // DOCTYPE
        new() { Pattern = Compile(@"<!DOCTYPE[^>]*>"), Kind = TokenKind.Preprocessor },
        
        // CDATA
        new() { Pattern = Compile(@"<!\[CDATA\[[\s\S]*?\]\]>"), Kind = TokenKind.String },
        
        // 标签
        new() { Pattern = Compile(@"</?[a-zA-Z][a-zA-Z0-9-]*"), Kind = TokenKind.Keyword },
        
        // 属性名
        new() { Pattern = Compile(@"\s+[a-zA-Z_:][-a-zA-Z0-9_:.]*(?=\s*=)"), Kind = TokenKind.Attribute },
        
        // 属性值（双引号）
        new() { Pattern = Compile(@"""(?:[^""\\]|\\.)*"""), Kind = TokenKind.String },
        
        // 属性值（单引号）
        new() { Pattern = Compile(@"'(?:[^'\\]|\\.)*'"), Kind = TokenKind.String },
        
        // 特殊字符实体
        new() { Pattern = Compile(@"&[a-zA-Z]+;|&#\d+;|&#x[0-9a-fA-F]+;"), Kind = TokenKind.Number },
        
        // 内联样式和脚本标记
        new() { Pattern = Compile(@"\b(?:href|src|alt|title|id|class|style|name|value|type|placeholder|data-\w+|rel|target|onclick|onload|onerror)\b"),
            Kind = TokenKind.Attribute },
    };

    protected override IReadOnlyList<HighlightRule> GetRules() => Rules;
}
