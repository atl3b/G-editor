using GEditor.App.Services;
using GEditor.Core.Documents;
using GEditor.Core.IO;
using GEditor.Core.Management;
using GEditor.Core.Search;
using GEditor.Syntax;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace GEditor.App.ViewModels;

/// <summary>
/// 主窗口 ViewModel：管理所有文档和命令
/// </summary>
public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IDocumentManager _documentManager;
    private readonly ITextFileService _fileService;
    private readonly IDialogService _dialogService;
    private readonly ISearchService _searchService;
    private readonly ISyntaxHighlighterRegistry _syntaxRegistry;

    private DocumentTabViewModel? _activeTab;
    private EditorViewModel? _activeEditor;
    private SearchPanelViewModel? _searchPanel;
    private string _windowTitle = "G-editor";

    public MainWindowViewModel(
        IDocumentManager documentManager,
        ITextFileService fileService,
        IDialogService dialogService,
        ISearchService searchService,
        ISyntaxHighlighterRegistry syntaxRegistry)
    {
        _documentManager = documentManager ?? throw new ArgumentNullException(nameof(documentManager));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _syntaxRegistry = syntaxRegistry ?? throw new ArgumentNullException(nameof(syntaxRegistry));

        Documents = new ObservableCollection<DocumentTabViewModel>();
        StatusBar = new StatusBarViewModel();

        // 初始化命令
        InitializeCommands();

        // 创建新文档
        NewDocument();
    }

    public ISyntaxHighlighterRegistry SyntaxRegistry => _syntaxRegistry;

    #region 属性

    public ObservableCollection<DocumentTabViewModel> Documents { get; }

    public DocumentTabViewModel? ActiveTab
    {
        get => _activeTab;
        set
        {
            if (_activeTab != value)
            {
                if (_activeTab != null)
                    _activeTab.IsActive = false;

                _activeTab = value;

                if (_activeTab != null)
                {
                    _activeTab.IsActive = true;
                    _documentManager.SetActive(_activeTab.Document);
                    ActiveEditor = new EditorViewModel(_activeTab.Document, _syntaxRegistry);
                    // 创建或更新搜索面板
                    SearchPanel = new SearchPanelViewModel(_searchService, _activeTab.Document);
                    UpdateWindowTitle();
                    UpdateStatusBar();
                }
                else
                {
                    ActiveEditor = null;
                }

                OnPropertyChanged();
                UpdateStatusBar();
            }
        }
    }

    public EditorViewModel? ActiveEditor
    {
        get => _activeEditor;
        private set
        {
            if (_activeEditor != value)
            {
                _activeEditor = value;
                OnPropertyChanged();
            }
        }
    }

    public StatusBarViewModel StatusBar { get; }

    public SearchPanelViewModel? SearchPanel
    {
        get => _searchPanel;
        private set => SetProperty(ref _searchPanel, value);
    }

    public string WindowTitle
    {
        get => _windowTitle;
        private set => SetProperty(ref _windowTitle, value);
    }

    #endregion

    #region 命令

    public ICommand NewCommand { get; private set; } = null!;
    public ICommand OpenCommand { get; private set; } = null!;
    public ICommand SaveCommand { get; private set; } = null!;
    public ICommand SaveAsCommand { get; private set; } = null!;
    public ICommand CloseCommand { get; private set; } = null!;
    public ICommand ExitCommand { get; private set; } = null!;

    public ICommand UndoCommand { get; private set; } = null!;
    public ICommand RedoCommand { get; private set; } = null!;
    public ICommand CutCommand { get; private set; } = null!;
    public ICommand CopyCommand { get; private set; } = null!;
    public ICommand PasteCommand { get; private set; } = null!;
    public ICommand SelectAllCommand { get; private set; } = null!;

    public ICommand FindCommand { get; private set; } = null!;
    public ICommand ReplaceCommand { get; private set; } = null!;
    public ICommand FindNextCommand { get; private set; } = null!;
    public ICommand FindPreviousCommand { get; private set; } = null!;

    // 编码切换命令
    public ICommand ReopenWithEncodingCommand { get; private set; } = null!;
    public ICommand SaveWithEncodingCommand { get; private set; } = null!;

    // 换行符切换命令
    public ICommand ChangeLineEndingCommand { get; private set; } = null!;

    // 语言切换命令
    public ICommand ChangeLanguageCommand { get; private set; } = null!;

    private void InitializeCommands()
    {
        // 文件命令
        NewCommand = new RelayCommand(NewDocument);
        OpenCommand = new RelayCommand(async () => await OpenDocumentAsync());
        SaveCommand = new RelayCommand(SaveDocument, () => ActiveTab != null);
        SaveAsCommand = new RelayCommand(async () => await SaveDocumentAsAsync(), () => ActiveTab != null);
        CloseCommand = new RelayCommand(CloseActiveDocument, () => ActiveTab != null);
        ExitCommand = new RelayCommand(ExitApplication);

        // 编辑命令
        UndoCommand = new RelayCommand(Undo, () => ActiveTab?.Document.UndoRedoManager.CanUndo == true);
        RedoCommand = new RelayCommand(Redo, () => ActiveTab?.Document.UndoRedoManager.CanRedo == true);
        CutCommand = new RelayCommand(Cut, () => ActiveEditor?.HasSelection == true);
        CopyCommand = new RelayCommand(Copy, () => ActiveEditor?.HasSelection == true);
        PasteCommand = new RelayCommand(Paste, () => ActiveTab != null);
        SelectAllCommand = new RelayCommand(SelectAll, () => ActiveTab != null);

        // 搜索命令
        FindCommand = new RelayCommand(OpenFindPanel);
        ReplaceCommand = new RelayCommand(OpenReplacePanel);
        FindNextCommand = new RelayCommand(FindNext, () => SearchPanel?.IsVisible == true);
        FindPreviousCommand = new RelayCommand(FindPrevious, () => SearchPanel?.IsVisible == true);

        // 编码切换命令
        ReopenWithEncodingCommand = new RelayCommand<string>(ReopenWithEncoding);
        SaveWithEncodingCommand = new RelayCommand<string>(SaveWithEncoding);

        // 换行符切换命令
        ChangeLineEndingCommand = new RelayCommand<string>(ChangeLineEnding);

        // 语言切换命令
        ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);
    }

    #endregion

    #region 文件操作

    private void NewDocument()
    {
        var document = _documentManager.CreateNew();
        var tab = new DocumentTabViewModel(document);
        Documents.Add(tab);
        ActiveTab = tab;
    }

    private async Task OpenDocumentAsync()
    {
        var filePath = _dialogService.ShowOpenFileDialog();
        if (string.IsNullOrEmpty(filePath))
            return;

        // 检查是否已打开该文件
        var existingTab = Documents.FirstOrDefault(t => t.Document.FilePath == filePath);
        if (existingTab != null)
        {
            ActiveTab = existingTab;
            return;
        }

        try
        {
            var document = await _documentManager.OpenAsync(filePath);
            var tab = new DocumentTabViewModel(document);
            Documents.Add(tab);
            ActiveTab = tab;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法打开文件: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveDocument()
    {
        if (ActiveTab == null) return;

        var document = ActiveTab.Document;

        if (document.IsNew)
        {
            SaveDocumentAsAsync().Wait();
        }
        else
        {
            try
            {
                _fileService.Save(document);
                document.MarkAsSaved();
                ActiveTab.RaisePropertyChanged(nameof(DocumentTabViewModel.Title));
                ActiveTab.RaisePropertyChanged(nameof(DocumentTabViewModel.IsDirty));
                UpdateWindowTitle();
                StatusBar.Status = "已保存";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法保存文件: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private Task SaveDocumentAsAsync()
    {
        if (ActiveTab == null) return Task.CompletedTask;

        var filePath = _dialogService.ShowSaveFileDialog(ActiveTab.Document.DisplayName);
        if (string.IsNullOrEmpty(filePath))
            return Task.CompletedTask;

        try
        {
            _fileService.SaveAs(ActiveTab.Document, filePath);
            ActiveTab.Document.MarkAsSaved();
            ActiveTab.RaisePropertyChanged(nameof(DocumentTabViewModel.Title));
            ActiveTab.RaisePropertyChanged(nameof(DocumentTabViewModel.IsDirty));
            UpdateWindowTitle();
            StatusBar.Status = "已保存";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法保存文件: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        return Task.CompletedTask;
    }

    private void CloseActiveDocument()
    {
        if (ActiveTab == null) return;

        var document = ActiveTab.Document;

        if (document.IsDirty)
        {
            var result = MessageBox.Show(
                $"是否保存对 \"{document.DisplayName}\" 的更改?",
                "G-editor",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return;

            if (result == MessageBoxResult.Yes)
            {
                if (document.IsNew)
                    SaveDocumentAsAsync().Wait();
                else
                    SaveDocument();
            }
        }

        var tabToClose = ActiveTab;
        var index = Documents.IndexOf(tabToClose);
        Documents.Remove(tabToClose);
        tabToClose.Cleanup();

        // 切换到相邻标签
        if (Documents.Count > 0)
            ActiveTab = Documents[Math.Max(0, index - 1)];
        else
            NewDocument(); // 如果没有文档了，创建新文档
    }

    private void ExitApplication()
    {
        // 检查是否有未保存的文档
        var dirtyDocs = Documents.Where(t => t.Document.IsDirty).ToList();
        if (dirtyDocs.Any())
        {
            var result = MessageBox.Show(
                $"有 {dirtyDocs.Count} 个文档尚未保存。是否退出?",
                "G-editor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;
        }

        Application.Current.Shutdown();
    }

    #endregion

    #region 编辑操作

    private void Undo()
    {
        ActiveTab?.Document.Undo();
    }

    private void Redo()
    {
        ActiveTab?.Document.Redo();
    }

    private void Cut()
    {
        // 剪切通过 TextBox 的内置功能处理
    }

    private void Copy()
    {
        // 复制通过 TextBox 的内置功能处理
    }

    private void Paste()
    {
        // 粘贴通过 TextBox 的内置功能处理
    }

    private void SelectAll()
    {
        // 全选通过 TextBox 的内置功能处理
    }

    #endregion

    #region 搜索操作

    private void OpenFindPanel()
    {
        if (SearchPanel == null && ActiveTab != null)
        {
            SearchPanel = new SearchPanelViewModel(_searchService, ActiveTab.Document);
        }
        SearchPanel?.Show();
        StatusBar.Status = "查找功能 (Ctrl+F)";
    }

    private void OpenReplacePanel()
    {
        if (SearchPanel == null && ActiveTab != null)
        {
            SearchPanel = new SearchPanelViewModel(_searchService, ActiveTab.Document);
        }
        SearchPanel?.ShowWithReplace();
        StatusBar.Status = "替换功能 (Ctrl+H)";
    }

    private void FindNext()
    {
        SearchPanel?.FindNext();
    }

    private void FindPrevious()
    {
        SearchPanel?.FindPrevious();
    }

    #endregion

    #region 编码操作

    private void ReopenWithEncoding(string? encodingName)
    {
        if (ActiveTab == null || string.IsNullOrEmpty(encodingName)) return;

        if (ActiveTab.Document.IsDirty)
        {
            var result = MessageBox.Show(
                $"是否保存对 \"{ActiveTab.Document.DisplayName}\" 的更改?",
                "G-editor",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel) return;
            if (result == MessageBoxResult.Yes) SaveDocument();
        }

        var encoding = GetEncodingByName(encodingName);
        if (encoding == null) return;

        try
        {
            var filePath = ActiveTab.Document.FilePath;
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("该文档尚未保存，无法重新打开。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 重新加载文件
            var oldDoc = ActiveTab.Document;
            var newDoc = _fileService.Open(filePath);
            newDoc.EncodingInfo = new DocumentEncodingInfo
            {
                Encoding = encoding,
                HasBom = encoding.GetPreamble().Length > 0,
                DisplayName = encodingName
            };

            var tabIndex = Documents.IndexOf(ActiveTab);
            Documents.Remove(ActiveTab);
            oldDoc.Dispose();

            var newTab = new DocumentTabViewModel(newDoc);
            Documents.Insert(tabIndex, newTab);
            ActiveTab = newTab;

            StatusBar.Status = $"已重新打开为 {encodingName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法重新打开文件: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveWithEncoding(string? encodingName)
    {
        if (ActiveTab == null || string.IsNullOrEmpty(encodingName)) return;

        var encoding = GetEncodingByName(encodingName);
        if (encoding == null) return;

        try
        {
            if (ActiveTab.Document.IsNew)
            {
                var filePath = _dialogService.ShowSaveFileDialog(ActiveTab.Document.DisplayName);
                if (string.IsNullOrEmpty(filePath)) return;
                _fileService.SaveAs(ActiveTab.Document, filePath, encoding);
            }
            else
            {
                // 保存为指定编码
                var tempPath = ActiveTab.Document.FilePath + ".tmp";
                _fileService.SaveAs(ActiveTab.Document, tempPath, encoding);
                File.Delete(ActiveTab.Document.FilePath);
                File.Move(tempPath, ActiveTab.Document.FilePath);
                ActiveTab.Document.MarkAsSaved();
            }

            ActiveTab.RaisePropertyChanged(nameof(DocumentTabViewModel.Title));
            ActiveTab.RaisePropertyChanged(nameof(DocumentTabViewModel.IsDirty));
            UpdateStatusBar();
            StatusBar.Status = $"已保存为 {encodingName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法保存文件: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private System.Text.Encoding? GetEncodingByName(string name)
    {
        return name switch
        {
            "UTF-8" => new System.Text.UTF8Encoding(false),
            "UTF-8 (BOM)" => System.Text.Encoding.UTF8,
            "UTF-16 LE" => System.Text.Encoding.Unicode,
            "UTF-16 BE" => System.Text.Encoding.BigEndianUnicode,
            "GB2312" => System.Text.Encoding.GetEncoding("GB2312"),
            "GBK" => System.Text.Encoding.GetEncoding("GBK"),
            "ANSI" => System.Text.Encoding.Default,
            _ => null
        };
    }

    #endregion

    #region 换行符操作

    private void ChangeLineEnding(string? lineEndingName)
    {
        if (ActiveTab == null || string.IsNullOrEmpty(lineEndingName)) return;

        var lineEnding = lineEndingName switch
        {
            "Windows (CRLF)" => LineEnding.CRLF,
            "Unix (LF)" => LineEnding.LF,
            "Mac (CR)" => LineEnding.CR,
            _ => (LineEnding?)null
        };

        if (lineEnding == null) return;

        ActiveTab.Document.LineEndingInfo = new DocumentLineEndingInfo
        {
            DetectedLineEnding = ActiveTab.Document.LineEndingInfo.DetectedLineEnding,
            ActiveLineEnding = lineEnding.Value
        };

        UpdateStatusBar();
        StatusBar.Status = $"换行符已更改为 {lineEndingName}";
    }

    #endregion

    #region 语言操作

    private void ChangeLanguage(string? languageName)
    {
        if (ActiveTab == null || string.IsNullOrEmpty(languageName)) return;

        if (ActiveEditor != null)
        {
            ActiveEditor.SetLanguage(languageName, _syntaxRegistry);
            StatusBar.Language = ActiveEditor.LanguageDisplay;
        }
        else
        {
            StatusBar.Language = languageName;
        }
        StatusBar.Status = $"语言已更改为 {languageName}";
    }

    #endregion

    #region 辅助方法

    private void UpdateWindowTitle()
    {
        var docName = ActiveTab?.Document.DisplayName ?? "无文档";
        var modified = ActiveTab?.Document.IsDirty == true ? "*" : "";
        WindowTitle = $"G-editor - {docName}{modified}";
    }

    private void UpdateStatusBar()
    {
        if (ActiveTab == null || ActiveEditor == null)
        {
            StatusBar.Encoding = "UTF-8";
            StatusBar.LineEnding = "CRLF";
            StatusBar.Line = 1;
            StatusBar.Column = 1;
            StatusBar.Language = "Plain Text";
            StatusBar.Status = "就绪";
            return;
        }

        var doc = ActiveTab.Document;
        StatusBar.Encoding = doc.EncodingInfo.DisplayName;
        StatusBar.LineEnding = doc.LineEndingInfo.DetectedLineEnding.ToString();
        StatusBar.Line = ActiveEditor.CaretLine;
        StatusBar.Column = ActiveEditor.CaretColumn;
        StatusBar.Language = ActiveEditor.LanguageDisplay;
        StatusBar.Status = "就绪";
    }

    public void UpdateCaretPosition(int line, int column)
    {
        ActiveEditor?.UpdateCaretPosition(line, column);
        if (ActiveEditor != null)
        {
            StatusBar.Line = ActiveEditor.CaretLine;
            StatusBar.Column = ActiveEditor.CaretColumn;
        }
    }

    #endregion
}

// 扩展方法用于 DocumentTabViewModel
public static class DocumentTabViewModelExtensions
{
    public static void RaisePropertyChanged(this DocumentTabViewModel vm, string propertyName)
    {
        vm.GetType().BaseType?.GetMethod("OnPropertyChanged")?.Invoke(vm, new object[] { propertyName });
    }
}
