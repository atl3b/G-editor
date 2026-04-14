using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace GEditor.App.Controls;

/// <summary>
/// 矩形选区装饰器 - 在 RichTextBox 上绘制半透明矩形选区
/// Geek / Cyber 风格：
///   - Cyan 半透明填充（与主色调统一）
///   - 发光边框效果
///   - 零宽度选区显示为发光竖线光标
/// </summary>
public class ColumnSelectionAdorner : Adorner
{
    private List<Rect> _selectionRects = new();
    private bool _isVisible;

    // ── Cyber Cyan 配色方案 ──
    
    /// <summary>
    /// 选区填充色：半透明 Cyan，与 Geek 主题 AccentPrimary 一致
    /// </summary>
    private static readonly Brush _fillBrush = new SolidColorBrush(Color.FromArgb(60, 0, 212, 170)); // #00D4AA @ 24% opacity

    /// <summary>
    /// 边框颜色：亮 Cyan，带视觉冲击力
    /// </summary>
    private static readonly Color _borderColor = Color.FromRgb(0, 212, 170); // #00D4AA

    /// <summary>
    /// 边框笔：1px 实线
    /// </summary>
    private static readonly Pen _borderPen = new Pen(new SolidColorBrush(_borderColor), 1.0);

    /// <summary>
    /// 发光边框笔：外发光效果（通过半透明粗线模拟）
    /// </summary>
    private static readonly Pen _glowPen = new Pen(new SolidColorBrush(Color.FromArgb(40, 0, 212, 170)), 3.5);

    /// <summary>
    /// 零宽度选区（竖线光标）笔：Cyan 发光线条
    /// </summary>
    private static readonly Pen _cursorPen = new Pen(new SolidColorBrush(_borderColor), 1.8);

    /// <summary>
    /// 光标光晕笔：模拟 CRT/终端光标的微弱辉光
    /// </summary>
    private static readonly Pen _cursorGlowPen = new Pen(new SolidColorBrush(Color.FromArgb(50, 0, 212, 170)), 4.0);

    public ColumnSelectionAdorner(UIElement adornedElement) : base(adornedElement)
    {
        IsHitTestVisible = false;

        // 设置笔触为虚线风格（可选：让边框更具科技感）
        // _borderPen.DashStyle = DashStyles.Dot;
    }

    /// <summary>
    /// 设置选区矩形（兼容旧接口，单个矩形）
    /// </summary>
    public void SetSelection(Rect rect)
    {
        if (IsValidRect(rect))
        {
            _selectionRects = new List<Rect> { rect };
            _isVisible = rect.Width > 0 || rect.Height > 0;
        }
        else
        {
            _selectionRects.Clear();
            _isVisible = false;
        }
        InvalidateVisual();
    }

    /// <summary>
    /// 设置选区矩形列表（逐行绘制）
    /// </summary>
    public void SetSelectionRects(List<Rect> rects)
    {
        _selectionRects = rects?.Where(r => IsValidRect(r)).ToList() ?? new List<Rect>();
        _isVisible = _selectionRects.Count > 0;
        InvalidateVisual();
    }

    /// <summary>
    /// 清除选区
    /// </summary>
    public void ClearSelection()
    {
        _isVisible = false;
        _selectionRects.Clear();
        InvalidateVisual();
    }

    /// <summary>
    /// 是否可见
    /// </summary>
    public new bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                InvalidateVisual();
            }
        }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (!_isVisible || _selectionRects.Count == 0)
            return;

        foreach (var rect in _selectionRects)
        {
            if (!IsValidRect(rect))
                continue;

            if (rect.Width > 1.0)
            {
                // ═══ 正常矩形选区 ═══
                
                // 第1层：外部光晕（Glow effect）
                drawingContext.DrawRectangle(null, _glowPen, rect);

                // 第2层：半透明填充
                drawingContext.DrawRectangle(_fillBrush, null, rect);

                // 第3层：锐利边框
                drawingContext.DrawRectangle(_fillBrush, _borderPen, rect);
            }
            else
            {
                // ═══ 零宽度选区 — 发光竖线光标 ═══
                
                double centerX = rect.Left + rect.Width * 0.5;

                // 光晕层
                drawingContext.DrawLine(_cursorGlowPen,
                    new Point(centerX, rect.Top),
                    new Point(centerX, rect.Bottom));

                // 核心亮线
                drawingContext.DrawLine(_cursorPen,
                    new Point(centerX, rect.Top),
                    new Point(centerX, rect.Bottom));
            }
        }
    }

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
}
