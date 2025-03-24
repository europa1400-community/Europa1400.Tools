using System.IO;

namespace Europa1400.Tools.Structs.Ageb
{
    public class AgebBuildingCoordinatesStruct
    {
        public byte X { get; private set; }

        public byte Y { get; private set; }

        public byte Z { get; private set; }

        public static AgebBuildingCoordinatesStruct FromBytes(BinaryReader br)
        {
            var x = br.ReadByte();
            var y = br.ReadByte();
            var z = br.ReadByte();

            return new AgebBuildingCoordinatesStruct
            {
                X = x,
                Y = y,
                Z = z
            };
        }
    }
}