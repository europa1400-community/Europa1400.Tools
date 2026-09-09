using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Baf;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class BafIniDecoder : IDecoder
    {
        public Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default)
        {
            var text = File.ReadAllText(asset.FilePath, Encoding.Latin1);
            return Task.FromResult<object>(BafIniSettings.FromText(text));
        }
    }
}
