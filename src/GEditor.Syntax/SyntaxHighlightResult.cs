namespace GEditor.Syntax;

/// <summary>
/// 语法高亮结果 — 一次高亮操作的完整输出。
/// 包含逐行 Token 列表和元信息，供 UI 层消费。
/// </summary>
public sealed class SyntaxHighlightResult
{
    public string LanguageName { get; init; } = string.Empty;

    /// <summary>
    /// 逐行 Token 列表。外层索引 = 行号（0-based），内层 = 该行 Token 序列。
    /// </summary>
    public IReadOnlyList<IReadOnlyList<SyntaxToken>> LineTokens { get; init; }
        = Array.Empty<IReadOnlyList<SyntaxToken>>();

    public int LineCount => LineTokens.Count;

    public IReadOnlyList<SyntaxToken> GetTokens(int lineNumber)
        => lineNumber >= 0 && lineNumber < LineTokens.Count
            ? LineTokens[lineNumber]
            : Array.Empty<SyntaxToken>();
}
