using GEditor.Core.Documents;

namespace GEditor.App.ViewModels;

/// <summary>
/// 文档标签 ViewModel：封装单个文档的标签状态
/// </summary>
public sealed class DocumentTabViewModel : ViewModelBase
{
    private readonly Document _document;
    private bool _isActive;

    public DocumentTabViewModel(Document document)
    {
        _document = document;
        _document.Changed += OnDocumentChanged;
    }

    public Document Document => _document;

    public string Title
    {
        get
        {
            var name = _document.DisplayName;
            return _document.IsDirty ? $"{name}*" : name;
        }
    }

    public bool IsDirty => _document.IsDirty;

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    private void OnDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(IsDirty));
    }

    public void Cleanup()
    {
        _document.Changed -= OnDocumentChanged;
    }

    /// <summary>
    /// 公开属性变更通知（供外部触发特定属性刷新）
    /// </summary>
    public void NotifyPropertyChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
    }
}
