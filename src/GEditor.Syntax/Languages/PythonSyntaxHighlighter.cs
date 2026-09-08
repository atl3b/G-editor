namespace GEditor.Syntax;

public sealed class PythonSyntaxHighlighter : RegexBasedHighlighter
{
    public override string LanguageName => "Python";
    public override IReadOnlySet<string> SupportedExtensions => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".py", ".pyw", ".pyi" };

    private static readonly IReadOnlyList<HighlightRule> Rules = new HighlightRule[]
    {
        // 注释
        new() { Pattern = Compile(@"#.*$"), Kind = TokenKind.Comment },
        
        // 多行字符串（文档字符串）
        new() { Pattern = Compile(@"""(?:[^""\\]|\\.)*"""), Kind = TokenKind.String },
        new() { Pattern = Compile(@"'(?:[^'\\]|\\.)*'"), Kind = TokenKind.String },
        new() { Pattern = Compile(@"""""[\s\S]*?"""""), Kind = TokenKind.String },
        new() { Pattern = Compile(@"'''[\s\S]*?'''"), Kind = TokenKind.String },
        
        // 装饰器
        new() { Pattern = Compile(@"@\w+"), Kind = TokenKind.Attribute },
        
        // 关键字
        new() { Pattern = Compile(@"\b(?:False|None|True|and|as|assert|async|await|break|class|continue|def|del|elif|else|except|finally|for|from|global|if|import|in|is|lambda|nonlocal|not|or|pass|raise|return|try|while|with|yield|__name__|__main__|self|cls)\b"),
            Kind = TokenKind.Keyword },
        
        // 内置函数和类型
        new() { Pattern = Compile(@"\b(?:abs|aiter|all|any|anext|ascii|bin|bool|breakpoint|bytearray|bytes|callable|chr|classmethod|compile|complex|copyright|credits|delattr|dict|dir|divmod|enumerate|eval|exec|exit|filter|float|format|frozenset|getattr|globals|hasattr|hash|help|hex|id|input|int|isinstance|issubclass|iter|len|license|list|locals|map|max|memoryview|min|next|object|oct|open|ord|pow|print|property|quit|range|repr|reversed|round|set|setattr|slice|sorted|staticmethod|str|sum|super|tuple|type|vars|zip|__import__|Exception|ValueError|TypeError|KeyError|IndexError|IOError|OSError|RuntimeError|AttributeError|ImportError|NotImplementedError|StopIteration|GeneratorExit|print|len|range|str|int|list|dict|tuple|set|frozenset|bytes|bytearray|type|isinstance|issubclass|hasattr|getattr|setattr|delattr|property|classmethod|staticmethod|super|object|enumerate|zip|map|filter|sorted|reversed|all|any|abs|min|max|sum|round|pow|divmod|hex|oct|bin|chr|ord|ascii|repr|format|hash|id|callable|iter|next|slice|memoryview|complex|bool|frozenset|bytes|bytearray|exec|eval|compile|breakpoint|open|input|exit|quit|credits|license|copyright|help|globals|locals|vars)\b"),
            Kind = TokenKind.Type },
        
        // 数字
        new() { Pattern = Compile(@"\b\d+(?:\.\d+)?(?:[eE][+-]?\d+)?(?:j|J)?\b"), Kind = TokenKind.Number },
        new() { Pattern = Compile(@"\b0[bB][01]+\b"), Kind = TokenKind.Number },  // 二进制
        new() { Pattern = Compile(@"\b0[oO][0-7]+\b"), Kind = TokenKind.Number },   // 八进制
        new() { Pattern = Compile(@"\b0[xX][0-9a-fA-F]+\b"), Kind = TokenKind.Number }, // 十六进制
    };

    protected override IReadOnlyList<HighlightRule> GetRules() => Rules;
}
