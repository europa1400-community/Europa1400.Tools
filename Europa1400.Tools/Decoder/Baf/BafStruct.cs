using System.Collections.Generic;
using System.IO;
using Europa1400.Tools.Extensions;
using Europa1400.Tools.Structs;

namespace Europa1400.Tools.Decoder.Baf;

internal class BafStruct
{
    internal required string Magic { get; init; }
    internal required uint DataSize { get; init; }
    internal required uint BoneCount { get; init; }
    internal required BafFrame[] Frames { get; init; }

    internal static BafStruct FromBytes(BinaryReader br)
    {
        var magic = br.ReadCString();
        br.SkipRequiredByte(0x30);
        var dataSize = br.ReadUInt32();
        br.ReadByte();
        br.ReadUInt16();
        br.SkipRequiredBytes(0xCD, 0xAB, 0x23);
        br.ReadUInt32();
        br.SkipRequiredByte(0x33);
        br.ReadByte();
        br.SkipRequiredBytes(0x24, 0x00);

        if (br.PeekByte() == 0x37)
        {
            br.ReadByte();
            br.ReadUInt32();
        }

        br.SkipRequiredByte(0x36);
        br.ReadUInt32();
        br.SkipRequiredByte(0x34);
        br.ReadUInt32();

        while (br.PeekByte() != 0x18)
        {
            var marker = br.ReadByte();
            if (marker == 0x29 || marker == 0x2A)
                br.ReadUInt32();
        }

        var boneCount = 0u;

        br.SkipRequiredByte(0x18);
        var firstKeyValue = br.ReadUInt32();
        br.SkipRequiredByte(0x19);
        boneCount = br.ReadUInt32();

        var has21 = br.SkipOptionalByte(0x21);

        var frames = new List<BafFrame>();

        {
            var positions = new List<Vector3Struct>();
            for (var j = 0; j < boneCount; j++)
                positions.Add(Vector3Struct.FromBytes(br));
            frames.Add(new BafFrame
            {
                KeyValue = firstKeyValue,
                Positions = positions.ToArray()
            });
        }

        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            br.SkipRequiredByte(0x28);

            // Check for end-of-animation marker (0x2F)
            if (br.PeekByte() == 0x2F)
                break;

            br.SkipRequiredByte(0x18);
            var keyValue = br.ReadUInt32();
            br.SkipRequiredByte(0x19);
            var count = br.ReadUInt32();
            br.SkipOptionalByte(0x21);

            var positions = new List<Vector3Struct>();
            for (var j = 0; j < count; j++)
                positions.Add(Vector3Struct.FromBytes(br));

            frames.Add(new BafFrame
            {
                KeyValue = keyValue,
                Positions = positions.ToArray()
            });
        }

        return new BafStruct
        {
            Magic = magic,
            DataSize = dataSize,
            BoneCount = boneCount,
            Frames = frames.ToArray()
        };
    }
}

internal class BafFrame
{
    internal required uint KeyValue { get; init; }
    internal required Vector3Struct[] Positions { get; init; }
}
