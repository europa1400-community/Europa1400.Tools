using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class Script
    {
        public string EventName { get; set; }
        public string ScriptPath { get; set; }

        public static Script FromBytes(BinaryReader br)
        {
            return new Script
            {
                EventName = ReadHelper.ReadCString(br),
                ScriptPath = ReadHelper.ReadCString(br)
            };
        }
    }

    public class ScriptElement
    {
        public int ScriptCount { get; set; }
        public List<Script> Scripts { get; set; }

        public static ScriptElement FromBytes(BinaryReader br)
        {
            var element = new ScriptElement
            {
                ScriptCount = br.ReadInt32()
            };

            element.Scripts = new List<Script>(element.ScriptCount);
            for (int i = 0; i < element.ScriptCount; i++)
                element.Scripts.Add(Script.FromBytes(br));

            ReadHelper.SkipZeroes(br);

            return element;
        }
    }
}
