namespace GEditor.Syntax;

/// <summary>
/// 语法高亮器接口 — 每种语言一个实现。
/// 职责：接收文本，输出 Token 列表。不持有状态，不依赖 UI。
/// </summary>
public interface ISyntaxHighlighter
{
    /// <summary>支持的语言名称（如 "C#", "JSON", "XML"）</summary>
    string LanguageName { get; }

    /// <summary>支持的文件扩展名集合（如 { ".cs", ".csx" }）</summary>
    IReadOnlySet<string> SupportedExtensions { get; }

    /// <summary>高亮单行文本，返回 Token 列表</summary>
    IReadOnlyList<SyntaxToken> HighlightLine(string lineText, int lineNumber);

    /// <summary>高亮整个文档，返回逐行 Token 列表</summary>
    SyntaxHighlightResult HighlightDocument(IReadOnlyList<string> lines);
}
