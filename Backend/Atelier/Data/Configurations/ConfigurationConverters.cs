using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace Atelier.Data.Configurations;

internal static class ConfigurationConverters
{
    internal const string IsoFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    internal static readonly ValueConverter<DateTimeOffset, string> IsoUtcConverter = new(
        v => v.ToUniversalTime().ToString(IsoFormat, CultureInfo.InvariantCulture),
        v => DateTimeOffset.Parse(
                 v,
                 CultureInfo.InvariantCulture,
                 DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal));

    internal static ValueConverter<TEnum, string> LowerCaseEnum<TEnum>() where TEnum : struct, Enum
        => new(
            v => v.ToString()!.ToLowerInvariant(),
            v => Enum.Parse<TEnum>(v, ignoreCase: true));

}
