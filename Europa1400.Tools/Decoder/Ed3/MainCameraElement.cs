using System;
using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class MainCameraElement
    {
        public string Name { get; set; }
        public List<float>? Values { get; set; }
        public byte? Num1 { get; set; }
        public int CameraDataCount { get; set; }
        public List<float>? Values1 { get; set; }
        public byte? MainCameraElementNum1 { get; set; }
        public byte? MainCameraElementNum2 { get; set; }
        public byte? MainCameraElementNum3 { get; set; }
        public byte? MainCameraElementNum4 { get; set; }
        public MainCameraNums? Nums1 { get; set; }
        public List<float>? Values2 { get; set; }
        public MainCameraNums? Nums2 { get; set; }
        public List<float>? Values3 { get; set; }
        public MainCameraNums? Nums3 { get; set; }
        public List<float>? Values4 { get; set; }
        public MainCameraNums? Nums4 { get; set; }
        public List<CameraData>? CameraDatas { get; set; }
        public List<float>? Values5 { get; set; }
        public byte? Num5 { get; set; }

        public static MainCameraElement FromBytes(BinaryReader br, byte type)
        {
            var element = new MainCameraElement();
            element.Name = ReadHelper.ReadCString(br);

            if (type == 0xAF)
            {
                element.Values = ReadHelper.ReadFloats(br, 22);
                element.Num1 = br.ReadByte();
            }
            else
            {
                element.CameraDataCount = type == 0xB9 ? 5 : 6;
                element.Values1 = ReadHelper.ReadFloats(br, 7);
                element.MainCameraElementNum1 = br.ReadByte();
                element.MainCameraElementNum2 = br.ReadByte();
                element.MainCameraElementNum3 = br.ReadByte();
                element.MainCameraElementNum4 = br.ReadByte();
                element.Nums1 = MainCameraNums.FromBytes(br);
                element.Values2 = ReadHelper.ReadFloats(br, 8);
                element.Nums2 = MainCameraNums.FromBytes(br);
                element.Values3 = ReadHelper.ReadFloats(br, 2);
                element.Nums3 = MainCameraNums.FromBytes(br);
                element.Values4 = ReadHelper.ReadFloats(br, 2);
                element.Nums4 = MainCameraNums.FromBytes(br);

                element.CameraDatas = new List<CameraData>();
                for (int i = 0; i < element.CameraDataCount; i++)
                    element.CameraDatas.Add(CameraData.FromBytes(br));

                element.Values5 = ReadHelper.ReadFloats(br, 11);
                element.Num5 = br.ReadByte();
            }

            ReadHelper.SkipZeroes(br);

            return element;
        }
    }
}
