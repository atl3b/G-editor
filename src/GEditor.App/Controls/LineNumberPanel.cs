using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GEditor.App.Controls;

/// <summary>
/// 行号面板控件 - 在编辑区左侧显示行号
/// Premium Edition v2.0:
///   - 深色背景 + 柔和青灰色数字
///   - 选中行高亮 (Cyan 微光)
///   - 渐变分隔线
///   - 奇偶行交替背景（极淡）
///   - 可视区域优化渲染
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
    /// 垂直偏移（跟随 ScrollViewer 滚动同步）
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
    /// 当前活动行（光标所在行，0-based，用于高亮显示）
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

    #region 私有字段 - Premium Dark 配色方案

    // ── 配色常量（与 App.xaml 设计令牌系统一致）──
    
    // 行号默认颜色：冷灰蓝色，类似 VS Code 行号风格
    private static readonly Color LineNumDefaultColor = Color.FromRgb(68, 78, 95);       // #444E5F
    // 选中行号颜色：更亮的蓝灰色，带微妙强调感
    private static readonly Color LineNumActiveColor = Color.FromRgb(110, 125, 145);     // #6E7D91
    // 背景色：比编辑区略深，创造层次感
    private static readonly Color BgColor = Color.FromRgb(13, 17, 23);                   // #0D1117
    // 分隔线颜色：微妙的深色渐变基础色
    private static readonly Color SeparatorColor = Color.FromRgb(35, 43, 55);           // #232B37
    // 当前行高亮背景色：半透明 Cyan（与主色调统一）
    private static readonly Color ActiveLineBgColor = Color.FromArgb(28, 0, 212, 170);   // #00D4AA @ ~11%
    // 奇数行交替背景（极淡，增加视觉节奏）
    private static readonly Color AltLineBgColor = Color.FromArgb(6, 255, 255, 255);

    // ── 画刷实例 ──
    private static readonly Brush _lineNumBrush = new SolidColorBrush(LineNumDefaultColor);
    private static readonly Brush _lineNumActiveBrush = new SolidColorBrush(LineNumActiveColor);
    private static readonly Brush _backgroundBrush = new SolidColorBrush(BgColor);
    private static readonly Brush _activeLineBgBrush = new SolidColorBrush(ActiveLineBgColor);
    private static readonly Brush _altLineBgBrush = new SolidColorBrush(AltLineBgColor);

    // 字体：等宽 Geek 风格（Consolas）
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

    /// <summary>
    /// 根据总行数计算面板宽度（自适应位数）
    /// </summary>
    private void UpdateWidth()
    {
        int digits = Math.Max(1, (int)Math.Floor(Math.Log10(Math.Max(1, LineCount))) + 1);
        _cachedWidth = digits * 9.5 + 24.0; // 每位约9.5px + padding (左右各12)
        Width = _cachedWidth;
    }

    #endregion

    #region 渲染核心 - OnRender

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (!IsLineNumberVisible || RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        // ═══ 绘制背景 ═══
        drawingContext.DrawRectangle(_backgroundBrush, null, 
            new Rect(0, 0, RenderSize.Width, RenderSize.Height));

        // ═══ 绘制右侧分隔线（渐变淡入淡出效果）═══
        double separatorX = RenderSize.Width - 1.5;
        
        // 使用 LinearGradientBrush 创建分隔线的渐变效果（两端淡出）
        var separatorGradientPen = new Pen(
            new LinearGradientBrush
            {
                StartPoint = new Point(separatorX, 0),
                EndPoint = new Point(separatorX, RenderSize.Height),
                GradientStops = new GradientStopCollection
                {
                    new(Color.FromArgb(0x00, 0x23, 0x2B, 0x37), 0.0),      // 顶部透明
                    new(SeparatorColor, 0.12),                               // 12% 处出现
                    new(SeparatorColor, 0.88),                               // 中间保持
                    new(Color.FromArgb(0x00, 0x23, 0x2B, 0x37), 1.0),      // 底部透明
                }
            }, 1);
        
        drawingContext.DrawLine(separatorGradientPen,
            new Point(separatorX, 0),
            new Point(separatorX, RenderSize.Height));
        
        // 释放画笔引用的渐变画刷
        separatorGradientPen.Brush = null;

        // ═══ 计算可见区域行号范围（性能优化：只渲染可见行）═══
        int firstVisibleLine = Math.Max(0, (int)(VerticalOffset / LineHeight));
        int lastVisibleLine = Math.Min(LineCount - 1, 
            firstVisibleLine + (int)(RenderSize.Height / LineHeight) + 2);
        double dpiScale = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        // ═══ 逐行渲染行号 ═══
        for (int i = firstVisibleLine; i <= lastVisibleLine; i++)
        {
            double y = i * LineHeight - VerticalOffset;

            // 视口裁剪：跳过完全不可见的行
            if (y + LineHeight < -10 || y > RenderSize.Height + 10)
                continue;

            bool isActiveLine = (i == CurrentLine);

            // 偶数行交替背景（极淡的白色条纹）
            if (!isActiveLine && i % 2 == 1)
            {
                drawingContext.DrawRectangle(_altLineBgBrush, null,
                    new Rect(0, y, RenderSize.Width, LineHeight));
            }

            // 当前行高亮背景 - Cyan 微光效果
            if (isActiveLine)
            {
                drawingContext.DrawRectangle(_activeLineBgBrush, null,
                    new Rect(0, y, RenderSize.Width, LineHeight));
                
                // 当前行的左侧竖条指示器（额外的视觉提示）
                var activeIndicatorPen = new Pen(
                    new SolidColorBrush(Color.FromArgb(180, 0, 212, 170)), 2.0);
                drawingContext.DrawLine(activeIndicatorPen,
                    new Point(0, y),
                    new Point(0, y + LineHeight));
            }

            // ═══ 绘制行号文字 ═══
            string lineNumber = (i + 1).ToString();
            
            var formattedText = new FormattedText(
                lineNumber,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                11.5, // 稍小字号，Geek 精致感
                isActiveLine ? _lineNumActiveBrush : _lineNumBrush,
                dpiScale);

            // 右对齐绘制，留出适当右边距
            double x = RenderSize.Width - 15 - formattedText.Width;
            double textY = y + (LineHeight - formattedText.Height) / 2.0;

            // 渲染行号文字
            drawingContext.DrawText(formattedText, new Point(x, textY));
        }
    }

    #endregion
}
