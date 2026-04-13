namespace GEditor.Core.Search;

/// <summary>搜索匹配结果 — 值对象（不可变）</summary>
public sealed class SearchMatch : IEquatable<SearchMatch>
{
    public int Line { get; init; }
    public int Column { get; init; }
    public int Length { get; init; }
    public string MatchedText { get; init; } = string.Empty;
    public string LineText { get; init; } = string.Empty;

    public bool Equals(SearchMatch? other)
    {
        if (other is null) return false;
        return Line == other.Line
            && Column == other.Column
            && Length == other.Length
            && MatchedText == other.MatchedText;
    }

    public override bool Equals(object? obj) => Equals(obj as SearchMatch);

    public override int GetHashCode() => HashCode.Combine(Line, Column, Length, MatchedText);
}
