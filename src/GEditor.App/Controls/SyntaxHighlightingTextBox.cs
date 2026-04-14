using GEditor.Syntax;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace GEditor.App.Controls;

/// <summary>
/// 支持语法高亮的文本编辑器控件
/// </summary>
public class SyntaxHighlightingTextBox : RichTextBox
{
    #region 依赖属性

    /// <summary>
    /// 纯文本内容（用于绑定）
    /// </summary>
    public static readonly DependencyProperty PlainTextProperty =
        DependencyProperty.Register(
            nameof(PlainText),
            typeof(string),
            typeof(SyntaxHighlightingTextBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPlainTextChanged));

    /// <summary>
    /// 当前语法高亮器
    /// </summary>
    public static readonly DependencyProperty SyntaxHighlighterProperty =
        DependencyProperty.Register(
            nameof(SyntaxHighlighter),
            typeof(ISyntaxHighlighter),
            typeof(SyntaxHighlightingTextBox),
            new PropertyMetadata(null, OnSyntaxHighlighterChanged));

    /// <summary>
    /// 文档行列表（用于高亮）
    /// </summary>
    public static readonly DependencyProperty DocumentLinesProperty =
        DependencyProperty.Register(
            nameof(DocumentLines),
            typeof(IReadOnlyList<string>),
            typeof(SyntaxHighlightingTextBox),
            new PropertyMetadata(null, OnDocumentLinesChanged));

