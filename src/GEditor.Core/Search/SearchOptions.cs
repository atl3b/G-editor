namespace GEditor.Core.Search;

/// <summary>搜索选项 — 值对象（不可变标志包）</summary>
public sealed class SearchOptions
{
    public bool MatchCase { get; init; }
    public bool WholeWord { get; init; }
    public bool UseRegex { get; init; }

    public static SearchOptions Default => new();
    public static SearchOptions CaseSensitive => new() { MatchCase = true };
    public static SearchOptions WholeWordOnly => new() { WholeWord = true };
}
