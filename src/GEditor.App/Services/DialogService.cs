namespace GEditor.App.Services;

/// <summary>
/// 对话框服务接口
/// </summary>
public interface IDialogService
{
    /// <summary>显示打开文件对话框</summary>
    string? ShowOpenFileDialog();

    /// <summary>显示保存文件对话框</summary>
    string? ShowSaveFileDialog(string? defaultFileName = null);

    /// <summary>显示确认对话框</summary>
    bool ShowConfirmDialog(string message, string title = "确认");
}
