using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer.Bioinformatics;

namespace Italbytz.EA.Trainer;

public class LogicGpRlcwMulticlassTrainer<TOutput> : LogicGpMulticlassTrainer<
    TOutput> where TOutput : class, new()
{
    public LogicGpRlcwMulticlassTrainer(int phase1Generations,
        int phase2Generations, double minMaxWeight = 0.0)
    {
        ConfusionAndSizeFitnessValue.UsedMetric = Metric.F1Score;
        RunStrategy = new RlcwRunStrategy(phase1Generations, phase2Generations,
            minMaxWeight);
    }
}