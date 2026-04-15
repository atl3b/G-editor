using GEditor.Core.Documents;
using GEditor.Core.Selection;

namespace GEditor.Core.Buffer;

/// <summary>
/// 文本缓冲区：负责行级别文本存储与编辑操作。
/// 不持有文件路径、编码等元信息，纯文本操作。
/// </summary>
public sealed class EditorBuffer
{
    private readonly List<string> _lines = new();

    public int LineCount => _lines.Count;
    public string this[int index] => _lines[index];
    public IReadOnlyList<string> Lines => _lines.AsReadOnly();

    /// <summary>文本变更事件</summary>
    public event EventHandler<DocumentChangedEventArgs>? Changed;

    private void OnChanged(int startLine, int endLine, string changeType)
        => Changed?.Invoke(this, new DocumentChangedEventArgs
        {
            StartLine = startLine,
            EndLine = endLine,
            ChangeType = changeType
        });

    /// <summary>获取全部文本（用指定换行符拼接）</summary>
    public string GetAllText(string lineEnding)
        => string.Join(lineEnding, _lines);

    /// <summary>设置全部文本（替换现有内容）</summary>
    public void SetAllText(string text)
    {
        _lines.Clear();
        if (string.IsNullOrEmpty(text))
        {
            _lines.Add(string.Empty);
        }
        else
        {
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            _lines.AddRange(lines);
        }
        OnChanged(0, Math.Max(0, _lines.Count - 1), "replace");
    }

    /// <summary>获取指定范围的文本</summary>
    public string GetRange(int startLine, int startCol, int endLine, int endCol)
    {
        if (startLine < 0 || startLine >= _lines.Count)
            throw new ArgumentOutOfRangeException(nameof(startLine));
        if (endLine < 0 || endLine >= _lines.Count)
            throw new ArgumentOutOfRangeException(nameof(endLine));
        if (startCol < 0)
            throw new ArgumentOutOfRangeException(nameof(startCol));
        if (endCol < 0)
            throw new ArgumentOutOfRangeException(nameof(endCol));

        if (startLine == endLine)
        {
            if (startCol > _lines[startLine].Length || endCol > _lines[startLine].Length)
                throw new ArgumentOutOfRangeException(null, "列超出行长度");
            return _lines[startLine].Substring(startCol, endCol - startCol);
        }

        if (startCol > _lines[startLine].Length)
            startCol = _lines[startLine].Length;
        if (endCol > _lines[endLine].Length)
            endCol = _lines[endLine].Length;

        var parts = new List<string> { _lines[startLine][startCol..] };
        for (int i = startLine + 1; i < endLine; i++)
            parts.Add(_lines[i]);
        parts.Add(_lines[endLine][..endCol]);
        return string.Join(Environment.NewLine, parts);
    }

    /// <summary>插入文本到指定位置，返回新光标位置</summary>
    public (int newLine, int newCol) Insert(int line, int column, string text)
    {
        if (string.IsNullOrEmpty(text))
            return (line, column);

        var newLines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string currentLine = _lines[line];

        if (newLines.Length == 1)
        {
            _lines[line] = currentLine.Insert(column, text);
            OnChanged(line, line, "insert");
            return (line, column + text.Length);
        }

        // 多行插入
        string before = currentLine[..column];
        string after = currentLine[column..];

        _lines[line] = before + newLines[0];
        for (int i = 1; i < newLines.Length - 1; i++)
            _lines.Insert(line + i, newLines[i]);
        _lines.Insert(line + newLines.Length - 1, newLines[^1] + after);

        int newCursorLine = line + newLines.Length - 1;
        int newCursorCol = newLines[^1].Length;

        OnChanged(line, newCursorLine, "insert");
        return (newCursorLine, newCursorCol);
    }

    /// <summary>删除指定位置和长度的文本，返回新光标位置</summary>
    public (int newLine, int newCol) Delete(int line, int column, int length)
    {
        if (length <= 0)
            return (line, column);

        // 计算删除范围的起点和终点
        int startLine = line;
        int startCol = column;
        int endLine = line;
        int endCol = column;

        int remaining = length;
        int currentLine = line;
        int currentCol = column;
        bool crossingCompleted = false; // 标记跨越是否完成

        while (remaining > 0 && currentLine < _lines.Count)
        {
            int lineLength = _lines[currentLine].Length;
            int charsInCurrentLine = lineLength - currentCol;

            if (remaining <= charsInCurrentLine)
            {
                // 删除在这一行内完成
                endLine = currentLine;
                // 如果跨越已完成，删除到行首
                endCol = crossingCompleted ? 0 : currentCol + remaining;
                remaining = 0;
                crossingCompleted = true;
            }
            else
            {
                // 需要跨越到下一行
                remaining -= charsInCurrentLine;
                endLine = currentLine + 1;
                currentLine++;
                currentCol = 0;

                // 跨越换行符
                if (remaining > 0)
                {
                    remaining -= 1;
                    if (remaining == 0)
                    {
                        // 跨越完成，删除整行
                        endCol = 0;
                        crossingCompleted = true;
                        break; // 立即结束循环
                    }
                    else if (endLine < _lines.Count)
                    {
                        // 跨越换行符后，还要跨越下一行
                        int nextLineLength = _lines[endLine].Length;
                        if (remaining <= nextLineLength)
                        {
                            // 跨越到下一行后，删除整行，endCol = remaining
                            endCol = remaining;
                            crossingCompleted = true;
                            break; // 立即结束循环
                        }
                    }
                }
            }
        }

        // 确保 endLine 在有效范围内
        if (endLine >= _lines.Count)
        {
            endLine = _lines.Count - 1;
            endCol = _lines[endLine].Length;
        }

        string currentLineText = _lines[startLine];
        string endLineText = _lines[endLine];

        // 处理越界的 endCol
        if (endCol > endLineText.Length)
            endCol = endLineText.Length;

        string before = currentLineText[..startCol];
        string after = endCol < endLineText.Length ? endLineText[endCol..] : string.Empty;

        // 移除被删除的行
        int linesToRemove = endLine - startLine;
        for (int i = 0; i < linesToRemove; i++)
            _lines.RemoveAt(startLine + 1);

        _lines[startLine] = before + after;

        OnChanged(startLine, Math.Max(startLine, endLine - linesToRemove), "delete");
        return (startLine, startCol);
    }

