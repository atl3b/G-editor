using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace GEditor.App.Controls;

/// <summary>
/// 矩形选区装饰器 - 在 RichTextBox 上绘制半透明矩形列选区
/// 
/// Premium Edition v2.0 - Cyber/Tech Aesthetic:
///   ├── 填充：Cyan 半透明（#00D4AA @ 22% opacity）
///   ├── 边框：亮 Cyan 实线 + 外发光层
///   ├── 零宽度选区：CRT 风格发光竖线光标
///   └── 防护：所有 Rect 经过 NaN/Infinity 检查
/// </summary>
public class ColumnSelectionAdorner : Adorner
{
    private List<Rect> _selectionRects = new();
    private bool _isVisible;

    // ══════════════════════ Cyber-Tech 配色方案 ══════════════════════

    /// <summary>
    /// 选区填充色：半透明 Cyan，与主色调 AccentPrimary 统一
    /// </summary>
    private static readonly Brush _fillBrush = new SolidColorBrush(
        Color.FromArgb(56, 0, 212, 170)); // #00D4AA @ ~22% opacity

    /// <summary>
    /// 边框颜色：亮 Cyan，高视觉冲击力
    /// </summary>
    private static readonly Color _borderColor = Color.FromRgb(0, 212, 170); // #00D4AA

    /// <summary>
    /// 边框笔：1px 锐利实线
    /// </summary>
    private static readonly Pen _borderPen = new Pen(
        new SolidColorBrush(_borderColor), 1.0);

    /// <summary>
    /// 发光边框笔：外 Glow 效果（通过半透明粗线模拟）
    /// 模拟 Neon / Cyber 发光效果
    /// </summary>
    private static readonly Pen _glowPen = new Pen(
        new SolidColorBrush(Color.FromArgb(45, 0, 212, 170)), 4.0);

    /// <summary>
    /// 零宽度选区（竖线光标）核心笔：Cyan 亮线
    /// </summary>
    private static readonly Pen _cursorPen = new Pen(
        new SolidColorBrush(_borderColor), 1.8);

    /// <summary>
    /// 光标光晕笔：模拟 CRT 终端 / 老式显示器光标的辉光效果
    /// </summary>
    private static readonly Pen _cursorGlowPen = new Pen(
        new SolidColorBrush(Color.FromArgb(55, 0, 212, 170)), 5.0);
    
    /// <summary>
    /// 光标最外层微弱辉光
    /// </summary>
    private static readonly Pen _cursorOuterGlowPen = new Pen(
        new SolidColorBrush(Color.FromArgb(20, 0, 212, 170)), 10.0);

    public ColumnSelectionAdorner(UIElement adornedElement) : base(adornedElement)
    {
        IsHitTestVisible = false; // 不阻挡鼠标事件
    }

    #region 公共接口

    /// <summary>
    /// 设置选区为单个矩形（兼容旧版接口）
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
    /// 设置选区为矩形列表（逐行绘制模式）
    /// </summary>
    public void SetSelectionRects(List<Rect> rects)
    {
        _selectionRects = rects?.Where(r => IsValidRect(r)).ToList() ?? new List<Rect>();
        _isVisible = _selectionRects.Count > 0;
        InvalidateVisual();
    }

    /// <summary>
    /// 清除所有选区
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

    #endregion

    #region 渲染核心 - OnRender

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (!_isVisible || _selectionRects.Count == 0)
            return;

        foreach (var rect in _selectionRects)
        {
            // 安全检查：跳过无效矩形
            if (!IsValidRect(rect))
                continue;

            if (rect.Width > 1.5)
            {
                // ═════ 正常矩形选区渲染 ═════
                
                // Layer 1: 外部光晕（Glow effect - 模拟霓虹灯效果）
                drawingContext.DrawRectangle(null, _glowPen, rect);

                // Layer 2: 半透明填充（主体颜色）
                drawingContext.DrawRectangle(_fillBrush, null, rect);

                // Layer 3: 锐利边框（核心轮廓）
                drawingContext.DrawRectangle(_fillBrush, _borderPen, rect);
            }
            else
            {
                // ═════ 零宽度选区 — CRT 发光竖线光标 ═════
                
                double centerX = rect.Left + rect.Width * 0.5;

                // 最外层微弱辉光（模拟屏幕散射）
                drawingContext.DrawLine(_cursorOuterGlowPen,
                    new Point(centerX, rect.Top),
                    new Point(centerX, rect.Bottom));

                // 中间层光晕
                drawingContext.DrawLine(_cursorGlowPen,
                    new Point(centerX, rect.Top),
                    new Point(centerX, rect.Bottom));

                // 核心亮线（最高亮度）
                drawingContext.DrawLine(_cursorPen,
                    new Point(centerX, rect.Top),
                    new Point(centerX, rect.Bottom));
            }
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 检查矩形是否有效（非空、非 NaN、非 Infinity）
    /// 这是防御性编程的关键，GetCharacterRect 在边缘情况可能返回无效值
    /// </summary>
    private static bool IsValidRect(Rect rect)
    {
        return !rect.IsEmpty
            && !double.IsNaN(rect.X) && !double.IsNaN(rect.Y)
            && !double.IsNaN(rect.Width) && !double.IsNaN(rect.Height)
            && !double.IsInfinity(rect.X) && !double.IsInfinity(rect.Y)
            && !double.IsInfinity(rect.Width) && !double.IsInfinity(rect.Height);
    }

    #endregion
}
