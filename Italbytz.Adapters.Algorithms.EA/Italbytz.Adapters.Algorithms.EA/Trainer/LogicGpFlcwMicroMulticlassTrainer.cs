using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer.Gecco;

namespace Italbytz.EA.Trainer;

public class
    LogicGpFlcwMicroMulticlassTrainer<TOutput> : LogicGpMulticlassTrainer<
    TOutput>
    where TOutput : class, new()
{
    public LogicGpFlcwMicroMulticlassTrainer(int generations)
    {
        ConfusionAndSizeFitnessValue.UsedMetric =
            (ClassMetric.Accuracy, Averaging.Micro);
        RunStrategy = new FlcwRunStrategy(generations);
    }
}