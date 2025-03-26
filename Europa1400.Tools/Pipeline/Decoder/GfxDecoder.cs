using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Gfx;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class GfxDecoder : IDecoder, ISubAssetProvider
    {
        public Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var br = new BinaryReader(stream);
            var result = GfxStruct.FromBytes(br);
            return Task.FromResult<object>(new List<ShapebankDefinitionStruct>(result.ShapebankDefinitions));
        }

        public IEnumerable<GameAsset> GetSubAssets(GameAsset asset, object decoded)
        {
            if (!(asset is GfxAsset))
                throw new InvalidDataException("Asset is not a GfxAsset");

            if (!(decoded is IEnumerable<ShapebankDefinitionStruct> shapebankDefinitions))
                throw new InvalidDataException("Decoded object is not a list of ShapebankDefinitionStruct");

            return shapebankDefinitions.Select(def =>
            {
                var parentDirectory = Path.GetDirectoryName(asset.FilePath);
                var relativeParentDirectory = Path.GetDirectoryName(asset.RelativePath);
                var filePath = Path.Combine(parentDirectory ?? string.Empty, def.Name);
                var relativePath = Path.Combine(relativeParentDirectory ?? string.Empty, def.Name);

                return new GfxAsset(filePath, relativePath);
            });
        }
    }
}