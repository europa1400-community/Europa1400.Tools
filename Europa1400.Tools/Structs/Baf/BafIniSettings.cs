using System;
using System.Linq;

namespace Europa1400.Tools.Structs.Baf;

public class BafIniSettings
{
    public int NumKeys { get; set; }
    public int[] Keys { get; set; }
    public int LoopIn { get; set; }
    public int LoopOut { get; set; }

    public static BafIniSettings FromText(string text)
    {
        var numKeys = 0;
        var keys = Array.Empty<int>();
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
}
