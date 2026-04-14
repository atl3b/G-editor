using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GEditor.App.Controls;

/// <summary>
/// 行号面板控件 - 在编辑区左侧显示行号
/// Geek 风格：暗色背景 + 柔和青灰色数字 + 选中行高亮 + 微妙分隔线
/// </summary>
public class LineNumberPanel : FrameworkElement
{
    #region 依赖属性

    /// <summary>
    /// 总行数
    /// </summary>
    public static readonly DependencyProperty LineCountProperty =
        DependencyProperty.Register(
            nameof(LineCount),
            typeof(int),
            typeof(LineNumberPanel),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender, OnLineCountChanged));

    /// <summary>
    /// 垂直偏移（跟随 ScrollViewer 滚动）
    /// </summary>
    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.Register(
            nameof(VerticalOffset),
            typeof(double),
            typeof(LineNumberPanel),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// 行高（需要与编辑器行高匹配）
    /// </summary>
    public static readonly DependencyProperty LineHeightProperty =
        DependencyProperty.Register(
            nameof(LineHeight),
            typeof(double),
            typeof(LineNumberPanel),
            new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// 当前活动行（光标所在行，用于高亮显示）
    /// </summary>
    public static readonly DependencyProperty CurrentLineProperty =
        DependencyProperty.Register(
            nameof(CurrentLine),
            typeof(int),
            typeof(LineNumberPanel),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// 是否显示行号
    /// </summary>
    public static readonly DependencyProperty IsLineNumberVisibleProperty =
        DependencyProperty.Register(
            nameof(IsLineNumberVisible),
            typeof(bool),
            typeof(LineNumberPanel),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnIsVisibleChanged));

    #endregion

    #region 属性

    public int LineCount
    {
        get => (int)GetValue(LineCountProperty);
        set => SetValue(LineCountProperty, value);
    }

    public double VerticalOffset
    {
        get => (double)GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    public double LineHeight
    {
        get => (double)GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    /// <summary>
    /// 当前光标所在行（0-based），该行会以高亮样式渲染
    /// </summary>
    public int CurrentLine
    {
        get => (int)GetValue(CurrentLineProperty);
        set => SetValue(CurrentLineProperty, value);
    }

    public bool IsLineNumberVisible
    {
        get => (bool)GetValue(IsLineNumberVisibleProperty);
        set => SetValue(IsLineNumberVisibleProperty, value);
    }

    #endregion

    #region 私有字段 - Geek 配色方案

    // 行号默认颜色：冷灰蓝色，类似 Sublime/VS Code 行号
    private static readonly Color LineNumDefaultColor = Color.FromRgb(74, 85, 104);      // #4A5568
    // 选中行号颜色：更亮的青灰色
    private static readonly Color LineNumActiveColor = Color.FromRgb(113, 128, 150);       // #718096
    // 背景色：与编辑器同系但略深
    private static readonly Color BgColor = Color.FromRgb(20, 25, 32);                     // #141920
    // 分隔线颜色：微妙深色
    private static readonly Color SeparatorColor = Color.FromRgb(40, 48, 58);              // #28303A
    // 当前行高亮背景色：微妙的暖色调
    private static readonly Color ActiveLineBgColor = Color.FromArgb(30, 0, 212, 170);     // 半透明 Cyan
    // 奇偶行交替色（极淡）
    private static readonly Color AltLineBgColor = Color.FromArgb(8, 255, 255, 255);

    private static readonly Brush _lineNumBrush = new SolidColorBrush(LineNumDefaultColor);
    private static readonly Brush _lineNumActiveBrush = new SolidColorBrush(LineNumActiveColor);
    private static readonly Brush _backgroundBrush = new SolidColorBrush(BgColor);
    private static readonly Brush _separatorPenBrush = new SolidColorBrush(SeparatorColor);
    private static readonly Brush _activeLineBgBrush = new SolidColorBrush(ActiveLineBgColor);
    private static readonly Brush _altLineBgBrush = new SolidColorBrush(AltLineBgColor);

    // 字体：等宽 Geek 风格
    private static readonly Typeface _typeface = new Typeface(
        new FontFamily("Consolas"),
        FontStyles.Normal,
        FontWeights.Normal,
        FontStretches.Normal);

    private double _cachedWidth;

    #endregion

    #region 构造函数

    public LineNumberPanel()
    {
        UpdateWidth();
    }

    #endregion

    #region 回调

    private static void OnLineCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LineNumberPanel panel)
        {
            panel.UpdateWidth();
        }
    }

    private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LineNumberPanel panel)
        {
            panel.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    #endregion

    #region 方法

    private void UpdateWidth()
    {
        int digits = Math.Max(1, (int)Math.Floor(Math.Log10(Math.Max(1, LineCount))) + 1);
        _cachedWidth = digits * 9.0 + 22.0; // 每位约9px + padding (左右各11)
        Width = _cachedWidth;
    }

    #endregion

    #region 渲染

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (!IsLineNumberVisible || RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        // ── 绘制背景 ──
        drawingContext.DrawRectangle(_backgroundBrush, null, new Rect(0, 0, RenderSize.Width, RenderSize.Height));

        // ── 绘制右侧分隔线（微妙渐变效果）──
        double separatorX = RenderSize.Width - 1.5;
        var separatorGradientPen = new Pen(
            new LinearGradientBrush
            {
                StartPoint = new Point(separatorX, 0),
                EndPoint = new Point(separatorX, RenderSize.Height),
                GradientStops = new GradientStopCollection
                {
                    new(Color.FromArgb(0x00, 0x28, 0x30, 0x3A), 0.0),
                    new(SeparatorColor, 0.15),
                    new(SeparatorColor, 0.85),
                    new(Color.FromArgb(0x00, 0x28, 0x30, 0x3A), 1.0),
                }
            }, 1);
        drawingContext.DrawLine(separatorGradientPen,
            new Point(separatorX, 0),
            new Point(separatorX, RenderSize.Height));
        separatorGradientPen.Brush = null; // 释放

        // ── 计算可见区域行号范围 ──
        int firstVisibleLine = Math.Max(0, (int)(VerticalOffset / LineHeight));
        int lastVisibleLine = Math.Min(LineCount - 1, firstVisibleLine + (int)(RenderSize.Height / LineHeight) + 2);
        double dpiScale = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        // ── 逐行绘制 ──
        for (int i = firstVisibleLine; i <= lastVisibleLine; i++)
        {
            double y = i * LineHeight - VerticalOffset;

            // 视口裁剪
            if (y + LineHeight < 0 || y > RenderSize.Height)
                continue;

            bool isActiveLine = (i == CurrentLine);

            // 偶数行交替背景（极淡）
            if (!isActiveLine && i % 2 == 1)
            {
                drawingContext.DrawRectangle(_altLineBgBrush, null,
                    new Rect(0, y, RenderSize.Width, LineHeight));
            }

            // 当前行高亮背景
            if (isActiveLine)
            {
                drawingContext.DrawRectangle(_activeLineBgBrush, null,
                    new Rect(0, y, RenderSize.Width, LineHeight));
            }

            // 绘制行号文字
            string lineNumber = (i + 1).ToString();
            var formattedText = new FormattedText(
                lineNumber,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                12,   // 稍小的字号，Geek 精致感
                isActiveLine ? _lineNumActiveBrush : _lineNumBrush,
                dpiScale);

            // 右对齐绘制，留出适当边距
            double x = RenderSize.Width - 14 - formattedText.Width;
            double textY = y + (LineHeight - formattedText.Height) / 2.0;

            // 当前行使用稍粗字重（通过微调位置模拟视觉权重差异）
            drawingContext.DrawText(formattedText, new Point(x, textY));
        }
    }

    #endregion
}
