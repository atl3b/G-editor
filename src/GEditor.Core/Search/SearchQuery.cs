namespace GEditor.Core.Search;

/// <summary>搜索条件 — 值对象（Pattern + Options）</summary>
public sealed class SearchQuery
{
    public string Pattern { get; init; } = string.Empty;
    public SearchOptions Options { get; init; } = new();

    public static SearchQuery Create(string pattern, SearchOptions? options = null) => new()
    {
        Pattern = pattern,
        Options = options ?? SearchOptions.Default
    };
}
