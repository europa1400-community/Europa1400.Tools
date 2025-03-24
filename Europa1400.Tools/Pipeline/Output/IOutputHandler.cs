using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output;

public interface IOutputHandler<in TOutput>
{
    void Write(TOutput output, IGameAsset asset);
}