using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace GEditor.App.Controls;

/// <summary>
/// 矩形选区装饰器 - 在 RichTextBox 上绘制半透明矩形选区
/// </summary>
public class ColumnSelectionAdorner : Adorner
{
    private Rect _selectionRect;
    private bool _isVisible;
    private readonly Brush _fillBrush;
    private readonly Pen _borderPen;

    public ColumnSelectionAdorner(UIElement adornedElement) : base(adornedElement)
    {
        IsHitTestVisible = false;

        // 半透明蓝色填充
        _fillBrush = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215));

        // 蓝色边框
        _borderPen = new Pen(new SolidColorBrush(Color.FromRgb(0, 120, 215)), 1);
    }

    /// <summary>
    /// 设置选区矩形
    /// </summary>
    public void SetSelection(Rect rect)
    {
        _selectionRect = rect;
        _isVisible = rect.Width > 0 || rect.Height > 0;
        InvalidateVisual();
    }

    /// <summary>
    /// 清除选区
    /// </summary>
    public void ClearSelection()
    {
        _isVisible = false;
        _selectionRect = Rect.Empty;
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
        if (!_isVisible)
            return;

        // 绘制填充矩形
        drawingContext.DrawRectangle(_fillBrush, _borderPen, _selectionRect);
    }
}
