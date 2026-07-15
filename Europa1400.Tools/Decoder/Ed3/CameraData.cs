using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class CameraData
    {
        public List<float> Values1 { get; set; }
        public MainCameraNums Nums1 { get; set; }
        public List<float> Values2 { get; set; }
        public MainCameraNums Nums2 { get; set; }
        public List<float> Values3 { get; set; }
        public MainCameraNums Nums3 { get; set; }

        public static CameraData FromBytes(BinaryReader br)
        {
            return new CameraData
            {
                Values1 = ReadHelper.ReadFloats(br, 17),
                Nums1 = MainCameraNums.FromBytes(br),
                Values2 = ReadHelper.ReadFloats(br, 2),
                Nums2 = MainCameraNums.FromBytes(br),
                Values3 = ReadHelper.ReadFloats(br, 2),
                Nums3 = MainCameraNums.FromBytes(br)
            };
        }
    }
}
