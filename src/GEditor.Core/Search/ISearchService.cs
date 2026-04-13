using GEditor.Core.Buffer;
using GEditor.Core.Editing;

namespace GEditor.Core.Search;

/// <summary>
/// 搜索服务接口 — 纯只读，不直接修改 Buffer。
/// 替换操作通过返回 IEditCommand 交给 UndoRedoManager 执行。
/// </summary>
public interface ISearchService
{
    /// <summary>在缓冲区中查找所有匹配项（无副作用）</summary>
    IReadOnlyList<SearchMatch> FindAll(EditorBuffer buffer, SearchQuery query);

    /// <summary>从指定位置向后查找下一个匹配项（支持循环）</summary>
    SearchMatch? FindNext(EditorBuffer buffer, SearchQuery query, int fromLine, int fromColumn);

    /// <summary>从指定位置向前查找上一个匹配项（支持循环）</summary>
    SearchMatch? FindPrevious(EditorBuffer buffer, SearchQuery query, int fromLine, int fromColumn);

    /// <summary>统计匹配总数</summary>
    int CountMatches(EditorBuffer buffer, SearchQuery query);

    /// <summary>
    /// 构建 "Replace All" 复合编辑命令（不直接修改 buffer）。
    /// 返回的 IEditCommand 可通过 Document.ExecuteCommand() 提交到 UndoRedoManager。
    /// </summary>
    IEditCommand CreateReplaceAllCommand(EditorBuffer buffer, SearchQuery query, string replacement);
}
