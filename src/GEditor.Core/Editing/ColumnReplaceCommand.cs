using GEditor.Core.Buffer;
using GEditor.Core.Selection;

namespace GEditor.Core.Editing;

/// <summary>
/// 列模式替换命令 - 删除并插入新文本（复合命令）
/// </summary>
public sealed class ColumnReplaceCommand : IEditCommand
{
    private readonly ColumnSelection _selection;
    private readonly string _newText;
    private string[]? _deletedTexts;

    public string Description => $"列模式替换: \"{_newText}\"";

    public ColumnReplaceCommand(ColumnSelection selection, string newText)
    {
        _selection = selection;
        _newText = newText ?? string.Empty;
    }

    public void Execute(EditorBuffer buffer)
    {
        // 先删除选区内容
        var lineRanges = _selection.GetLineRanges(buffer.Lines);
        var deleteRanges = new List<(int line, int column, int length)>();

        foreach (var range in lineRanges)
        {
            if (!range.IsEmpty)
            {
                deleteRanges.Add((range.Line, range.StartColumn, range.Length));
            }
        }

        _deletedTexts = buffer.DeleteAtColumns(deleteRanges);

        // 计算插入位置（删除后，每行都在原选区起始列位置插入）
        var normalized = _selection.Normalized();
        var positions = new List<(int line, int column)>();

        for (int line = normalized.StartLine; line <= normalized.EndLine; line++)
        {
            if (line >= 0 && line < buffer.LineCount)
            {
                positions.Add((line, normalized.StartColumn));
            }
        }

        // 插入新文本
        buffer.InsertAtColumns(positions, _newText);
    }

    public void Undo(EditorBuffer buffer)
    {
        if (_deletedTexts == null)
            return;

        // 删除刚插入的文本
        var normalized = _selection.Normalized();
        var lineRanges = normalized.GetLineRanges(buffer.Lines);
        var deleteRanges = new List<(int line, int column, int length)>();

        int textIndex = 0;
        foreach (var range in lineRanges)
        {
            if (!range.IsEmpty)
            {
                deleteRanges.Add((range.Line, range.StartColumn, _newText.Length));
                textIndex++;
            }
        }

        buffer.DeleteAtColumns(deleteRanges);

        // 恢复原始文本
        textIndex = 0;
        foreach (var range in lineRanges)
        {
            if (!range.IsEmpty && textIndex < _deletedTexts.Length)
            {
                buffer.InsertAtColumns(new[] { (range.Line, range.StartColumn) }, _deletedTexts[textIndex]);
                textIndex++;
            }
        }
    }
}
