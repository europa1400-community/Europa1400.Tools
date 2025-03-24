using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfTextureNameStruct
    {
        public string Name { get; private set; }

        public static BgfTextureNameStruct FromBytes(BinaryReader br)
        {
            var name = br.ReadCString();
            br.SkipNonLatin1();
            br.SkipOptionalByte(0x2F);

            return new BgfTextureNameStruct
            {
                Name = name
            };
        }
    }
}