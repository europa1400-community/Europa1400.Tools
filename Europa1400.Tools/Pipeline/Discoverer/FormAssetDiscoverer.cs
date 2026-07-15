using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class FormAssetDiscoverer : IAssetDiscoverer<FormAsset>
    {
        public IEnumerable<FormAsset> DiscoverAllFromGame(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return Array.Empty<FormAsset>();

            return Directory.GetFiles(gamePath, "*.form", SearchOption.AllDirectories)
                .Select(filePath => new FormAsset(
                    filePath,
                    Path.GetRelativePath(gamePath, filePath).Replace('\\', '/')
                ));
        }

        public FormAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("FORM file not found", filePath);

            return new FormAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}
