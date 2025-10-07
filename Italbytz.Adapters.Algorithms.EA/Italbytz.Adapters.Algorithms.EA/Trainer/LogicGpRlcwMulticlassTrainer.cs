using Italbytz.EA.Fitness;
using Italbytz.EA.Graph;
using Italbytz.EA.Trainer.Bioinformatics;
using LogicGpGraph = Italbytz.EA.Trainer.Gecco.LogicGpGraph;

namespace Italbytz.EA.Trainer;

public class LogicGpRlcwMulticlassTrainer<TOutput> : LogicGpMulticlassTrainer<
    TOutput> where TOutput : class, new()
{
    public LogicGpRlcwMulticlassTrainer(int phase1Generations,
        int phase2Generations, int folds = 5, double minMaxWeight = 0.0,
        OperatorGraph? algorithmGraph = null,
        Metric usedMetric = Metric.F1Score)
    {
        algorithmGraph ??= new LogicGpGraph();
        ConfusionAndSizeFitnessValue.UsedMetric = usedMetric;
        RunStrategy = new RlcwRunStrategy(algorithmGraph, phase1Generations,
            phase2Generations,
            folds, minMaxWeight);
    }
}