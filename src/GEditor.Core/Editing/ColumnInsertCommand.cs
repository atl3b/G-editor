using GEditor.Core.Buffer;
using GEditor.Core.Selection;

namespace GEditor.Core.Editing;

/// <summary>
/// 列模式插入命令 - 在多行指定列位置同时插入文本
/// </summary>
public sealed class ColumnInsertCommand : IEditCommand
{
    private readonly IReadOnlyList<(int line, int column)> _positions;
    private readonly string _text;

    public string Description => $"列模式插入: \"{_text}\"";

    public ColumnInsertCommand(IReadOnlyList<(int line, int column)> positions, string text)
    {
        _positions = positions ?? throw new ArgumentNullException(nameof(positions));
        _text = text ?? string.Empty;
    }

    public void Execute(EditorBuffer buffer)
    {
        buffer.InsertAtColumns(_positions, _text);
    }

    public void Undo(EditorBuffer buffer)
    {
        // 删除刚才插入的文本
        var deleteRanges = new List<(int line, int column, int length)>();

        foreach (var (line, column) in _positions)
        {
            if (line >= 0 && line < buffer.LineCount)
            {
                int col = Math.Max(0, Math.Min(column, buffer.GetLineLength(line)));
                deleteRanges.Add((line, col, _text.Length));
            }
        }

        buffer.DeleteAtColumns(deleteRanges);
    }
}
