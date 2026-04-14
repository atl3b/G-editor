using GEditor.Core.Selection;
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

    /// <summary>
    /// 是否启用列模式
    /// </summary>
    public static readonly DependencyProperty IsColumnModeEnabledProperty =
        DependencyProperty.Register(
            nameof(IsColumnModeEnabled),
            typeof(bool),
            typeof(SyntaxHighlightingTextBox),
            new PropertyMetadata(false, OnIsColumnModeEnabledChanged));

    /// <summary>
    /// 当前列选区
    /// </summary>
    public static readonly DependencyProperty ColumnSelectionProperty =
        DependencyProperty.Register(
            nameof(ColumnSelection),
            typeof(ColumnSelection),
            typeof(SyntaxHighlightingTextBox),
            new PropertyMetadata(ColumnSelection.Empty, OnColumnSelectionChanged));

    /// <summary>
    /// 是否启用自动换行
    /// </summary>
    public static readonly DependencyProperty IsWordWrapEnabledProperty =
        DependencyProperty.Register(
            nameof(IsWordWrapEnabled),
            typeof(bool),
            typeof(SyntaxHighlightingTextBox),
            new PropertyMetadata(false, OnIsWordWrapEnabledChanged));

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

    public bool IsColumnModeEnabled
    {
        get => (bool)GetValue(IsColumnModeEnabledProperty);
        set => SetValue(IsColumnModeEnabledProperty, value);
    }

    public ColumnSelection ColumnSelection
    {
        get => (ColumnSelection)GetValue(ColumnSelectionProperty);
        set => SetValue(ColumnSelectionProperty, value);
    }

    public bool IsWordWrapEnabled
    {
        get => (bool)GetValue(IsWordWrapEnabledProperty);
        set => SetValue(IsWordWrapEnabledProperty, value);
    }

    #endregion

    #region 列模式字段

    private ColumnSelectionAdorner? _columnSelectionAdorner;
    private Point _columnSelectionStart;
    private bool _isColumnSelecting;
    private int _startLineIndex;
    private int _startColumnIndex;

    /// <summary>
    /// 列模式键盘输入事件 - 在所有选中行同时插入文本时触发
    /// </summary>
    public event Action<string>? ColumnModeTextInput;

    /// <summary>
    /// 列模式退出事件
    /// </summary>
    public event Action? ColumnModeExited;

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

    private static void OnIsColumnModeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyntaxHighlightingTextBox textBox)
        {
            bool isEnabled = (bool)e.NewValue;
            if (isEnabled)
            {
                textBox.EnsureColumnSelectionAdorner();
                textBox.Cursor = Cursors.Cross;
            }
            else
            {
                textBox.ClearColumnSelection();
                textBox.Cursor = Cursors.IBeam;
            }
        }
    }

    private static void OnColumnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyntaxHighlightingTextBox textBox)
        {
            textBox.UpdateColumnSelectionVisual();
        }
    }

    private static void OnIsWordWrapEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyntaxHighlightingTextBox textBox)
        {
            bool isEnabled = (bool)e.NewValue;
            if (!isEnabled)
            {
                // 禁用自动换行：设置一个很大的 PageWidth 使文本不换行
                // 注意：不能用 double.MaxValue，会导致布局计算溢出和 NaN/Infinity 传播
                textBox.Document.PageWidth = 100000.0;
                textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            else
            {
                // 启用自动换行：恢复默认的页面宽度
                textBox.Document.PageWidth = double.NaN; // NaN 表示自适应宽度（自动换行）
                textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            // 重新应用高亮以适应新的宽度
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
        // ESC 退出列模式
        if (e.Key == Key.Escape && IsColumnModeEnabled)
        {
            IsColumnModeEnabled = false;
            ColumnModeExited?.Invoke();
            e.Handled = true;
            return;
        }

        // 处理 Tab 键
        if (e.Key == Key.Tab)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (IsColumnModeEnabled && !ColumnSelection.IsEmpty)
                {
                    // 列模式下 Tab 在所有选中行插入空格
                    ColumnModeTextInput?.Invoke("    ");
                    e.Handled = true;
                    return;
                }
                CaretPosition.InsertTextInRun("    "); // 插入4个空格
                e.Handled = true;
            }
        }

        // 列模式下的键盘输入处理
        if (IsColumnModeEnabled && !ColumnSelection.IsEmpty)
        {
            // Alt+Shift+方向键扩展列选区
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var normalized = ColumnSelection.Normalized();
                bool changed = false;
                int startLine = normalized.StartLine;
                int startCol = normalized.StartColumn;
                int endLine = normalized.EndLine;
                int endCol = normalized.EndColumn;

                if (e.Key == Key.Up)
                {
                    startLine = Math.Max(0, startLine - 1);
                    changed = true;
                }
                else if (e.Key == Key.Down)
                {
                    endLine = Math.Min(Document.Blocks.Count - 1, endLine + 1);
                    changed = true;
                }
                else if (e.Key == Key.Left)
                {
                    startCol = Math.Max(0, startCol - 1);
                    changed = true;
                }
                else if (e.Key == Key.Right)
                {
                    endCol += 1;
                    changed = true;
                }

                if (changed)
                {
                    ColumnSelection = new ColumnSelection(startLine, startCol, endLine, endCol);
                    UpdateColumnSelectionVisual();
                    e.Handled = true;
                    return;
                }
            }

            // 普通文本输入在列选区所有行同时插入
            if (e.Key == Key.Enter)
            {
                ColumnModeTextInput?.Invoke("\n");
                e.Handled = true;
                return;
            }

            // 可打印字符
            if (IsPrintableKey(e.Key) && !Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) && !Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                // 实际字符由 OnPreviewTextInput 处理
            }

            // 删除键
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                ColumnModeTextInput?.Invoke("");
                e.Handled = true;
                return;
            }
        }

        // 处理 Alt 键按下时的列模式
        if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            if (IsColumnModeEnabled)
            {
                Cursor = Cursors.Cross;
            }
        }
    }

    /// <summary>
    /// 处理文本输入事件 - 用于列模式多行同时输入
    /// </summary>
    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        if (IsColumnModeEnabled && !ColumnSelection.IsEmpty && !string.IsNullOrEmpty(e.Text))
        {
            ColumnModeTextInput?.Invoke(e.Text);
            e.Handled = true;
            return;
        }

        base.OnPreviewTextInput(e);
    }

    /// <summary>
    /// 判断按键是否为可打印字符
    /// </summary>
    private static bool IsPrintableKey(Key key)
    {
        return key >= Key.D0 && key <= Key.Z
            || key >= Key.OemTilde && key <= Key.OemBackslash
            || key == Key.Space
            || key == Key.OemMinus
            || key == Key.OemPlus
            || key == Key.OemOpenBrackets
            || key == Key.OemCloseBrackets
            || key == Key.OemPipe
            || key == Key.OemSemicolon
            || key == Key.OemQuotes
            || key == Key.OemComma
            || key == Key.OemPeriod
            || key == Key.OemQuestion
            || key >= Key.NumPad0 && key <= Key.NumPad9;
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        // Alt+鼠标拖动直接进入列选择（无需预启用列模式）
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            // 自动启用列模式
            if (!IsColumnModeEnabled)
                IsColumnModeEnabled = true;

            // 开始列选择
            _isColumnSelecting = true;
            _columnSelectionStart = e.GetPosition(this);

            // 获取起始位置
            var lineCol = GetLineAndColumnFromPoint(_columnSelectionStart);
            _startLineIndex = lineCol.line;
            _startColumnIndex = lineCol.column;

            // 初始化零宽度选区（竖线光标）
            ColumnSelection = new ColumnSelection(_startLineIndex, _startColumnIndex, _startLineIndex, _startColumnIndex);
            UpdateColumnSelectionVisual();

            // 抑制默认选择行为
            e.Handled = true;
            CaptureMouse();
            return;
        }

        // 如果正在列模式且已有选区，点击其他地方退出列模式
        if (IsColumnModeEnabled && !Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            IsColumnModeEnabled = false;
            ColumnModeExited?.Invoke();
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        if (_isColumnSelecting && IsColumnModeEnabled)
        {
            var currentPoint = e.GetPosition(this);
            var lineCol = GetLineAndColumnFromPoint(currentPoint);

            int endLine = lineCol.line;
            int endColumn = lineCol.column;

            // 更新选区
            var selection = new ColumnSelection(
                Math.Min(_startLineIndex, endLine),
                Math.Min(_startColumnIndex, endColumn),
                Math.Max(_startLineIndex, endLine),
                Math.Max(_startColumnIndex, endColumn));

            ColumnSelection = selection;
            UpdateColumnSelectionVisual();
            e.Handled = true;
            return;
        }

        base.OnPreviewMouseMove(e);
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (_isColumnSelecting)
        {
            _isColumnSelecting = false;
            ReleaseMouseCapture();
            e.Handled = true;
            return;
        }

        base.OnPreviewMouseLeftButtonUp(e);
    }

    #endregion

    #region 列选择辅助方法

    /// <summary>
    /// 检查矩形是否有效（非 NaN、非 Infinity、非空）
    /// </summary>
    private static bool IsValidRect(Rect rect)
    {
        return !rect.IsEmpty
            && !double.IsNaN(rect.X) && !double.IsNaN(rect.Y)
            && !double.IsNaN(rect.Width) && !double.IsNaN(rect.Height)
            && !double.IsInfinity(rect.X) && !double.IsInfinity(rect.Y)
            && !double.IsInfinity(rect.Width) && !double.IsInfinity(rect.Height);
    }

    private void EnsureColumnSelectionAdorner()
    {
        if (_columnSelectionAdorner == null)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                _columnSelectionAdorner = new ColumnSelectionAdorner(this);
                adornerLayer.Add(_columnSelectionAdorner);
            }
        }
        else
        {
            _columnSelectionAdorner.IsVisible = true;
        }
    }

    private void UpdateColumnSelectionVisual()
    {
        if (_columnSelectionAdorner == null)
            return;

        if (ColumnSelection.IsEmpty)
        {
            _columnSelectionAdorner.ClearSelection();
            return;
        }

        var rects = GetSelectionRectsFromColumnSelection(ColumnSelection);
        _columnSelectionAdorner.SetSelectionRects(rects);
    }

    private void ClearColumnSelection()
    {
        _columnSelectionAdorner?.ClearSelection();
        ColumnSelection = ColumnSelection.Empty;
    }

    private (int line, int column) GetLineAndColumnFromPoint(Point point)
    {
        // 获取点击位置的文本指针
        var textPointer = GetPositionFromPoint(point, true);
        if (textPointer == null)
            return (0, 0);

        // 计算行号 - 使用 Document.Blocks 索引（逻辑行）而非 GetLineStartPosition（视觉行）
        int lineNumber = 0;
        foreach (var block in Document.Blocks)
        {
            if (block is Paragraph paragraph)
            {
                var contentStart = paragraph.ContentStart;
                var contentEnd = paragraph.ContentEnd;
                if (contentStart != null && contentEnd != null)
                {
                    if (textPointer.CompareTo(contentStart) >= 0 && textPointer.CompareTo(contentEnd) <= 0)
                    {
                        // 找到当前行，计算列号
                        var range = new TextRange(contentStart, textPointer);
                        return (lineNumber, range.Text.Length);
                    }
                }
                lineNumber++;
            }
        }

        // 如果未找到（可能在文档末尾之后），返回最后一行
        return (Math.Max(0, Document.Blocks.Count - 1), 0);
    }

    private Rect GetSelectionRectFromColumnSelection(ColumnSelection selection)
    {
        var normalized = selection.Normalized();
        var rects = GetSelectionRectsFromColumnSelection(selection);

        // 如果只有一个矩形，返回它
        if (rects.Count == 1)
            return rects[0];

        // 合并多个矩形为一个大矩形
        if (rects.Count > 1)
        {
            double minLeft = rects.Min(r => r.Left);
            double maxRight = rects.Max(r => r.Right);
            double top = rects.First().Top;
            double bottom = rects.Last().Bottom;
            return new Rect(minLeft, top, maxRight - minLeft, bottom - top);
        }

        return Rect.Empty;
    }

    /// <summary>
    /// 获取列选区的逐行矩形列表（用于逐行绘制高亮）
    /// </summary>
    private List<Rect> GetSelectionRectsFromColumnSelection(ColumnSelection selection)
    {
        var normalized = selection.Normalized();
        var rects = new List<Rect>();

        // 获取每行的字符矩形
        for (int line = normalized.StartLine; line <= normalized.EndLine; line++)
        {
            if (line < 0 || line >= Document.Blocks.Count)
                continue;

            var paragraph = Document.Blocks.ElementAtOrDefault(line) as Paragraph;
            if (paragraph == null)
                continue;

            // 获取行起始位置（使用 Paragraph.ContentStart 而非 GetLineStartPosition）
            var lineStart = paragraph.ContentStart;
            if (lineStart == null)
                continue;

            // 获取起始列位置
            var startPointer = GetTextPositionAtOffset(lineStart, normalized.StartColumn);
            var endPointer = GetTextPositionAtOffset(lineStart, normalized.EndColumn);

            if (startPointer != null)
            {
                var startRect = startPointer.GetCharacterRect(LogicalDirection.Forward);
                if (!IsValidRect(startRect))
                    continue;

                if (line == normalized.StartLine && line == normalized.EndLine)
                {
                    // 单行选区
                    var endRect = endPointer?.GetCharacterRect(LogicalDirection.Backward) ?? startRect;
                    if (!IsValidRect(endRect))
                        endRect = startRect;
                    var rect = new Rect(startRect.Left, startRect.Top, endRect.Right - startRect.Left, startRect.Height);
                    if (IsValidRect(rect))
                        rects.Add(rect);
                }
                else
                {
                    // 多行选区中的某一行
                    var endRect = endPointer?.GetCharacterRect(LogicalDirection.Backward) ?? new Rect(startRect.Right, startRect.Top, 0, startRect.Height);
                    if (!IsValidRect(endRect))
                        endRect = new Rect(startRect.Right, startRect.Top, 0, startRect.Height);

                    if (line == normalized.StartLine)
                    {
                        var rect = new Rect(startRect.Left, startRect.Top, startRect.Right - startRect.Left, startRect.Height);
                        if (IsValidRect(rect))
                            rects.Add(rect);
                    }
                    else if (line == normalized.EndLine)
                    {
                        var rect = new Rect(startRect.Left, startRect.Top, endRect.Right - startRect.Left, startRect.Height);
                        if (IsValidRect(rect))
                            rects.Add(rect);
                    }
                    else
                    {
                        // 中间行 - 延伸到行末
                        var lineEndRect = paragraph.ContentEnd.GetCharacterRect(LogicalDirection.Backward);
                        double right = IsValidRect(lineEndRect) ? Math.Max(lineEndRect.Right, startRect.Right) : startRect.Right;
                        var rect = new Rect(startRect.Left, startRect.Top, right - startRect.Left, startRect.Height);
                        if (IsValidRect(rect))
                            rects.Add(rect);
                    }
                }
            }
        }

        return rects;
    }

    private TextPointer? GetTextPositionAtOffset(TextPointer start, int offset)
    {
        var current = start;
        int count = 0;

        while (current != null && count < offset)
        {
            // 只计算文本字符，跳过元素开始/结束标记
            if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                var textRun = current.GetTextInRun(LogicalDirection.Forward);
                int charsToSkip = Math.Min(textRun.Length, offset - count);
                current = current.GetPositionAtOffset(charsToSkip);
                count += charsToSkip;
            }
            else
            {
                var next = current.GetPositionAtOffset(1);
                if (next == null || next.CompareTo(current) == 0)
                    break;
                current = next;
            }
        }

        return current;
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
