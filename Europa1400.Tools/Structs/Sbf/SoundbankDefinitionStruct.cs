using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Sbf
{
    public class SoundbankDefinitionStruct
    {
        public uint Address { get; private set; }
        public string Name { get; private set; }
        public SoundbankType SoundbankType { get; private set; }
        public byte[] Padding { get; private set; }

        public static SoundbankDefinitionStruct FromBytes(BinaryReader br)
        {
            var address = br.ReadUInt32();
            var name = br.ReadPaddedString(50);
            var soundbankType = (SoundbankType)br.ReadUInt16();
            var padding = br.ReadBytes(8);

            return new SoundbankDefinitionStruct
            {
                Address = address,
                Name = name,
                Padding = padding,
                SoundbankType = soundbankType
            };
        }
    }
}