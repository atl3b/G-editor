using GEditor.Core.Buffer;
using GEditor.Core.Documents;
using GEditor.Core.Editing;
using GEditor.Core.Selection;
using GEditor.Syntax;
using System.IO;
using System.Windows.Input;

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

    // 列模式相关属性
    private bool _isColumnMode;
    private ColumnSelection _columnSelection;

    // 自动换行
    private bool _isWordWrapEnabled;

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

        // 初始化列模式命令
        InitializeColumnModeCommands();
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

    #region 列模式属性和命令

    /// <summary>
    /// 是否处于列模式
    /// </summary>
    public bool IsColumnMode
    {
        get => _isColumnMode;
        set
        {
            if (SetProperty(ref _isColumnMode, value))
            {
                OnPropertyChanged(nameof(ColumnModeText));
            }
        }
    }

    /// <summary>
    /// 列模式状态显示文本
    /// </summary>
    public string ColumnModeText => _isColumnMode ? "[列模式]" : string.Empty;

    /// <summary>
    /// 当前列选区
    /// </summary>
    public ColumnSelection ColumnSelection
    {
        get => _columnSelection;
        set => SetProperty(ref _columnSelection, value);
    }

    /// <summary>
    /// 是否启用自动换行
    /// </summary>
    public bool IsWordWrapEnabled
    {
        get => _isWordWrapEnabled;
        set => SetProperty(ref _isWordWrapEnabled, value);
    }

    /// <summary>
    /// 列模式切换命令
    /// </summary>
    public ICommand ToggleColumnModeCommand { get; private set; } = null!;

    /// <summary>
    /// 列模式插入命令
    /// </summary>
    public ICommand ColumnInsertCommand { get; private set; } = null!;

    /// <summary>
    /// 列模式删除命令
    /// </summary>
    public ICommand ColumnDeleteCommand { get; private set; } = null!;

    /// <summary>
    /// 列模式复制命令
    /// </summary>
    public ICommand ColumnCopyCommand { get; private set; } = null!;

    /// <summary>
    /// 列模式剪切命令
    /// </summary>
    public ICommand ColumnCutCommand { get; private set; } = null!;

    /// <summary>
    /// 列模式粘贴命令
    /// </summary>
    public ICommand ColumnPasteCommand { get; private set; } = null!;

    private void InitializeColumnModeCommands()
    {
        ToggleColumnModeCommand = new RelayCommand(() =>
        {
            IsColumnMode = !IsColumnMode;
            if (!IsColumnMode)
            {
                ColumnSelection = ColumnSelection.Empty;
            }
        });

        ColumnInsertCommand = new RelayCommand<string>(text =>
        {
            if (string.IsNullOrEmpty(text) || Document == null)
                return;

            if (_isColumnMode && !_columnSelection.IsEmpty)
            {
                // 在列选区插入
                var normalized = _columnSelection.Normalized();
                var positions = new List<(int line, int column)>();

                for (int line = normalized.StartLine; line <= normalized.EndLine; line++)
                {
                    int col = line == normalized.StartLine
                        ? normalized.StartColumn
                        : 0;
                    positions.Add((line, col));
                }

                var command = new Core.Editing.ColumnInsertCommand(positions, text);
                Document.UndoRedoManager.Execute(command, Buffer);
            }
            else
            {
                // 在当前光标位置插入
                // 使用0-based坐标
                var command = new InsertTextCommand(_caretLine - 1, _caretColumn - 1, text);
                Document.UndoRedoManager.Execute(command, Buffer);
            }
        });

        ColumnDeleteCommand = new RelayCommand(() =>
        {
            if (Document == null)
                return;

            if (_isColumnMode && !_columnSelection.IsEmpty)
            {
                var command = new Core.Editing.ColumnDeleteCommand(_columnSelection);
                Document.UndoRedoManager.Execute(command, Buffer);
                ColumnSelection = ColumnSelection.Empty;
            }
        });

        ColumnCopyCommand = new RelayCommand(() =>
        {
            if (!_isColumnMode || _columnSelection.IsEmpty || Document == null)
                return;

            var lines = Buffer.GetColumnText(_columnSelection);
            var text = string.Join(Environment.NewLine, lines);
            System.Windows.Clipboard.SetText(text);
        });

        ColumnCutCommand = new RelayCommand(() =>
        {
            if (!_isColumnMode || _columnSelection.IsEmpty || Document == null)
                return;

            var lines = Buffer.GetColumnText(_columnSelection);
            var text = string.Join(Environment.NewLine, lines);
            System.Windows.Clipboard.SetText(text);

            var command = new Core.Editing.ColumnDeleteCommand(_columnSelection);
            Document.UndoRedoManager.Execute(command, Buffer);
            ColumnSelection = ColumnSelection.Empty;
        });

        ColumnPasteCommand = new RelayCommand(() =>
        {
            if (!_isColumnMode || Document == null)
                return;

            if (!System.Windows.Clipboard.ContainsText())
                return;

            var text = System.Windows.Clipboard.GetText();
            if (string.IsNullOrEmpty(text))
                return;

            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (!_columnSelection.IsEmpty)
            {
                // 在列选区位置逐行粘贴
                var normalized = _columnSelection.Normalized();
                var pasteCommands = new List<IEditCommand>();

                for (int i = 0; i < lines.Length && (normalized.StartLine + i) <= normalized.EndLine; i++)
                {
                    int line = normalized.StartLine + i;
                    int col = normalized.StartColumn;

                    if (line >= 0 && line < Buffer.LineCount)
                    {
                        col = Math.Max(0, Math.Min(col, Buffer.GetLineLength(line)));
                        var lineText = lines[i];
                        pasteCommands.Add(new InsertTextCommand(line, col, lineText));
                    }
                }

                if (pasteCommands.Count > 0)
                {
                    var compositeCmd = new CompositeEditCommand("列模式粘贴", pasteCommands);
                    Document.UndoRedoManager.Execute(compositeCmd, Buffer);
                }
            }
            else
            {
                // 在当前光标位置粘贴
                var command = new InsertTextCommand(_caretLine - 1, _caretColumn - 1, text);
                Document.UndoRedoManager.Execute(command, Buffer);
            }
        });
    }

    #endregion

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
