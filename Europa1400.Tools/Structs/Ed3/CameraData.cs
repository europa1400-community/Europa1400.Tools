using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ed3
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
                Values1 = br.ReadSingles(17).ToList(),
                Nums1 = MainCameraNums.FromBytes(br),
                Values2 = br.ReadSingles(2).ToList(),
                Nums2 = MainCameraNums.FromBytes(br),
                Values3 = br.ReadSingles(2).ToList(),
                Nums3 = MainCameraNums.FromBytes(br)
            };
        }
    }
}
