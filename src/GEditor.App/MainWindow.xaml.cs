using System.Windows;
using System.Windows.Controls;
using GEditor.App.Controls;
using GEditor.App.ViewModels;

namespace GEditor.App;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    private ScrollViewer? _editorScrollViewer;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;

        // 连接编辑器控件的列模式事件
        EditorTextBox.ColumnModeTextInput += OnColumnModeTextInput;
        EditorTextBox.ColumnModeExited += OnColumnModeExited;

        // 延迟查找 ScrollViewer 并绑定滚动事件
        Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        // 查找编辑器内部的 ScrollViewer
        _editorScrollViewer = FindVisualChild<ScrollViewer>(EditorTextBox);
        if (_editorScrollViewer != null)
        {
            _editorScrollViewer.ScrollChanged += OnEditorScrollChanged;
        }
    }

    private void OnEditorScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // 同步行号面板的垂直偏移
        if (LineNumberPanel != null && _editorScrollViewer != null)
        {
            LineNumberPanel.VerticalOffset = _editorScrollViewer.VerticalOffset;
        }
    }

    private void OnColumnModeTextInput(string text)
    {
        if (_viewModel?.ActiveEditor == null)
            return;

        var editor = _viewModel.ActiveEditor;
        if (!editor.IsColumnMode || editor.ColumnSelection.IsEmpty)
            return;

        if (string.IsNullOrEmpty(text))
        {
            // 删除操作
            editor.ColumnDeleteCommand.Execute(null);
        }
        else
        {
            // 插入操作
            editor.ColumnInsertCommand.Execute(text);
        }
    }

    private void OnColumnModeExited()
    {
        if (_viewModel?.ActiveEditor == null)
            return;

        var editor = _viewModel.ActiveEditor;
        editor.IsColumnMode = false;
        _viewModel.StatusBar.ColumnMode = string.Empty;
        _viewModel.StatusBar.IsColumnModeActive = false;
        _viewModel.StatusBar.Status = "列模式已关闭";
    }

    /// <summary>
    /// 在可视化树中查找指定类型的子元素
    /// </summary>
    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;

            var found = FindVisualChild<T>(child);
            if (found != null)
                return found;
        }
        return null;
    }
}
