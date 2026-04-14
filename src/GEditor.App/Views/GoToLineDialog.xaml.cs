using System.Windows;

namespace GEditor.App.Views;

/// <summary>
/// 跳转到行号对话框
/// </summary>
public partial class GoToLineDialog : Window
{
    public int TotalLines
    {
        get => (int)GetValue(TotalLinesProperty);
        set => SetValue(TotalLinesProperty, value);
    }

    public static readonly DependencyProperty TotalLinesProperty =
        DependencyProperty.Register(
            nameof(TotalLines),
            typeof(int),
            typeof(GoToLineDialog),
            new PropertyMetadata(1, OnTotalLinesChanged));

    public int TargetLine { get; private set; }

    public GoToLineDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private static void OnTotalLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GoToLineDialog dialog && e.NewValue is int totalLines)
        {
            dialog.MaxLineText.Text = $" - {totalLines}";
            dialog.LineNumberTextBox.Text = "1";
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LineNumberTextBox.Text = "1";
        LineNumberTextBox.SelectAll();
        LineNumberTextBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(LineNumberTextBox.Text, out int line) && line >= 1 && line <= TotalLines)
        {
            TargetLine = line;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show(
                $"请输入有效的行号 (1 - {TotalLines})",
                "无效输入",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            LineNumberTextBox.SelectAll();
            LineNumberTextBox.Focus();
        }
    }
}
