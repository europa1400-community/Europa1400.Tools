using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline
{
    public class PipelineProgress
    {
        public int Total { get; set; }
        public int Current { get; set; }
        public GameAsset? Asset { get; set; }
        public string? FileName { get; set; }
        public PipelineStep Step { get; set; } = PipelineStep.None;

        public override string ToString()
        {
            return $"{Step} {Current}/{Total} {Asset?.RelativePath ?? FileName}";
        }
    }
}