namespace GEditor.Syntax;

public sealed class MarkdownSyntaxHighlighter : RegexBasedHighlighter
{
    public override string LanguageName => "Markdown";
    public override IReadOnlySet<string> SupportedExtensions => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".md", ".markdown", ".mdown", ".mkd" };

    private static readonly IReadOnlyList<HighlightRule> Rules = new HighlightRule[]
    {
        // 标题（# ## ### 等）
        new() { Pattern = Compile(@"^#{1,6}\s.+"), Kind = TokenKind.Keyword },
        
        // 粗体和斜体
        new() { Pattern = Compile(@"\*{3}.+?\*{3}"), Kind = TokenKind.String },  // ***bold italic***
        new() { Pattern = Compile(@"_{3}.+?_{3}"), Kind = TokenKind.String },    // ___bold italic___
        new() { Pattern = Compile(@"\*{2}.+?\*{2}"), Kind = TokenKind.String },  // **bold**
        new() { Pattern = Compile(@"_.+?_"), Kind = TokenKind.String },           // _italic_
        new() { Pattern = Compile(@"\*.+?\*"), Kind = TokenKind.String },         // *italic*
        
        // 删除线
        new() { Pattern = Compile(@"~~.+?~~"), Kind = TokenKind.Comment },
        
        // 代码块（围栏）
        new() { Pattern = Compile(@"```[\s\S]*?```"), Kind = TokenKind.Preprocessor },
        
        // 行内代码
        new() { Pattern = Compile(@"`[^`]+`"), Kind = TokenKind.Number },
        
        // 链接
        new() { Pattern = Compile(@"\[([^\]]+)\]\(([^)]+)\)"), Kind = TokenKind.Attribute },
        
        // 图片
        new() { Pattern = Compile(@"!\[([^\]]*)\]\(([^)]+\))"), Kind = TokenKind.Attribute },
        
        // 自动链接
        new() { Pattern = Compile(@"<(?:https?|ftp|mailto):[^>]+>"), Kind = TokenKind.Attribute },
        
        // 引用
        new() { Pattern = Compile(@"^\s*>+.+"), Kind = TokenKind.Comment },
        
        // 无序列表
        new() { Pattern = Compile(@"^\s*[-*+]\s+.+"), Kind = TokenKind.Operator },
        
        // 有序列表
        new() { Pattern = Compile(@"^\s*\d+\.\s+.+"), Kind = TokenKind.Operator },
        
        // 水平线
        new() { Pattern = Compile(@"^(?:-{3,}|\*{3,}|_{3,})$"), Kind = TokenKind.Keyword },
        
        // HTML 标签（在 Markdown 中）
        new() { Pattern = Compile(@"</?[a-zA-Z][a-zA-Z0-9]*[^>]*>"), Kind = TokenKind.Type },
        
        // 转义字符
        new() { Pattern = Compile(@"\\[\\`*_{}[\]()#+\-.!|~]"), Kind = TokenKind.Operator },
    };

    protected override IReadOnlyList<HighlightRule> GetRules() => Rules;
}
