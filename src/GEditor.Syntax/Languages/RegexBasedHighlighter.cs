using System.Text.RegularExpressions;

namespace GEditor.Syntax;

/// <summary>
/// 基于正则规则的高亮器基类 — 提供通用的高亮执行引擎。
/// </summary>
public abstract class RegexBasedHighlighter : ISyntaxHighlighter
{
    public abstract string LanguageName { get; }
    public abstract IReadOnlySet<string> SupportedExtensions { get; }

    protected abstract IReadOnlyList<HighlightRule> GetRules();

    public virtual IReadOnlyList<SyntaxToken> HighlightLine(string lineText, int lineNumber)
    {
        var tokens = new List<SyntaxToken>();
        int position = 0;

        if (string.IsNullOrEmpty(lineText))
            return tokens;

        while (position < lineText.Length)
        {
            SyntaxToken? bestMatch = null;

            foreach (var rule in GetRules())
            {
                var match = rule.Pattern.Match(lineText, position);
                if (match.Success && match.Index == position)
                {
                    bestMatch = new SyntaxToken
                    {
                        Kind = rule.Kind,
                        StartColumn = position,
                        Length = match.Length,
                        Text = match.Value,
                        LineNumber = lineNumber
                    };
                    break;
                }
            }

            if (bestMatch != null)
            {
                tokens.Add(bestMatch);
                position += bestMatch.Length;
            }
            else
            {
                tokens.Add(new SyntaxToken
                {
                    Kind = TokenKind.None,
                    StartColumn = position,
                    Length = 1,
                    Text = lineText[position].ToString(),
                    LineNumber = lineNumber
                });
                position++;
            }
        }

        return tokens;
    }

    public virtual SyntaxHighlightResult HighlightDocument(IReadOnlyList<string> lines)
    {
        var lineTokens = new IReadOnlyList<SyntaxToken>[lines.Count];
        for (int i = 0; i < lines.Count; i++)
            lineTokens[i] = HighlightLine(lines[i], i);

        return new SyntaxHighlightResult
        {
            LanguageName = LanguageName,
            LineTokens = lineTokens
        };
    }

    protected static Regex Compile(string pattern)
        => new(pattern, RegexOptions.Compiled);
}
