using GEditor.Core.Documents;

namespace GEditor.Core.IO;

/// <summary>换行符检测器 — CRLF/LF/CR 识别</summary>
public sealed class LineEndingDetector : ILineEndingDetector
{
    public LineEnding Detect(string text)
    {
        if (string.IsNullOrEmpty(text))
            return LineEnding.Unknown;

        int crlfCount = 0;
        int lfCount = 0;
        int crCount = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    crlfCount++;
                    i++; // skip \n
                }
                else
                {
                    crCount++;
                }
            }
            else if (text[i] == '\n')
            {
                lfCount++;
            }
        }

        // Return the dominant line ending
        if (crlfCount >= lfCount && crlfCount >= crCount && crlfCount > 0)
            return LineEnding.CRLF;
        if (lfCount >= crCount && lfCount > 0)
            return LineEnding.LF;
        if (crCount > 0)
            return LineEnding.CR;

        return LineEnding.Unknown;
    }
}
