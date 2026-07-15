using System.Collections.Generic;
using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Res;

internal class ResStruct
{
    internal required uint EntryCount { get; init; }
    internal required uint Section1Offset { get; init; }
    internal required uint Section2Offset { get; init; }
    internal required List<string> NameStrings { get; init; }
    internal required List<string> ValueStrings { get; init; }
    internal required byte[] StringDataAreaBytes { get; init; }

    internal static ResStruct FromBytes(BinaryReader br)
    {
        var entryCount = br.ReadUInt32();
        var section1Offset = br.ReadUInt32();
        var section2Offset = br.ReadUInt32();

        var stringDataStart = 12 + entryCount * 4;
        br.BaseStream.Position = stringDataStart;
        var stringDataAreaBytes = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));

        br.BaseStream.Position = section1Offset;
        var nameStrings = new List<string>();
        while (br.BaseStream.Position < section2Offset)
        {
            var s = br.ReadCString();
            if (string.IsNullOrEmpty(s)) break;
            nameStrings.Add(s);
        }

        br.BaseStream.Position = section2Offset;
        var valueStrings = new List<string>();
        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var s = br.ReadCString();
            if (string.IsNullOrEmpty(s)) break;
            valueStrings.Add(s);
        }

        return new ResStruct
        {
            EntryCount = entryCount,
            Section1Offset = section1Offset,
            Section2Offset = section2Offset,
            NameStrings = nameStrings,
            ValueStrings = valueStrings,
            StringDataAreaBytes = stringDataAreaBytes
        };
    }
}
