using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer.Gecco;

namespace Italbytz.EA.Trainer;

public class
    LogicGpFlrwMicroMulticlassTrainer<TOutput> : LogicGpMulticlassTrainer<
    TOutput>
    where TOutput : class, new()
{
    public LogicGpFlrwMicroMulticlassTrainer(int generations)
    {
        ConfusionAndSizeFitnessValue.UsedMetric = Metric.MicroAccuracy;
        RunStrategy = new FlrwRunStrategy(generations);
    }
}