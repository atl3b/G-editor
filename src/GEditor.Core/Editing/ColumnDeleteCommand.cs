using GEditor.Core.Buffer;
using GEditor.Core.Selection;

namespace GEditor.Core.Editing;

/// <summary>
/// 列模式删除命令 - 从多行指定范围删除文本
/// </summary>
public sealed class ColumnDeleteCommand : IEditCommand
{
    private readonly ColumnSelection _selection;
    private string[]? _deletedTexts;

    public string Description => "列模式删除";

    public ColumnDeleteCommand(ColumnSelection selection)
    {
        _selection = selection;
    }

    public void Execute(EditorBuffer buffer)
    {
        var lineRanges = _selection.GetLineRanges(buffer.Lines);
        var ranges = new List<(int line, int column, int length)>();

        foreach (var range in lineRanges)
        {
            if (!range.IsEmpty)
            {
                ranges.Add((range.Line, range.StartColumn, range.Length));
            }
        }

        _deletedTexts = buffer.DeleteAtColumns(ranges);
    }

    public void Undo(EditorBuffer buffer)
    {
        if (_deletedTexts == null || _deletedTexts.Length == 0)
            return;

        var lineRanges = _selection.GetLineRanges(buffer.Lines);
        var textIndex = 0;

        // 从后往前恢复文本
        for (int i = lineRanges.Count - 1; i >= 0; i--)
        {
            var range = lineRanges[i];
            if (!range.IsEmpty && textIndex < _deletedTexts.Length)
            {
                buffer.InsertAtColumns(new[] { (range.Line, range.StartColumn) }, _deletedTexts[textIndex]);
                textIndex++;
            }
        }
    }
}
