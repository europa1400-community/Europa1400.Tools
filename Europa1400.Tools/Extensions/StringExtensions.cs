using System.Text.RegularExpressions;

namespace Europa1400.Tools.Extensions;

internal static partial class StringExtensions
{
    internal static string NormalizeName(
        this string value, 
        bool performStripNonAscii = true, 
        bool performLower = true, 
        bool performRemoveSuffix = true)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (performStripNonAscii)
        {
            value = value.StripNonAscii();
        }

        if (performLower)
        {
            value = value.ToLower();
        }

        if (performRemoveSuffix)
        {
            value = Path.GetFileNameWithoutExtension(value);
        }

        return value;
    }
    
    private static string StripNonAscii(this string input)
    {
        return StripNonAsciiRegex().Replace(input, string.Empty);
    }

    [GeneratedRegex(@"[^\x00-\x7F]+")]
    private static partial Regex StripNonAsciiRegex();
}