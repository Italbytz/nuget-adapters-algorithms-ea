using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Selection;
using Italbytz.EA.Trainer.Gecco;
using Italbytz.ML;
using Microsoft.ML;

namespace Italbytz.EA.Trainer.Bioinformatics;

public class FastRunStrategy(int generations, double minMaxWeight = 0.0)
    : CommonRunStrategy
{
    private const int MaximumSize = 50;
    private readonly bool sizeDetermination = true;
    private int _currentMaxSize;

    public IValidatedPopulationSelection SelectionStrategy { get; set; } =
        new FinalCandidatesSelection();

    public override IIndividual Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping)
    {
        // Phase 1: Determine model size
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var fitnessIncrease = true;
        var previousAvgFitness = 0.0;
        _currentMaxSize = 1;
        const int k = 5; // Number of folds
        var cvResults = mlContext.Data.CrossValidationSplit(input);
        while (fitnessIncrease)
        {
            var individualLists = new IIndividualList[k];
            var foldIndex = 0;

            foreach (var fold in cvResults)
            {
                individualLists[foldIndex] = TrainAndValidate(fold.TrainSet,
                    fold.TestSet, featureValueMappings, labelMapping);
                foldIndex++;
            }

            // Calculate average fitness for individuals of current size
            var avgFitness =
                CalculateAverageFitness(individualLists, _currentMaxSize);

            // Check if we should increase the size or stop

            if (_currentMaxSize >= MaximumSize ||
                avgFitness < previousAvgFitness)
            {
                fitnessIncrease = false;
                // Use the previous size as it had better fitness
                _currentMaxSize = Math.Max(1, _currentMaxSize - 1);
            }
            else
            {
                previousAvgFitness = avgFitness;
                _currentMaxSize++;
            }
        }

        return null;
    }


    private double CalculateAverageFitness(IIndividualList[] individualLists,
        int targetSize)
    {
        var count = 0;
        var sumFitness = 0.0;

        foreach (var list in individualLists)
        foreach (var individual in list)
            if (individual.Size == targetSize)
            {
                sumFitness += individual.LatestKnownFitness.ConsolidatedValue;
                count++;
            }

        return count > 0 ? sumFitness / count : 0.0;
    }


    protected override Task<IIndividualList> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            new Gecco.LogicGpGraph(), new CompleteInitialization(),
            _currentMaxSize,
            generations: sizeDetermination ? 250 : generations,
            minMaxWeight: minMaxWeight);
    }
}