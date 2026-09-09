using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class GlobAssetDiscoverer<TAsset> : IAssetDiscoverer<TAsset> where TAsset : GameAsset
    {
        private readonly Func<string, string, TAsset> _create;
        private readonly string _displayName;
        private readonly string _searchPattern;

        public GlobAssetDiscoverer(string searchPattern, string displayName, Func<string, string, TAsset> create)
        {
            _searchPattern = searchPattern;
            _displayName = displayName;
            _create = create;
        }

        public IEnumerable<TAsset> DiscoverAllFromGame(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return Array.Empty<TAsset>();

            return Directory.GetFiles(gamePath, _searchPattern, SearchOption.AllDirectories)
                .Select(filePath => _create(
                    filePath,
                    Path.GetRelativePath(gamePath, filePath).Replace('\\', '/')));
        }

        public TAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"{_displayName} file not found", filePath);

            return _create(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}
