using System.ComponentModel;
using System.Globalization;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Database.Converters;

public class UserIdToStringTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        Console.WriteLine($"Converting from string: {value}");
        if (value is string s && Guid.TryParse(s, out var guid))
        {
            return new UserId(guid);
        }

        throw new NotSupportedException();
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
        Type destinationType)
    {
        if (value is Guid userId && destinationType == typeof(string))
            return userId.ToString();

        return base.ConvertTo(context, culture, value, destinationType);
    }
}