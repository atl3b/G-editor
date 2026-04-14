namespace GEditor.Core.Selection;

/// <summary>
/// 矩形选区值对象，表示列模式下的选区范围。
/// 使用 0-based 坐标系统（与 EditorBuffer 保持一致）。
/// </summary>
public readonly record struct ColumnSelection(int StartLine, int StartColumn, int EndLine, int EndColumn)
{
    /// <summary>
    /// 创建一个空的列选区
    /// </summary>
    public static ColumnSelection Empty => new(0, 0, 0, 0);

    /// <summary>
    /// 选区是否为空（起点和终点相同）
    /// </summary>
    public bool IsEmpty => StartLine == EndLine && StartColumn == EndColumn;

    /// <summary>
    /// 起始行是否小于等于结束行
    /// </summary>
    public bool IsNormalized => StartLine < EndLine || (StartLine == EndLine && StartColumn <= EndColumn);

    /// <summary>
    /// 包含的行数（闭区间）
    /// </summary>
    public int LineCount => EndLine - StartLine + 1;

    /// <summary>
    /// 确保选区标准化（起始点 <= 结束点）
    /// </summary>
    public ColumnSelection Normalized()
    {
        if (IsNormalized)
            return this;

        int normStartLine = Math.Min(StartLine, EndLine);
        int normEndLine = Math.Max(StartLine, EndLine);
        int normStartCol = Math.Min(StartColumn, EndColumn);
        int normEndCol = Math.Max(StartColumn, EndColumn);

        return new ColumnSelection(normStartLine, normStartCol, normEndLine, normEndCol);
    }

    /// <summary>
    /// 计算每行的实际选区范围，处理短行截断。
    /// </summary>
    /// <param name="lines">文档行列表</param>
    /// <returns>每行的选区信息 (行索引, 起始列, 结束列) 列表</returns>
    public IReadOnlyList<LineSelectionRange> GetLineRanges(IReadOnlyList<string> lines)
    {
        var result = new List<LineSelectionRange>();
        var normalized = Normalized();

        // 列选区的固定列范围
        int fixedStartCol = normalized.StartColumn;
        int fixedEndCol = normalized.EndColumn;

        for (int line = normalized.StartLine; line <= normalized.EndLine; line++)
        {
            if (line < 0 || line >= lines.Count)
                continue;

            int lineLength = lines[line].Length;

            // 如果行长度小于起始列，整个选区对该行无效（空选区）
            if (lineLength < fixedStartCol)
            {
                result.Add(new LineSelectionRange(line, 0, 0));
                continue;
            }

            // 截断到行长度
            int startCol = fixedStartCol;
            int endCol = Math.Min(fixedEndCol, lineLength);

            // 确保 startCol <= endCol
            if (startCol > endCol)
            {
                (startCol, endCol) = (endCol, startCol);
            }

            result.Add(new LineSelectionRange(line, startCol, endCol));
        }

        return result;
    }

    /// <summary>
    /// 偏移选区到新的行范围（用于插入/删除后的位置调整）
    /// </summary>
    public ColumnSelection Offset(int lineDelta, int columnDelta)
    {
        return new ColumnSelection(
            StartLine + lineDelta,
            StartColumn + columnDelta,
            EndLine + lineDelta,
            EndColumn + columnDelta);
    }
}

/// <summary>
/// 单行的选区范围
/// </summary>
public readonly record struct LineSelectionRange(int Line, int StartColumn, int EndColumn)
{
    /// <summary>
    /// 选区长度
    /// </summary>
    public int Length => EndColumn - StartColumn;

    /// <summary>
    /// 选区是否为空
    /// </summary>
    public bool IsEmpty => StartColumn == EndColumn;
}
