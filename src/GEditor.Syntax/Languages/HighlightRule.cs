using System.Text.RegularExpressions;

namespace GEditor.Syntax;

/// <summary>
/// 高亮规则 — 一条正则匹配规则，匹配成功产出指定 TokenKind。
/// </summary>
public sealed class HighlightRule
{
    public Regex Pattern { get; init; } = null!;
    public TokenKind Kind { get; init; }
    public int Priority { get; init; }
}
