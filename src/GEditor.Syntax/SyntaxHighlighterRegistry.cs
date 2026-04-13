namespace GEditor.Syntax;

/// <summary>语法高亮器注册中心</summary>
public sealed class SyntaxHighlighterRegistry : ISyntaxHighlighterRegistry
{
    private readonly Dictionary<string, ISyntaxHighlighter> _extensionMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ISyntaxHighlighter> _languageMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ISyntaxHighlighter> _highlighters = new();

    public IReadOnlyList<ISyntaxHighlighter> Highlighters => _highlighters.AsReadOnly();

    public IReadOnlyList<ILanguageDefinition> Languages =>
        _highlighters.Select(h => (ILanguageDefinition)new LanguageDefinition
        {
            LanguageId = h.LanguageName.ToLowerInvariant(),
            DisplayName = h.LanguageName,
            FileExtensions = h.SupportedExtensions
        }).ToList().AsReadOnly();

    public void Register(ISyntaxHighlighter highlighter)
    {
        ArgumentNullException.ThrowIfNull(highlighter);
        _highlighters.Add(highlighter);
        _languageMap[highlighter.LanguageName] = highlighter;

        foreach (var ext in highlighter.SupportedExtensions)
            _extensionMap[ext] = highlighter;
    }

    public ISyntaxHighlighter? GetHighlighterByExtension(string fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension)) return null;
        var ext = fileExtension.StartsWith('.') ? fileExtension : $".{fileExtension}";
        return _extensionMap.GetValueOrDefault(ext);
    }

    public ISyntaxHighlighter? GetHighlighterByLanguage(string languageName)
    {
        if (string.IsNullOrEmpty(languageName)) return null;
        return _languageMap.GetValueOrDefault(languageName);
    }

    public bool IsSupported(string fileExtension)
        => GetHighlighterByExtension(fileExtension) != null;
}
