using System.Linq;
using System.Text.RegularExpressions;

namespace Europa1400.Tools.Decoder.Baf;

internal partial class BafIniSettings
{
    internal int NumKeys { get; set; }
    internal int[] Keys { get; set; }
    internal int LoopIn { get; set; }
    internal int LoopOut { get; set; }

    internal static BafIniSettings FromText(string text)
    {
        var numKeys = 0;
        var keys = System.Array.Empty<int>();
        var loopIn = -1;
        var loopOut = -1;

        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("NumKeys="))
                int.TryParse(trimmed["NumKeys=".Length..], out numKeys);
            else if (trimmed.StartsWith("Keys="))
                keys = trimmed["Keys=".Length..].Split(',').Select(int.Parse).ToArray();
            else if (trimmed.StartsWith("LoopIn="))
                int.TryParse(trimmed["LoopIn=".Length..], out loopIn);
            else if (trimmed.StartsWith("LoopOut="))
                int.TryParse(trimmed["LoopOut=".Length..], out loopOut);
        }

        return new BafIniSettings
        {
            NumKeys = numKeys,
            Keys = keys,
            LoopIn = loopIn,
            LoopOut = loopOut
        };
    }

    [GeneratedRegex(@"^[^=]+=(.+)$", RegexOptions.Multiline)]
    private static partial Regex IniLineRegex();
}
