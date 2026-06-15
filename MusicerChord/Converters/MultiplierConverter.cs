using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicerChord.Converters
{
    public class MultiplierConverter : IValueConverter
    {
        // 変換処理 (int -> n倍された double)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                // XAML の ConverterParameter から倍率を取得（未指定ならデフォルト 20）
                double multiplier = 20.0;
                if (parameter != null && double.TryParse(parameter.ToString(), out double p))
                {
                    multiplier = p;
                }

                // 計算結果を返す
                return intValue * multiplier;
            }

            return 0.0;
        }

        // 逆変換 (今回は OneWay で使うため未実装でOK)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}