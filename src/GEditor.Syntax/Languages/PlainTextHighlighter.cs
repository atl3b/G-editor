namespace GEditor.Syntax;

/// <summary>纯文本高亮器 — 不做任何分类，所有 Token 为 PlainText</summary>
public sealed class PlainTextHighlighter : ISyntaxHighlighter
{
    public string LanguageName => "Plain Text";
    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".log", ".md", ".csv", ".cfg", ".ini", ".env"
    };

    public IReadOnlyList<SyntaxToken> HighlightLine(string lineText, int lineNumber)
    {
        if (string.IsNullOrEmpty(lineText))
            return Array.Empty<SyntaxToken>();

        return new[]
        {
            new SyntaxToken
            {
                Kind = TokenKind.PlainText,
                StartColumn = 0,
                Length = lineText.Length,
                Text = lineText,
                LineNumber = lineNumber
            }
        };
    }

    public SyntaxHighlightResult HighlightDocument(IReadOnlyList<string> lines)
    {
        var lineTokens = new IReadOnlyList<SyntaxToken>[lines.Count];
        for (int i = 0; i < lines.Count; i++)
            lineTokens[i] = HighlightLine(lines[i], i);

        return new SyntaxHighlightResult
        {
            LanguageName = LanguageName,
            LineTokens = lineTokens
        };
    }
}
