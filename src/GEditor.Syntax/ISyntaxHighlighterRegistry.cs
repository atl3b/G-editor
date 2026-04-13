namespace GEditor.Syntax;

/// <summary>高亮器注册中心接口</summary>
public interface ISyntaxHighlighterRegistry
{
    IReadOnlyList<ISyntaxHighlighter> Highlighters { get; }
    IReadOnlyList<ILanguageDefinition> Languages { get; }
    void Register(ISyntaxHighlighter highlighter);
    ISyntaxHighlighter? GetHighlighterByExtension(string fileExtension);
    ISyntaxHighlighter? GetHighlighterByLanguage(string languageName);
    bool IsSupported(string fileExtension);
}
