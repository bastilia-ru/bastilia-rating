namespace Bastilia.Rating.Domain.Common;

internal static class IdentificationParseHelper
{

    internal static int? TryParse1(ReadOnlySpan<char> value, IFormatProvider? provider, params ReadOnlySpan<string> prefixes)
    {
        ReadOnlySpan<char> val = RemovePrefixes(value, prefixes);

        if (int.TryParse(val, provider, out var id) && id > 0)
        {
            return id;
        }

        return null;
    }

    internal static ReadOnlySpan<char> RemovePrefixes(ReadOnlySpan<char> value, ReadOnlySpan<string> prefixes)
    {
        var val = value.Trim();
        foreach (var prefix in prefixes)
        {
            if (val.StartsWith(prefix))
            {
                val = val[prefix.Length..];
            }
        }
        if (val.StartsWith("("))
        {
            val = val[1..];
        }
        if (val.EndsWith(")"))
        {
            val = val[..^1];
        }

        return val;
    }

    internal static int SplitIdentifier(ReadOnlySpan<char> val, Span<Range> ranges) => val.SplitAny(ranges, "-,", StringSplitOptions.TrimEntries);
}
