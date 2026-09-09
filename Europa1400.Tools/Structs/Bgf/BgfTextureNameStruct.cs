using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfTextureNameStruct
    {
        public string Name { get; set; } = string.Empty;
        public string SecondaryName { get; set; } = string.Empty;
        public string NameAppendix { get; set; } = string.Empty;
        public string ResolvedName => string.IsNullOrEmpty(Name) ? SecondaryName : Name;
        public byte[] Metadata { get; set; } = [];

        public static BgfTextureNameStruct FromBytes(BinaryReader br)
        {
            var name = br.ReadCString();
            var secondaryName = br.ReadCString();
            var nameAppendix = br.ReadCString();
            // Fixed-width binary data can contain printable bytes, including '/'.
            var metadata = br.ReadBytes(6);
            if (metadata.Length != 6)
                throw new EndOfStreamException("Incomplete BGF texture metadata.");

            return new BgfTextureNameStruct
            {
                Name = name,
                SecondaryName = secondaryName,
                NameAppendix = nameAppendix,
                Metadata = metadata
            };
        }
    }
}
