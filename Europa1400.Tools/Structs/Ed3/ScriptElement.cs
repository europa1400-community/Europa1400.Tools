using System.Collections.Generic;
using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ed3
{
    public class Script
    {
        public string EventName { get; set; }
        public string ScriptPath { get; set; }

        public static Script FromBytes(BinaryReader br)
        {
            return new Script
            {
                EventName = br.ReadCString(),
                ScriptPath = br.ReadCString()
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
            for (var i = 0; i < element.ScriptCount; i++)
                element.Scripts.Add(Script.FromBytes(br));

            br.SkipWhile(0);

            return element;
        }
    }
}
