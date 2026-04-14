using GEditor.Core.Buffer;
using GEditor.Core.Documents;
using GEditor.Syntax;
using System.IO;

namespace GEditor.App.ViewModels;

/// <summary>
/// 编辑区 ViewModel：封装单个文档的编辑状态
/// </summary>
public sealed class EditorViewModel : ViewModelBase
{
    private Document _document;
    private int _caretLine = 1;
    private int _caretColumn = 1;
    private string _selectedText = string.Empty;
    private bool _hasSelection;
    private string _currentLanguage = "Plain Text";
    private ISyntaxHighlighter? _currentHighlighter;

    public EditorViewModel(Document document, ISyntaxHighlighterRegistry? registry = null)
    {
        _document = document;
        _document.Changed += OnDocumentChanged;

        // 根据文件扩展名自动检测语言
        if (registry != null && !string.IsNullOrEmpty(document.FilePath))
        {
            var extension = Path.GetExtension(document.FilePath);
            _currentHighlighter = registry.GetHighlighterByExtension(extension);
            if (_currentHighlighter != null)
            {
                _currentLanguage = _currentHighlighter.LanguageName;
            }
        }
    }

    public Document Document => _document;

    public EditorBuffer Buffer => _document.Buffer;

    public string Text
    {
        get => _document.Buffer.GetAllText("\n");
        set
        {
            _document.LoadText(value);
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<string> Lines => _document.Buffer.Lines;

    public int LineCount => _document.Buffer.LineCount;

    public int CaretLine
    {
        get => _caretLine;
        set => SetProperty(ref _caretLine, Math.Max(1, Math.Min(value, LineCount)));
    }

    public int CaretColumn
    {
        get => _caretColumn;
        set => SetProperty(ref _caretColumn, Math.Max(1, value));
    }

    public string SelectedText
    {
        get => _selectedText;
        set => SetProperty(ref _selectedText, value);
    }

    public bool HasSelection
    {
        get => _hasSelection;
        set => SetProperty(ref _hasSelection, value);
    }

    public string EncodingDisplay => _document.EncodingInfo.DisplayName;

    public string LineEndingDisplay => _document.LineEndingInfo.DetectedLineEnding.ToString();

    public string LanguageDisplay
    {
        get => _currentLanguage;
        set
        {
            if (SetProperty(ref _currentLanguage, value))
            {
                OnPropertyChanged(nameof(HasHighlighting));
            }
        }
    }

    public bool HasHighlighting => _currentHighlighter != null && _currentHighlighter.LanguageName != "Plain Text";

    /// <summary>
    /// 当前语法高亮器
    /// </summary>
    public ISyntaxHighlighter? CurrentHighlighter => _currentHighlighter;

    public bool IsDirty => _document.IsDirty;

    /// <summary>
    /// 设置当前语言高亮器
    /// </summary>
    public void SetLanguage(string languageName, ISyntaxHighlighterRegistry registry)
    {
        var highlighter = registry.GetHighlighterByLanguage(languageName);
        if (highlighter != null)
        {
            _currentHighlighter = highlighter;
            _currentLanguage = languageName;
            OnPropertyChanged(nameof(LanguageDisplay));
            OnPropertyChanged(nameof(HasHighlighting));
        }
    }

    /// <summary>
    /// 获取指定行的语法 Token（用于高亮渲染）
    /// </summary>
    public IReadOnlyList<SyntaxToken>? GetHighlightedTokens(int lineNumber)
    {
        if (_currentHighlighter == null || lineNumber < 0 || lineNumber >= LineCount)
            return null;

        return _currentHighlighter.HighlightLine(Lines[lineNumber], lineNumber);
    }

    /// <summary>
    /// 获取整个文档的高亮结果
    /// </summary>
    public SyntaxHighlightResult? GetHighlightedDocument()
    {
        if (_currentHighlighter == null)
            return null;

        return _currentHighlighter.HighlightDocument(Lines);
    }

    public void UpdateCaretPosition(int line, int column)
    {
        if (SetProperty(ref _caretLine, line) || SetProperty(ref _caretColumn, column))
        {
            OnPropertyChanged(nameof(CaretLine));
            OnPropertyChanged(nameof(CaretColumn));
        }
    }

    public void SetDocument(Document document)
    {
        if (_document != document)
        {
            _document.Changed -= OnDocumentChanged;
            _document = document;
            _document.Changed += OnDocumentChanged;
            OnPropertyChanged(nameof(Document));
            OnPropertyChanged(nameof(Buffer));
            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(Lines));
            OnPropertyChanged(nameof(LineCount));
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(EncodingDisplay));
            OnPropertyChanged(nameof(LineEndingDisplay));
        }
    }

    private void OnDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(Lines));
        OnPropertyChanged(nameof(LineCount));
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(EncodingDisplay));
        OnPropertyChanged(nameof(LineEndingDisplay));
    }

    public void Cleanup()
    {
        _document.Changed -= OnDocumentChanged;
    }
}