    /// <summary>替换指定范围的文本，返回新光标位置</summary>
    public (int newLine, int newCol) Replace(int line, int column, int length, string newText)
    {
        Delete(line, column, length);
        return Insert(line, column, newText);
    }

    /// <summary>获取指定行的长度</summary>
    public int GetLineLength(int line) => _lines[line].Length;

    /// <summary>
    /// 计算文本在缓冲区中的等效长度（将 \r\n 视为单个换行符）
    /// </summary>
    public static int GetBufferLength(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int count = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                continue; // Skip \n in \r\n sequence
            count++;
        }
        return count;
    }

    #region 列模式操作

    /// <summary>
    /// 获取矩形区域文本（按行返回，每行仅包含选中部分）
    /// </summary>
    /// <param name="selection">列选区（0-based坐标）</param>
    /// <returns>每行选中部分的文本数组</returns>
    public string[] GetColumnText(ColumnSelection selection)
    {
        var normalized = selection.Normalized();
        var lineRanges = normalized.GetLineRanges(_lines.AsReadOnly());
        var result = new string[lineRanges.Count];

        for (int i = 0; i < lineRanges.Count; i++)
        {
            var range = lineRanges[i];
            result[i] = range.IsEmpty
                ? string.Empty
                : _lines[range.Line].Substring(range.StartColumn, range.Length);
        }

        return result;
    }

    /// <summary>
    /// 在多行指定列位置同时插入文本（不触发撤销）
    /// </summary>
    /// <param name="positions">插入位置列表 (行索引, 列索引)，0-based坐标</param>
    /// <param name="text">要插入的文本</param>
    public void InsertAtColumns(IReadOnlyList<(int line, int column)> positions, string text)
    {
        if (string.IsNullOrEmpty(text) || positions.Count == 0)
            return;

        // 按行号排序，从后往前插入以避免行号偏移问题
        var sortedPositions = positions
            .OrderByDescending(p => p.line)
            .ThenByDescending(p => p.column)
            .ToList();

        int minLine = int.MaxValue;
        int maxLine = int.MinValue;

        foreach (var (line, column) in sortedPositions)
        {
            if (line < 0 || line >= _lines.Count)
                continue;

            minLine = Math.Min(minLine, line);
            maxLine = Math.Max(maxLine, line);

            int col = Math.Max(0, Math.Min(column, _lines[line].Length));
            _lines[line] = _lines[line].Insert(col, text);
        }

        if (minLine <= maxLine)
        {
            OnChanged(minLine, maxLine, "column-insert");
        }
    }

    /// <summary>
    /// 从多行指定范围删除文本（不触发撤销）
    /// </summary>
    /// <param name="ranges">删除范围列表 (行索引, 起始列, 长度)</param>
    /// <returns>被删除的文本列表（按行顺序）</returns>
    public string[] DeleteAtColumns(IReadOnlyList<(int line, int column, int length)> ranges)
    {
        if (ranges.Count == 0)
            return Array.Empty<string>();

        // 按行号排序，从后往前删除以避免偏移问题
        var sortedRanges = ranges
            .OrderByDescending(r => r.line)
            .ThenByDescending(r => r.column)
            .ToList();

        var result = new List<string>();

        int minLine = int.MaxValue;
        int maxLine = int.MinValue;

        foreach (var (line, column, length) in sortedRanges)
        {
            if (line < 0 || line >= _lines.Count || length <= 0)
                continue;

            minLine = Math.Min(minLine, line);
            maxLine = Math.Max(maxLine, line);

            int startCol = Math.Max(0, column);
            int endCol = Math.Min(startCol + length, _lines[line].Length);

            if (startCol >= endCol)
                continue;

            string deletedText = _lines[line].Substring(startCol, endCol - startCol);
            result.Add(deletedText);

            string before = _lines[line][..startCol];
            string after = _lines[line][endCol..];
            _lines[line] = before + after;
        }

        if (result.Count > 0 && minLine <= maxLine)
        {
            OnChanged(minLine, maxLine, "column-delete");
        }

        // 返回结果时需要反转顺序以匹配原始顺序
        result.Reverse();
        return result.ToArray();
    }

    #endregion
}
