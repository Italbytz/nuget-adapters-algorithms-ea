using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer.Gecco;

namespace Italbytz.EA.Trainer;

public class LogicGpGpasBinaryTrainer : LogicGpBinaryTrainer
{
    public LogicGpGpasBinaryTrainer(int generations)
    {
        ConfusionAndSizeFitnessValue.UsedMetric =
            (ClassMetric.Accuracy, Averaging.Micro);
        RunStrategy = new GPASRunStrategy(generations);
    }
}