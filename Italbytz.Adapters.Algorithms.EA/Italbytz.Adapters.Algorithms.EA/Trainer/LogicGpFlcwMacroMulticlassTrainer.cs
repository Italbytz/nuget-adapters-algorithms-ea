using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer.Gecco;

namespace Italbytz.EA.Trainer;

public class
    LogicGpFlcwMacroMulticlassTrainer<TOutput> : LogicGpMulticlassTrainer<
    TOutput> where TOutput : class, new()
{
    public LogicGpFlcwMacroMulticlassTrainer(int generations)
    {
        ConfusionAndSizeFitnessValue.UsedMetric = Metric.MacroAccuracy;
        RunStrategy = new FlcwRunStrategy(generations);
    }
}