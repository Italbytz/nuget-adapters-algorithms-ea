using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer.Gecco;

namespace Italbytz.EA.Trainer;

public class LogicGpGpasBinaryTrainer : LogicGpBinaryTrainer
{
    public LogicGpGpasBinaryTrainer(int generations)
    {
        ConfusionAndSizeFitnessValue.UsedMetric = Metric.MicroAccuracy;
        RunStrategy = new GPASRunStrategy(generations);
    }
}