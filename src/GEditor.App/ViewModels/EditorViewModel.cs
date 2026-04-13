using GEditor.Core.Buffer;
using GEditor.Core.Documents;

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

    public EditorViewModel(Document document)
    {
        _document = document;
        _document.Changed += OnDocumentChanged;
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

    public string LanguageDisplay => "Plain Text";

    public bool IsDirty => _document.IsDirty;

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
