﻿using Europa1400.Tools.Enums;

namespace Europa1400.Tools.Decoder.Sbf;

internal class SoundDefinitionStruct
{
    public required SoundType SoundType { get; init; }

    public required uint Length { get; init; }

    public required uint Magic { get; init; }

    internal static SoundDefinitionStruct FromBytes(BinaryReader br)
    {
        var soundType = (SoundType)br.ReadUInt32();
        var length = br.ReadUInt32();
        var magic = br.ReadUInt32();

        return new SoundDefinitionStruct
        {
            SoundType = soundType,
            Length = length,
            Magic = magic
        };
    }
}
