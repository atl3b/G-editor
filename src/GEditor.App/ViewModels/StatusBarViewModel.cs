namespace GEditor.App.ViewModels;

/// <summary>
/// 状态栏 ViewModel：显示文档信息
/// </summary>
public sealed class StatusBarViewModel : ViewModelBase
{
    private string _encoding = "UTF-8";
    private string _lineEnding = "CRLF";
    private int _line = 1;
    private int _column = 1;
    private string _language = "Plain Text";
    private string _status = "就绪";
    private string _columnMode = string.Empty;
    private bool _isColumnModeActive;

    public string Encoding
    {
        get => _encoding;
        set => SetProperty(ref _encoding, value);
    }

    public string LineEnding
    {
        get => _lineEnding;
        set => SetProperty(ref _lineEnding, value);
    }

    public int Line
    {
        get => _line;
        set => SetProperty(ref _line, value);
    }

    public int Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    public string Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string ColumnMode
    {
        get => _columnMode;
        set => SetProperty(ref _columnMode, value);
    }

    /// <summary>
    /// 列模式是否激活（用于状态栏高亮）
    /// </summary>
    public bool IsColumnModeActive
    {
        get => _isColumnModeActive;
        set => SetProperty(ref _isColumnModeActive, value);
    }

    public string PositionText => $"Ln {_line}, Col {_column}";
}
