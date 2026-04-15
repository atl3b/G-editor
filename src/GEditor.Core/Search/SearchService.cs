using System.Text.RegularExpressions;
using GEditor.Core.Buffer;
using GEditor.Core.Editing;

namespace GEditor.Core.Search;

/// <summary>搜索服务 — 纯文本搜索 + Replace All 命令构建</summary>
public sealed class SearchService : ISearchService
{
    public IReadOnlyList<SearchMatch> FindAll(EditorBuffer buffer, SearchQuery query)
    {
        if (buffer is null || string.IsNullOrEmpty(query.Pattern))
            return Array.Empty<SearchMatch>();

        var regex = BuildRegex(query);
        var matches = new List<SearchMatch>();

        for (int i = 0; i < buffer.LineCount; i++)
        {
            var lineText = buffer[i];
            foreach (Match m in regex.Matches(lineText))
            {
                matches.Add(new SearchMatch
                {
                    Line = i,
                    Column = m.Index,
                    Length = m.Length,
                    MatchedText = m.Value,
                    LineText = lineText
                });
            }
        }

        return matches;
    }

    public SearchMatch? FindNext(EditorBuffer buffer, SearchQuery query, int fromLine, int fromColumn)
    {
        if (buffer is null || string.IsNullOrEmpty(query.Pattern))
            return null;

        var regex = BuildRegex(query);
        int totalLines = buffer.LineCount;

        // Phase 1: 从当前位置搜索到文档末尾
        var result = SearchFromPosition(buffer, regex, fromLine, fromColumn, totalLines - 1);
        if (result != null)
            return result;

        // Phase 2: wrap-around — 从文档开头搜到起始位置之前
        if (fromLine > 0)
            return SearchFromPosition(buffer, regex, 0, 0, fromLine - 1);

        return null;
    }

    /// <summary>
    /// 在指定行范围内搜索下一个匹配项
    /// </summary>
    private static SearchMatch? SearchFromPosition(EditorBuffer buffer, Regex regex, int startLine, int startCol, int endLine)
    {
        for (int line = startLine; line <= endLine; line++)
        {
            int col = (line == startLine) ? startCol : 0;
            var lineText = buffer[line];

            foreach (Match m in regex.Matches(lineText))
            {
                if (m.Index >= col)
                {
                    return new SearchMatch
                    {
                        Line = line,
                        Column = m.Index,
                        Length = m.Length,
                        MatchedText = m.Value,
                        LineText = lineText
                    };
                }
            }
        }
        return null;
    }

    public SearchMatch? FindPrevious(EditorBuffer buffer, SearchQuery query, int fromLine, int fromColumn)
    {
        if (buffer is null || string.IsNullOrEmpty(query.Pattern))
            return null;

        var regex = BuildRegex(query);
        var allMatches = FindAll(buffer, query);

        if (allMatches.Count == 0)
            return null;

        // Find the last match that is before (fromLine, fromColumn)
        SearchMatch? best = null;
        foreach (var m in allMatches)
        {
            bool isBefore = m.Line < fromLine || (m.Line == fromLine && m.Column < fromColumn);
            if (isBefore)
                best = m;
        }

        // Wrap around: if nothing found before, return the last match
        return best ?? allMatches[^1];
    }

    public int CountMatches(EditorBuffer buffer, SearchQuery query)
    {
        if (buffer is null || string.IsNullOrEmpty(query.Pattern))
            return 0;

        var regex = BuildRegex(query);
        int count = 0;

        for (int i = 0; i < buffer.LineCount; i++)
            count += regex.Matches(buffer[i]).Count;

        return count;
    }

    public IEditCommand CreateReplaceAllCommand(EditorBuffer buffer, SearchQuery query, string replacement)
    {
        var matches = FindAll(buffer, query);
        if (matches.Count == 0)
        {
            return new CompositeEditCommand("No matches to replace", Array.Empty<IEditCommand>());
        }

        // Build commands from back to front to avoid position shifts
        var commands = new List<IEditCommand>();
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var m = matches[i];
            commands.Add(new ReplaceTextCommand(m.Line, m.Column, m.Length, m.MatchedText, replacement));
        }

        return new CompositeEditCommand(
            $"Replace All: '{query.Pattern}' → '{replacement}' ({matches.Count} occurrences)",
            commands);
    }

    private Regex BuildRegex(SearchQuery query)
    {
        string pattern = query.Pattern;

        if (!query.Options.UseRegex)
            pattern = Regex.Escape(pattern);

        if (query.Options.WholeWord)
            pattern = $@"\b{pattern}\b";

        var options = query.Options.MatchCase
            ? RegexOptions.None
            : RegexOptions.IgnoreCase;

        return new Regex(pattern, options);
    }
}
