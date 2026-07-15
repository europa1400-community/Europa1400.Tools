using System.IO;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline
{
    public static class PathHelper
    {
        public static string GetDecodedCacheRelativePath(GameAsset asset)
        {
            return Path.ChangeExtension(asset.RelativePath, ".json");
        }

        public static string GetDecodedCachePath(GameAsset asset, string typeFolder)
        {
            // Get the relative directory (without the filename)
            var relativeDir = Path.GetDirectoryName(asset.RelativePath) ?? string.Empty;

            // Get the filename but change its extension to .json
            var fileName = Path.ChangeExtension(Path.GetFileName(asset.RelativePath), ".json");

            // Combine them into a safe relative path
            var safePath = Path.Combine(relativeDir, fileName).Replace('\\', '/');

            return Path.Combine(PipelineSettings.CacheRoot, "decoded", typeFolder, safePath);
        }

        public static string GetDecodedCacheBasePath(GameAsset asset)
        {
            var typeFolder = asset.GetType().Name;

            return Path.Combine(PipelineSettings.CacheRoot, "decoded", typeFolder);
        }
    }
}