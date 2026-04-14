using GEditor.Core.Buffer;

namespace GEditor.Core.Selection;

/// <summary>
/// 列编辑操作接口 - 定义列模式下的各种操作
/// </summary>
public interface IColumnEditOperation
{
    /// <summary>
    /// 操作描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 在指定缓冲区上执行列编辑操作
    /// </summary>
    /// <param name="buffer">编辑器缓冲区</param>
    /// <param name="selection">要操作的列选区</param>
    /// <param name="text">要插入或替换的文本（可选）</param>
    void Execute(EditorBuffer buffer, ColumnSelection selection, string? text = null);

    /// <summary>
    /// 撤销操作
    /// </summary>
    /// <param name="buffer">编辑器缓冲区</param>
    /// <param name="selection">原始选区</param>
    /// <param name="originalText">被删除或替换的原始文本</param>
    void Undo(EditorBuffer buffer, ColumnSelection selection, string? originalText = null);
}
