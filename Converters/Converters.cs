using System.Globalization;

namespace VillaBookingMAUI.Converters
{
    /// <summary>
    /// Преобразува bool стойност в цвят (за депозит статус и др.)
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public Color TrueColor { get; set; } = Color.FromArgb("#2ECC71");
        public Color FalseColor { get; set; } = Color.FromArgb("#E74C3C");

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? TrueColor : FalseColor;
            return FalseColor;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Инвертира bool стойност (за IsVisible когато трябва обратното).
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return false;
        }
    }

    /// <summary>
    /// Преобразува HouseId в текстово описание.
    /// </summary>
    public class HouseIdToNameConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int id)
                return id == 1 ? "Къща 1" : "Къща 2";
            return "Неизвестна";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Връща true ако стрингът не е празен (за IsVisible binding).
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Преобразува HouseId (1 или 2) в цвят за цветната лента в списъка.
    /// </summary>
    public class HouseIdToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int id)
                return id == 1 ? Color.FromArgb("#2ECC71") : Color.FromArgb("#F39C12");
            return Color.FromArgb("#7F8C8D");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
