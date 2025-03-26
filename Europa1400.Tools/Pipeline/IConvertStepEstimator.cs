namespace Europa1400.Tools.Pipeline
{
    public interface IConvertStepEstimator<in TInput>
    {
        int EstimateConvertSteps(TInput decoded);
    }
}