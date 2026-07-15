using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Scr;

internal class ScrStruct
{
    internal required byte[] RawData { get; init; }
    internal required List<(byte X, byte Y, byte Z)> Triplets { get; init; }

    internal static ScrStruct FromBytes(BinaryReader br)
    {
        var rawData = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
        var triplets = new List<(byte X, byte Y, byte Z)>();

        for (var i = 0; i + 2 < rawData.Length; i += 3)
            triplets.Add((rawData[i], rawData[i + 1], rawData[i + 2]));

        return new ScrStruct
        {
            RawData = rawData,
            Triplets = triplets
        };
    }
}