    /// <summary>
    /// 行高亮颜色
    /// </summary>
    public static readonly DependencyProperty CurrentLineBackgroundProperty =
        DependencyProperty.Register(
            nameof(CurrentLineBackground),
            typeof(Brush),
            typeof(SyntaxHighlightingTextBox),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(30, 0, 120, 215))));

    #endregion

    #region 属性

    public string PlainText
    {
        get => (string)GetValue(PlainTextProperty);
        set => SetValue(PlainTextProperty, value);
    }

    public ISyntaxHighlighter? SyntaxHighlighter
    {
        get => (ISyntaxHighlighter?)GetValue(SyntaxHighlighterProperty);
        set => SetValue(SyntaxHighlighterProperty, value);
    }

    public IReadOnlyList<string>? DocumentLines
    {
        get => (IReadOnlyList<string>?)GetValue(DocumentLinesProperty);
        set => SetValue(DocumentLinesProperty, value);
    }

    public Brush CurrentLineBackground
    {
        get => (Brush)GetValue(CurrentLineBackgroundProperty);
        set => SetValue(CurrentLineBackgroundProperty, value);
    }

    #endregion

    #region 构造函数

    public SyntaxHighlightingTextBox()
    {
        AcceptsReturn = true;
        AcceptsTab = true;
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        FontFamily = new FontFamily("Consolas");
        FontSize = 14;
        Padding = new Thickness(5);
        BorderThickness = new Thickness(0);
        
        // 默认文档
        Document = new FlowDocument
        {
            FontFamily = FontFamily,
            FontSize = FontSize
        };

        TextChanged += OnInternalTextChanged;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    #endregion

    #region 事件处理

    private static void OnPlainTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyntaxHighlightingTextBox textBox && !textBox._isUpdating)
        {
            textBox.SetText((string)e.NewValue);
        }
    }

    private static void OnSyntaxHighlighterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyntaxHighlightingTextBox textBox)
        {
            textBox.UpdateHighlighting();
        }
    }

    private static void OnDocumentLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyntaxHighlightingTextBox textBox)
        {
            textBox.UpdateHighlighting();
        }
    }

    private bool _isUpdating;

    private void OnInternalTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;
        
        _isUpdating = true;
        try
        {
            PlainText = GetPlainText();
            UpdateHighlighting();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // 处理 Tab 键
        if (e.Key == Key.Tab)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                CaretPosition.InsertTextInRun("    "); // 插入4个空格
                e.Handled = true;
            }
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 刷新语法高亮
    /// </summary>
    public void RefreshHighlighting()
    {
        UpdateHighlighting();
    }

    #endregion

    #region 私有方法

    private void SetText(string text)
    {
        _isUpdating = true;
        try
        {
            Document.Blocks.Clear();
            var paragraph = new Paragraph();
            if (!string.IsNullOrEmpty(text))
            {
                paragraph.Inlines.Add(new Run(text));
            }
            Document.Blocks.Add(paragraph);
            UpdateHighlighting();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private string GetPlainText()
    {
        var textRange = new TextRange(Document.ContentStart, Document.ContentEnd);
        return textRange.Text.TrimEnd('\r', '\n');
    }

    private void UpdateHighlighting()
    {
        if (SyntaxHighlighter == null || DocumentLines == null)
        {
            return;
        }

        _isUpdating = true;
        try
        {
            var highlightResult = SyntaxHighlighter.HighlightDocument(DocumentLines);
            
            // 重建文档，保留光标位置
            var caretLine = GetCaretLine();
            var caretColumn = GetCaretColumn();
            
            Document.Blocks.Clear();
            
            foreach (var lineTokens in highlightResult.LineTokens)
            {
                var paragraph = new Paragraph();
                
                // 为每个 token 创建一个带颜色的 Run
                foreach (var token in lineTokens)
                {
                    var run = new Run(token.Text)
                    {
                        Foreground = GetBrushForTokenKind(token.Kind)
                    };
                    paragraph.Inlines.Add(run);
                }
                
                // 如果行没有 token，添加一个空的 Run
                if (lineTokens.Count == 0)
                {
                    paragraph.Inlines.Add(new Run());
                }
                
                Document.Blocks.Add(paragraph);
            }
            
            // 恢复光标位置
            RestoreCaretPosition(caretLine, caretColumn);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private Brush GetBrushForTokenKind(TokenKind kind)
    {
        return kind switch
        {
            TokenKind.Keyword => new SolidColorBrush(Color.FromRgb(86, 156, 214)),       // 蓝色
            TokenKind.String => new SolidColorBrush(Color.FromRgb(214, 157, 133)),        // 橙色
            TokenKind.Number => new SolidColorBrush(Color.FromRgb(181, 206, 168)),        // 浅绿色
            TokenKind.Comment => new SolidColorBrush(Color.FromRgb(106, 153, 85)),        // 绿色
            TokenKind.Preprocessor => new SolidColorBrush(Color.FromRgb(155, 155, 155)), // 灰色
            TokenKind.Type => new SolidColorBrush(Color.FromRgb(78, 201, 176)),          // 青色
            TokenKind.Attribute => new SolidColorBrush(Color.FromRgb(255, 200, 0)),     // 金色
            TokenKind.Operator => new SolidColorBrush(Color.FromRgb(220, 220, 220)),    // 白色
            TokenKind.Delimiter => new SolidColorBrush(Color.FromRgb(220, 220, 220)),   // 白色
            TokenKind.Identifier => new SolidColorBrush(Colors.White),                   // 白色
            TokenKind.PlainText or TokenKind.None or _ => new SolidColorBrush(Colors.White) // 默认白色
        };
    }

    private int GetCaretLine()
    {
        var line = Document.ContentStart.GetLineStartPosition(0);
        int lineNumber = 0;
        while (line != null && line.CompareTo(CaretPosition) < 0)
        {
            var nextLine = line.GetLineStartPosition(1);
            if (nextLine == null || nextLine.CompareTo(line) == 0)
                break;
            line = nextLine;
            lineNumber++;
        }
        return lineNumber;
    }

    private int GetCaretColumn()
    {
        var lineStart = CaretPosition.GetLineStartPosition(0);
        if (lineStart == null) return 1;
        
        var range = new TextRange(lineStart, CaretPosition);
        return range.Text.Length + 1;
    }

    private void RestoreCaretPosition(int line, int column)
    {
        try
        {
            var pointer = Document.ContentStart;
            int currentLine = 0;
            
            while (currentLine < line && pointer != null)
            {
                var nextLine = pointer.GetLineStartPosition(1);
                if (nextLine == null) break;
                pointer = nextLine;
                currentLine++;
            }
            
            if (pointer != null)
            {
                // 将指针移动到指定列
                for (int i = 0; i < column - 1 && pointer != null; i++)
                {
                    var next = pointer.GetPositionAtOffset(1);
                    if (next == null) break;
                    pointer = next;
                }
                CaretPosition = pointer;
            }
        }
        catch
        {
            // 忽略位置恢复错误
        }
    }

    #endregion
}
