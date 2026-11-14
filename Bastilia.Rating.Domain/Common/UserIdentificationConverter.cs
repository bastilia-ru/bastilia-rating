using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Bastilia.Rating.Domain.Common;

public class UserIdentificationConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => value is string s && UserIdentification.TryParse(s, culture, out var id)
            ? id
            : base.ConvertFrom(context, culture, value);
}
