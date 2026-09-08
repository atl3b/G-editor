namespace GEditor.Core.Editing;

/// <summary>
/// Tab/空格转换服务 - 处理缩进格式转换
/// </summary>
public static class TabSpaceConverter
{
    /// <summary>
    /// 默认 Tab 宽度（空格数）
    /// </summary>
    public const int DefaultTabWidth = 4;

    /// <summary>
    /// 将文本中的 Tab 转换为空格
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <param name="tabWidth">Tab 宽度（空格数）</param>
    /// <returns>转换后的文本</returns>
    public static string TabsToSpaces(string text, int tabWidth = DefaultTabWidth)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var spaces = new string(' ', tabWidth);
        return text.Replace("\t", spaces);
    }

    /// <summary>
    /// 将文本中的前导空格转换为 Tab
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <param name="tabWidth">Tab 宽度（空格数）</param>
    /// <returns>转换后的文本</returns>
    public static string SpacesToTabs(string text, int tabWidth = DefaultTabWidth)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = ConvertLeadingSpacesToTabs(lines[i], tabWidth);
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// 将单行前导空格转换为 Tab
    /// </summary>
    private static string ConvertLeadingSpacesToTabs(string line, int tabWidth)
    {
        if (string.IsNullOrEmpty(line))
            return line;

        // 计算前导空格数量
        int spaceCount = 0;
        foreach (char c in line)
        {
            if (c == ' ')
                spaceCount++;
            else if (c == '\t')
            {
                // 已有的 Tab 按 tabWidth 计算
                spaceCount = ((spaceCount / tabWidth) + 1) * tabWidth;
            }
            else
                break;
        }

        if (spaceCount == 0)
            return line;

        // 计算需要多少个 Tab 和剩余空格
        int tabCount = spaceCount / tabWidth;
        int remainingSpaces = spaceCount % tabWidth;

        var newIndent = new string('\t', tabCount) + new string(' ', remainingSpaces);
        var content = line[spaceCount..];

        return newIndent + content;
    }

    /// <summary>
    /// 检测文本是否包含 Tab 字符
    /// </summary>
    public static bool ContainsTabs(string text)
    {
        return !string.IsNullOrEmpty(text) && text.Contains('\t');
    }

    /// <summary>
    /// 检测文本是否包含前导空格（可能来自 Tab 转换）
    /// </summary>
    public static bool ContainsLeadingSpaces(string text, int minConsecutive = DefaultTabWidth)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        foreach (var line in lines)
        {
            int consecutiveSpaces = 0;
            foreach (char c in line)
            {
                if (c == ' ')
                    consecutiveSpaces++;
                else if (c == '\t')
                    break; // 有 Tab 则不算纯空格缩进
                else
                {
                    if (consecutiveSpaces >= minConsecutive)
                        return true;
                    break;
                }
            }
            
            // 行末全是空格的情况
            if (consecutiveSpaces >= minConsecutive)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 转换结果信息
    /// </summary>
    public readonly record struct ConversionResult(
        string ConvertedText,
        int ReplacedCount,
        string Message);

    /// <summary>
    /// 执行 Tab 到空格转换并返回结果信息
    /// </summary>
    public static ConversionResult ConvertTabsToSpaces(string text, int tabWidth = DefaultTabWidth)
    {
        if (string.IsNullOrEmpty(text))
            return new ConversionResult(text, 0, "文本为空");

        int tabCount = 0;
        foreach (char c in text)
        {
            if (c == '\t') tabCount++;
        }

        if (tabCount == 0)
            return new ConversionResult(text, 0, "未找到 Tab 字符");

        var converted = TabsToSpaces(text, tabWidth);
        return new ConversionResult(converted, tabCount, $"已将 {tabCount} 个 Tab 转换为空格");
    }

    /// <summary>
    /// 执行空格到 Tab 转换并返回结果信息
    /// </summary>
    public static ConversionResult ConvertSpacesToTabs(string text, int tabWidth = DefaultTabWidth)
    {
        if (string.IsNullOrEmpty(text))
            return new ConversionResult(text, 0, "文本为空");

        var converted = SpacesToTabs(text, tabWidth);
        
        // 计算替换了多少组空格
        int replacedGroups = 0;
        bool hasChanges = false;
        
        for (int i = 0; i < Math.Min(text.Length, converted.Length); i++)
        {
            if (text[i] != converted[i])
            {
                hasChanges = true;
                if (converted[i] == '\t')
                    replacedGroups++;
            }
        }

        if (!hasChanges)
            return new ConversionResult(text, 0, "无需转换（已使用 Tab 或无前导空格）");

        return new ConversionResult(converted, replacedGroups, $"已将 {replacedGroups} 组空格转换为 Tab");
    }
}
