using Microsoft.Win32;
using System.Windows;

namespace GEditor.App.Services;

/// <summary>
/// WPF 对话框服务实现
/// </summary>
public sealed class WpfDialogService : IDialogService
{
    public string? ShowOpenFileDialog()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "所有文件 (*.*)|*.*|文本文件 (*.txt)|*.txt|C# 文件 (*.cs)|*.cs|XML 文件 (*.xml)|*.xml|JSON 文件 (*.json)|*.json",
            FilterIndex = 1,
            CheckFileExists = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveFileDialog(string? defaultFileName = null)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "所有文件 (*.*)|*.*|文本文件 (*.txt)|*.txt|C# 文件 (*.cs)|*.cs|XML 文件 (*.xml)|*.xml|JSON 文件 (*.json)|*.json",
            FilterIndex = 1,
            FileName = defaultFileName ?? string.Empty,
            AddExtension = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public bool ShowConfirmDialog(string message, string title = "确认")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
