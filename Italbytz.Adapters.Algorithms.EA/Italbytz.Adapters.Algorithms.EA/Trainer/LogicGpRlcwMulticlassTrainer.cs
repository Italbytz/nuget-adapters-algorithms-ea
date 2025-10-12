using Italbytz.EA.Fitness;
using Italbytz.EA.Graph;
using Italbytz.EA.Trainer.Bioinformatics;

namespace Italbytz.EA.Trainer;

public class LogicGpRlcwMulticlassTrainer<TOutput> : LogicGpMulticlassTrainer<
    TOutput> where TOutput : class, new()
{
    public LogicGpRlcwMulticlassTrainer(int phase1Time,
        int phase2Time, int maxIndividuals = 1000,
        int crossoverIndividuals = 14, int mutationIndividuals = 1,
        int folds = 5, double minMaxWeight = 0.0,
        OperatorGraph? algorithmGraph = null,
        (ClassMetric, Averaging)? usedMetric = null)
    {
        usedMetric ??=
            (ClassMetric.Accuracy, Averaging.Micro);
        algorithmGraph ??=
            new LogicGpGraph(maxIndividuals, crossoverIndividuals,
                mutationIndividuals);
        ConfusionAndSizeFitnessValue.UsedMetric =
            ((ClassMetric, Averaging))usedMetric;
        RunStrategy = new RlcwRunStrategy(algorithmGraph, phase1Time,
            phase2Time,
            folds, minMaxWeight);
    }
}