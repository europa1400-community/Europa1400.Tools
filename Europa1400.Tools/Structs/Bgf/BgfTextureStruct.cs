using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfTextureStruct
    {
        public uint Id { get; private set; }
        public string Name { get; private set; }
        public string? NameAppendix { get; private set; }
        public byte? Unknown1 { get; private set; }
        public byte? Unknown2 { get; private set; }

        public static BgfTextureStruct FromBytes(BinaryReader br)
        {
            br.SkipRequiredBytes(0x05, 0x06);
            var id = br.ReadUInt32();
            br.SkipOptionalByte(0x07);
            br.SkipOptionalByte(0x08);
            var name = br.ReadCString();
            var wasSkipped1 = br.SkipOptionalByte(0x08);
            var wasSkipped2 = br.SkipOptionalByte(0x09);
            var nameAppendix = wasSkipped1 || wasSkipped2 ? br.ReadCString() : null;
            var wasSkipped3 = br.SkipOptionalByte(0x0A);
            var unknown1 = wasSkipped3 ? br.ReadByte() as byte? : null;
            var wasSkipped4 = br.SkipOptionalByte(0x0B);
            var unknown2 = wasSkipped4 ? br.ReadByte() as byte? : null;
            br.SkipUntilInclusive(0x28);

            return new BgfTextureStruct
            {
                Id = id,
                Name = name,
                NameAppendix = nameAppendix,
                Unknown1 = unknown1,
                Unknown2 = unknown2
            };
        }
    }
}