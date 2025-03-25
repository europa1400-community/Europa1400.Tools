namespace Europa1400.Tools.Pipeline
{
    public class PipelineProgress
    {
        public int TotalSteps { get; set; }
        public int CurrentStep { get; set; }
        public string Message { get; set; }

        public double Percent => TotalSteps == 0 ? 0 : (double)CurrentStep / TotalSteps;
    }
}