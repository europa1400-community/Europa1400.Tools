using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Aobj
{
    public class AobjStruct
    {
        public AobjObjectStruct[] Objects { get; private set; }

        public static AobjStruct FromBytes(BinaryReader br)
        {
            var objects = br.ReadArray(AobjObjectStruct.FromBytes, 732);

            return new AobjStruct
            {
                Objects = objects
            };
        }
    }
}