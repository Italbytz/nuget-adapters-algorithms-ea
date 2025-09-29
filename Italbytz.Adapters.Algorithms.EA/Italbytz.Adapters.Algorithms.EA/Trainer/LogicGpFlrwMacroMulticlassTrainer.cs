using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer.Gecco;

namespace Italbytz.EA.Trainer;

public class
    LogicGpFlrwMacroMulticlassTrainer<TOutput> : LogicGpMulticlassTrainer<
    TOutput> where TOutput : class, new()
{
    public LogicGpFlrwMacroMulticlassTrainer(int generations)
    {
        ConfusionAndSizeFitnessValue.UsedMetric = Metric.MacroAccuracy;
        RunStrategy = new FlrwRunStrategy(generations);
    }
}