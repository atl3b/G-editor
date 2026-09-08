namespace GEditor.Core.Editing;

/// <summary>
/// 自动缩进服务 - 处理回车时的智能缩进
/// </summary>
public static class AutoIndenter
{
    /// <summary>
    /// 默认缩进大小（空格数）
    /// </summary>
    public const int DefaultIndentSize = 4;

    /// <summary>
    /// 获取指定行的缩进（前导空格/Tab数量）
    /// </summary>
    public static string GetLineIndent(string line)
    {
        if (string.IsNullOrEmpty(line))
            return string.Empty;

        int indent = 0;
        foreach (char c in line)
        {
            if (c == ' ')
                indent++;
            else if (c == '\t')
                indent += DefaultIndentSize; // Tab 视为 4 空格
            else
                break;
        }

        return line[..indent];
    }

    /// <summary>
    /// 检查行是否以增加缩进的字符结尾（如 {、(、: 等）
    /// </summary>
    public static bool ShouldIncreaseIndent(string line, bool trimEnd = true)
    {
        var trimmed = trimEnd ? line.TrimEnd() : line;
        if (string.IsNullOrEmpty(trimmed))
            return false;

        char lastChar = trimmed[^1];
        return lastChar is '{' or '(' or ':' or '[';
    }

    /// <summary>
    /// 检查行是否以减少缩进的字符开头（如 }、) 等）
    /// </summary>
    public static bool ShouldDecreaseIndent(string line, bool trimStart = true)
    {
        var trimmed = trimStart ? line.TrimStart() : line;
        if (string.IsNullOrEmpty(trimmed))
            return false;

        char firstChar = trimmed[0];
        return firstChar is '}' or ')' or ']';
    }

    /// <summary>
    /// 计算新行的缩进
    /// </summary>
    /// <param name="currentLine">当前行内容</param>
    /// <param name="nextLineContent">下一行的起始内容（如果有）</param>
    /// <param name="indentSize">缩进大小</param>
    /// <returns>应该插入的缩进字符串</returns>
    public static string CalculateNewLineIndent(string currentLine, string? nextLineContent = null, int indentSize = DefaultIndentSize)
    {
        // 获取当前行的缩进
        var baseIndent = GetLineIndent(currentLine);
        
        // 检查是否需要减少缩进（下一行以 } 开头）
        if (!string.IsNullOrEmpty(nextLineContent) && ShouldDecreaseIndent(nextLineContent))
        {
            // 减少一级缩进
            if (baseIndent.Length >= indentSize)
            {
                baseIndent = baseIndent[..^indentSize];
            }
            return baseIndent;
        }

        // 检查是否需要增加缩进（当前行以 { 结尾）
        if (ShouldIncreaseIndent(currentLine))
        {
            // 增加一级缩进
            baseIndent += new string(' ', indentSize);
        }

        return baseIndent;
    }

    /// <summary>
    /// 计算回车后应插入的完整文本（包含换行符和缩进）
    /// </summary>
    /// <param name="lines">文档所有行</param>
    /// <param name="currentLineIndex">当前行索引 (0-based)</param>
    /// <param name="currentColumn">当前列位置 (0-based)</param>
    /// <param name="indentSize">缩进大小</param>
    /// <returns>应插入的文本（换行符 + 缩进）</returns>
    public static string GetAutoIndentText(IReadOnlyList<string> lines, int currentLineIndex, int currentColumn, int indentSize = DefaultIndentSize)
    {
        if (lines == null || currentLineIndex < 0 || currentLineIndex >= lines.Count)
            return "\n";

        var currentLine = lines[currentLineIndex];
        
        // 获取光标前的部分（用于判断是否在 { 后面按回车）
        var beforeCursor = currentColumn < currentLine.Length 
            ? currentLine[..currentColumn] 
            : currentLine;

        // 计算基础缩进
        var baseIndent = GetLineIndent(beforeCursor);

        // 检查光标前是否需要增加缩进
        var trimmedBefore = beforeCursor.TrimEnd();
        if (!string.IsNullOrEmpty(trimmedBefore))
        {
            char lastChar = trimmedBefore[^1];
            if (lastChar is '{' or '(' or ':')
            {
                baseIndent += new string(' ', indentSize);
            }
        }

        return "\n" + baseIndent;
    }
}
