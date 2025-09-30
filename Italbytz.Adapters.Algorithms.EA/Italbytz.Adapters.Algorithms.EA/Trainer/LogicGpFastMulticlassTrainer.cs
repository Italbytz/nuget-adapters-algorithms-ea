using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer.Bioinformatics;

namespace Italbytz.EA.Trainer;

public class LogicGpFastMulticlassTrainer<TOutput> : LogicGpMulticlassTrainer<
    TOutput> where TOutput : class, new()
{
    public LogicGpFastMulticlassTrainer(int generations)
    {
        ConfusionAndSizeFitnessValue.UsedMetric = Metric.F1Score;
        RunStrategy = new FastRunStrategy(generations, 1.1);
    }
}