using System.IO;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfTextureCoordinatesStruct
    {
        public float U { get; set; }
        public float V { get; set; }
        public float W { get; set; }

        public static BgfTextureCoordinatesStruct FromBytes(BinaryReader br)
        {
            var u = br.ReadSingle();
            var v = br.ReadSingle();
            var w = br.ReadSingle();

            return new BgfTextureCoordinatesStruct
            {
                U = u,
                V = v,
                W = w
            };
        }
    }
}