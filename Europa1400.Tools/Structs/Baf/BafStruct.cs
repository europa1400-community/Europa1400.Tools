using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Extensions;
using Europa1400.Tools.Structs.Bgf;

namespace Europa1400.Tools.Structs.Baf;

public class BafStruct
{
    public required string Magic { get; init; }
    public required uint DataSize { get; init; }
    // Kept for source compatibility. These are mesh vertices, not bones.
    public required uint BoneCount { get; init; }
    public uint VertexCount => BoneCount;
    public uint ObjectCount { get; init; } = 1;
    public uint AttachmentCount { get; init; }
    public int LoopIn { get; init; } = -1;
    public int LoopOut { get; init; } = -1;
    public required BafFrame[] Frames { get; init; }

    internal static BafHeader ReadHeader(BinaryReader br)
    {
        var magic = br.ReadCString();
        if (magic != "BGF") throw new InvalidDataException("Invalid BAF magic.");
        br.SkipRequiredByte(0x30);
        var size = br.ReadUInt32();
        br.SkipRequiredBytes(0x01, 0x01, 0x00, 0xCD, 0xAB, 0x23);
        var header = new BafHeader { DataSize = size, FrameCount = br.ReadUInt32() };
        while (br.PeekByte() != 0x18)
        {
            switch (br.ReadByte())
            {
                case 0x33: header.HasVertices = br.ReadByte() != 0; break;
                case 0x24: br.ReadByte(); break;
                case 0x37: header.AttachmentCount = br.ReadUInt32(); break;
                case 0x36: header.ObjectCount = br.ReadUInt32(); break;
                case 0x34: header.VertexCount = br.ReadUInt32(); break;
                case 0x29: header.LoopIn = br.ReadInt32(); break;
                case 0x2A: header.LoopOut = br.ReadInt32(); break;
                default: throw new InvalidDataException($"Unknown BAF header marker at {br.BaseStream.Position - 1:X}.");
            }
        }
        if (header.FrameCount == 0 || header.ObjectCount == 0 ||
            header.FrameCount > br.BaseStream.Length || header.ObjectCount > br.BaseStream.Length)
            throw new InvalidDataException("Invalid BAF frame/object count.");
        return header;
    }

    public static BafStruct FromBytes(BinaryReader br)
    {
        var header = ReadHeader(br);
        var frames = new List<BafFrame>();
        int[]? layout = null;
        for (var f = 0; f < header.FrameCount; f++)
        {
            var positions = new List<Vector3Struct>();
            var counts = new int[checked((int)header.ObjectCount)];
            uint key = 0;
            for (var o = 0; o < counts.Length; o++)
            {
                br.SkipRequiredByte(0x18);
                var objectKey = br.ReadUInt32();
                if (o == 0) key = objectKey;
                else if (objectKey != key) throw new InvalidDataException("BAF objects disagree on frame time.");
                br.SkipRequiredByte(0x19);
                var count = br.ReadUInt32();
                br.SkipRequiredByte(0x21);
                if (count > (br.BaseStream.Length - br.BaseStream.Position) / 12)
                    throw new EndOfStreamException("Incomplete BAF vertex block.");
                counts[o] = checked((int)count);
                for (var v = 0; v < count; v++) positions.Add(Vector3Struct.FromBytes(br));
                br.SkipRequiredByte(0x28);
            }

            Vector3Struct? motionPosition = null, motionRotation = null;
            if (br.PeekByte() == 0x31)
            {
                br.ReadByte();
                motionPosition = Vector3Struct.FromBytes(br);
                motionRotation = Vector3Struct.FromBytes(br);
            }
            var attachments = br.ReadArray(BgfSkeletonStruct.FromBytes, header.AttachmentCount);
            if (layout != null && !layout.SequenceEqual(counts))
                throw new InvalidDataException("BAF vertex layout changes between frames.");
            if (frames.Count > 0 && key <= frames[^1].KeyValue)
                throw new InvalidDataException("BAF key times must increase.");
            layout = counts;
            frames.Add(new BafFrame
            {
                KeyValue = key, Positions = positions.ToArray(), ObjectVertexCounts = counts,
                MotionPosition = motionPosition, MotionRotation = motionRotation, Attachments = attachments
            });
        }
        br.SkipRequiredByte(0x2F);
        var total = (uint)frames[0].Positions.Length;
        if (header.VertexCount != 0 && header.VertexCount != total)
            throw new InvalidDataException("BAF total vertex count disagrees with its objects.");
        return new BafStruct
        {
            Magic = "BGF", DataSize = header.DataSize, BoneCount = total,
            ObjectCount = header.ObjectCount, AttachmentCount = header.AttachmentCount,
            LoopIn = header.LoopIn, LoopOut = header.LoopOut, Frames = frames.ToArray()
        };
    }
}

internal sealed class BafHeader
{
    public uint DataSize { get; set; }
    public uint FrameCount { get; set; }
    public uint ObjectCount { get; set; } = 1;
    public uint VertexCount { get; set; }
    public uint AttachmentCount { get; set; }
    public bool HasVertices { get; set; } = true;
    public int LoopIn { get; set; } = -1;
    public int LoopOut { get; set; } = -1;
}

public class BafFrame
{
    public required uint KeyValue { get; init; }
    public required Vector3Struct[] Positions { get; init; }
    public int[] ObjectVertexCounts { get; init; } = [];
    public Vector3Struct? MotionPosition { get; init; }
    public Vector3Struct? MotionRotation { get; init; }
    public BgfSkeletonStruct[] Attachments { get; init; } = [];
}
