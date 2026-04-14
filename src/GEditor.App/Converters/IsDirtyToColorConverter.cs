using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GEditor.App.Converters;

/// <summary>
/// IsDirty → 前景色转换器：未保存时橙色/已保存时默认色
/// </summary>
public class IsDirtyToColorConverter : IValueConverter
{
    private static readonly Brush DirtyBrush = new SolidColorBrush(Color.FromRgb(255, 165, 0)); // 橙色
    private static readonly Brush NormalBrush = new SolidColorBrush(Color.FromRgb(212, 212, 212)); // 默认灰白色

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isDirty)
        {
            return isDirty ? DirtyBrush : NormalBrush;
        }
        return NormalBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
