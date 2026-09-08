namespace GEditor.Core.Editing;

/// <summary>
/// 括号匹配服务 - 提供括号配对查找和高亮信息
/// </summary>
public static class BracketMatcher
{
    /// <summary>
    /// 括号配对映射
    /// </summary>
    private static readonly Dictionary<char, char> s_bracketPairs = new()
    {
        { '(', ')' },
        { '[', ']' },
        { '{', '}' },
        { ')', '(' },
        { ']', '[' },
        { '}', '{' }
    };

    /// <summary>
    /// 开括号集合
    /// </summary>
    private static readonly HashSet<char> s_openBrackets = ['(', '[', '{'];

    /// <summary>
    /// 所有括号字符
    /// </summary>
    private static readonly HashSet<char> s_allBrackets = ['(', ')', '[', ']', '{', '}'];

    /// <summary>
    /// 括号匹配结果
    /// </summary>
    public readonly record struct BracketMatch(
        int Line,           // 当前括号所在行 (0-based)
        int Column,         // 当前括号所在列 (0-based)
        char Bracket,       // 当前括号字符
        int MatchLine,      // 匹配括号所在行 (0-based)
        int MatchColumn,    // 匹配括号所在列 (0-based)
        bool IsValid);      // 是否找到有效匹配

    /// <summary>
    /// 检查指定位置是否有括号，并返回匹配结果
    /// </summary>
    /// <param name="lines">文档所有行</param>
    /// <param name="line">光标所在行 (0-based)</param>
    /// <param name="column">光标所在列 (0-based)</param>
    /// <returns>匹配结果，无括号时返回 null</returns>
    public static BracketMatch? FindMatch(IReadOnlyList<string> lines, int line, int column)
    {
        if (lines == null || line < 0 || line >= lines.Count)
            return null;

        var currentLine = lines[line];
        
        // 光标在行尾时检查前一个字符
        int checkCol = column;
        if (checkCol >= currentLine.Length)
            checkCol = currentLine.Length - 1;
        
        if (checkCol < 0)
            return null;

        char currentChar = currentLine[checkCol];

        // 如果当前字符不是括号，尝试检查前一个字符（处理光标在右括号后的情况）
        if (!s_allBrackets.Contains(currentChar))
        {
            if (checkCol > 0)
            {
                checkCol--;
                currentChar = currentLine[checkCol];
            }
            else
            {
                return null;
            }
        }

        if (!s_allBrackets.Contains(currentChar))
            return null;

        // 判断是开括号还是闭括号
        bool isOpen = s_openBrackets.Contains(currentChar);
        char matchingBracket = s_bracketPairs[currentChar];

        if (isOpen)
        {
            // 向后搜索匹配的闭括号
            return FindForward(lines, line, checkCol, currentChar, matchingBracket);
        }
        else
        {
            // 向前搜索匹配的开括号
            return FindBackward(lines, line, checkCol, currentChar, matchingBracket);
        }
    }

    /// <summary>
    /// 向后搜索匹配的闭括号
    /// </summary>
    private static BracketMatch? FindForward(IReadOnlyList<string> lines, int startLine, int startCol, char openBracket, char closeBracket)
    {
        int depth = 1;
        int line = startLine;
        int col = startCol + 1;

        while (line < lines.Count)
        {
            var currentLineText = lines[line];
            
            while (col < currentLineText.Length)
            {
                char c = currentLineText[col];
                if (c == openBracket)
                {
                    depth++;
                }
                else if (c == closeBracket)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return new BracketMatch(startLine, startCol, openBracket, line, col, true);
                    }
                }
                col++;
            }

            // 移动到下一行开头
            line++;
            col = 0;
        }

        // 未找到匹配
        return new BracketMatch(startLine, startCol, openBracket, -1, -1, false);
    }

    /// <summary>
    /// 向前搜索匹配的开括号
    /// </summary>
    private static BracketMatch? FindBackward(IReadOnlyList<string> lines, int startLine, int startCol, char closeBracket, char openBracket)
    {
        int depth = 1;
        int line = startLine;
        int col = startCol - 1;

        while (line >= 0)
        {
            var currentLineText = lines[line];
            
            while (col >= 0)
            {
                char c = currentLineText[col];
                if (c == closeBracket)
                {
                    depth++;
                }
                else if (c == openBracket)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return new BracketMatch(startLine, startCol, closeBracket, line, col, true);
                    }
                }
                col--;
            }

            // 移动到上一行末尾
            line--;
            if (line >= 0)
            {
                col = lines[line].Length - 1;
            }
        }

        // 未找到匹配
        return new BracketMatch(startLine, startCol, closeBracket, -1, -1, false);
    }

    /// <summary>
    /// 检查字符是否为括号
    /// </summary>
    public static bool IsBracket(char c) => s_allBrackets.Contains(c);

    /// <summary>
    /// 获取所有支持的括号对
    /// </summary>
    public static IReadOnlyDictionary<char, char> BracketPairs => s_bracketPairs;
}
