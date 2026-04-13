namespace GEditor.Syntax;

/// <summary>
/// 语法高亮 Token — 值对象，表示文本中一个已分类的片段。
/// 仅包含位置信息和语义分类，不包含任何颜色/样式数据。
/// </summary>
public sealed class SyntaxToken : IEquatable<SyntaxToken>
{
    public TokenKind Kind { get; init; }
    public int StartColumn { get; init; }
    public int Length { get; init; }
    public string Text { get; init; } = string.Empty;
    public int LineNumber { get; init; }

    public bool Equals(SyntaxToken? other)
    {
        if (other is null) return false;
        return Kind == other.Kind
            && StartColumn == other.StartColumn
            && Length == other.Length
            && Text == other.Text
            && LineNumber == other.LineNumber;
    }

    public override bool Equals(object? obj) => Equals(obj as SyntaxToken);

    public override int GetHashCode() => HashCode.Combine(Kind, StartColumn, Length, Text, LineNumber);
}
