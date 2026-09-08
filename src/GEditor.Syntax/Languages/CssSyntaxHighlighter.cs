namespace GEditor.Syntax;

public sealed class CssSyntaxHighlighter : RegexBasedHighlighter
{
    public override string LanguageName => "CSS";
    public override IReadOnlySet<string> SupportedExtensions => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".css", ".scss", ".sass", ".less" };

    private static readonly IReadOnlyList<HighlightRule> Rules = new HighlightRule[]
    {
        // 注释
        new() { Pattern = Compile(@"/\*[\s\S]*?\*/"), Kind = TokenKind.Comment },
        
        // 字符串
        new() { Pattern = Compile(@"""(?:[^""\\]|\\.)*"""), Kind = TokenKind.String },
        new() { Pattern = Compile(@"'(?:[^'\\]|\\.)*'"), Kind = TokenKind.String },
        
        // 选择器和属性（CSS 关键字）
        new() { Pattern = Compile(@"[.#]?[a-zA-Z_-][a-zA-Z0-9_-]*(?=\s*\{)"), Kind = TokenKind.Keyword },
        
        // 属性名
        new() { Pattern = Compile(@"[a-zA-Z-]+(?=\s*:)"), Kind = TokenKind.Attribute },
        
        // 值关键字
        new() { Pattern = Compile(@"\b(?:auto|inherit|initial|unset|none|block|inline|inline-block|flex|grid|table|inline-flex|inline-grid|absolute|relative|fixed|sticky|static|visible|hidden|scroll|auto|transparent|currentColor|revert|all)\b"),
            Kind = TokenKind.Keyword },
        
        // 颜色值
        new() { Pattern = Compile(@"#[0-9a-fA-F]{3,8}\b"), Kind = TokenKind.Number }, // 十六进制颜色
        new() { Pattern = Compile(@"\b(?:rgb|rgba|hsl|hsla|hwb|lab|lch|color)\s*\("), Kind = TokenKind.Type }, // 颜色函数
        
        // 单位和数字
        new() { Pattern = Compile(@"\d+(?:\.\d+)?(?:px|em|rem|%|vh|vw|vmin vmax|ch|ex|cm|mm|in|pt|pc|deg|rad|grad|turn|s|ms|fr)?\b"), Kind = TokenKind.Number },
        
        // 重要标记
        new() { Pattern = Compile(@"!important"), Kind = TokenKind.Preprocessor },
        
        // @ 规则
        new() { Pattern = Compile(@"@[a-zA-Z-]+\b"), Kind = TokenKind.Preprocessor },
        
        // 伪类和伪元素
        new() { Pattern = Compile(@"::?[a-zA-Z-]+\b(?=\s*\{)"), Kind = TokenKind.Type },
        
        // 函数
        new() { Pattern = Compile(@"\b(?:calc|clamp|min|max|fit-content|repeat|var|attr|url|counter|counters|env|linear-gradient|radial-gradient|conic-gradient|translate|rotate|scale|skew|matrix|perspective|filter|blur|brightness|contrast|drop-shadow|grayscale|hue-rotate|invert|opacity|saturate|sepia)\s*\("),
            Kind = TokenKind.Type },
    };

    protected override IReadOnlyList<HighlightRule> GetRules() => Rules;
}
