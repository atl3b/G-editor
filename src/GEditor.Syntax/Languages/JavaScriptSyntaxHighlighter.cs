namespace GEditor.Syntax;

public sealed class JavaScriptSyntaxHighlighter : RegexBasedHighlighter
{
    public override string LanguageName => "JavaScript";
    public override IReadOnlySet<string> SupportedExtensions => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".js", ".mjs", ".cjs", ".ts", ".tsx", ".jsx" };

    private static readonly IReadOnlyList<HighlightRule> Rules = new HighlightRule[]
    {
        // 注释
        new() { Pattern = Compile(@"//.*$"), Kind = TokenKind.Comment },
        new() { Pattern = Compile(@"/\*[\s\S]*?\*/"), Kind = TokenKind.Comment },
        
        // 字符串
        new() { Pattern = Compile(@"""(?:[^""\\]|\\.)*"""), Kind = TokenKind.String },
        new() { Pattern = Compile(@"'(?:[^'\\]|\\.)*'"), Kind = TokenKind.String },
        new() { Pattern = Compile(@"`(?:[^`\\]|\\.)*`"), Kind = TokenKind.String }, // 模板字符串
        
        // 正则表达式
        new() { Pattern = Compile(@"/(?![*+?])(?:[^\\/]|\\.)+/[gimuy]*"), Kind = TokenKind.String },
        
        // 关键字
        new() { Pattern = Compile(@"\b(?:break|case|catch|continue|debugger|default|delete|do|else|finally|for|function|if|in|instanceof|new|return|switch|this|throw|try|typeof|var|void|while|with|class|extends|export|import|super|yield|const|let|async|await|of|get|set|static|from|as|implements|interface|package|private|protected|public|true|false|null|undefined|NaN|Infinity)\b"),
            Kind = TokenKind.Keyword },
        
        // 内置对象
        new() { Pattern = Compile(@"\b(?:console|window|document|Array|Object|String|Number|Boolean|Function|Symbol|BigInt|Map|Set|WeakMap|WeakSet|Promise|Proxy|Reflect|Date|RegExp|Error|TypeError|RangeError|ReferenceError|SyntaxError|JSON|Math|parseInt|parseFloat|isNaN|isFinite|encodeURI|decodeURI|encodeURIComponent|decodeURIComponent|eval|setTimeout|setInterval|clearTimeout|clearInterval|requestAnimationFrame|require|module|exports|process|Buffer|global|__dirname|__filename|arguments|this|self|globalThis)\b"),
            Kind = TokenKind.Type },
        
        // 数字
        new() { Pattern = Compile(@"\b\d+(?:\.\d+)?(?:[eE][+-]?\d+)?\b"), Kind = TokenKind.Number },
        new() { Pattern = Compile(@"\b0[xX][0-9a-fA-F]+\b"), Kind = TokenKind.Number }, // 十六进制
        new() { Pattern = Compile(@"\b0[bB][01]+\b"), Kind = TokenKind.Number },   // 二进制
        new() { Pattern = Compile(@"\b0[oO][0-7]+\b"), Kind = TokenKind.Number },   // 八进制
        
        // TypeScript 特有
        new() { Pattern = Compile(@"\b(?:type|interface|enum|namespace|module|declare|abstract|readonly|keyof|infer|unique|never|unknown|any|void|satisfies|using|override)\b"),
            Kind = TokenKind.Type },
    };

    protected override IReadOnlyList<HighlightRule> GetRules() => Rules;
}
