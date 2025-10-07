using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Graph;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Selection;
using Italbytz.EA.Trainer.Gecco;
using Italbytz.ML;
using Microsoft.ML;

namespace Italbytz.EA.Trainer.Bioinformatics;

public class RlcwRunStrategy(
    OperatorGraph algorithmGraph,
    int phase1Generations,
    int phase2Generations,
    int folds = 5,
    double minMaxWeight = 0.0)
    : CommonRunStrategy
{
    private const int MaximumSize = 10;
    private int _currentMaxSize;
    private bool sizeDetermination = true;

    public IValidatedPopulationSelection SelectionStrategy { get; set; } =
        new FinalCandidatesSelection();

    public override (IIndividual, IIndividualList) Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping)
    {
        var allIndividuals = new ListBasedPopulation();

        // Phase 1: Determine model size
        sizeDetermination = true;
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var fitnessDecreases = 0;
        var bestAvgFitness = 0.0;
        var chosenSize = 1;
        _currentMaxSize = 1;
        var cvResults = mlContext.Data.CrossValidationSplit(input);
        IIndividualList[] individualLists;
        int foldIndex;
        while (fitnessDecreases < 2)
        {
            individualLists = new IIndividualList[folds];
            foldIndex = 0;

            foreach (var fold in cvResults)
            {
                individualLists[foldIndex] = TrainAndValidate(fold.TrainSet,
                    fold.TestSet, featureValueMappings, labelMapping);
                foldIndex++;
            }

            var bestIndividualsPhase1 =
                BestModelsForGivenSizeAndMetric(individualLists,
                    _currentMaxSize,
                    g => g.TrainingFitness.ConsolidatedValue +
                         5 * g.ValidationFitness.ConsolidatedValue);


            var avgFitness = CalculateFitnessSumOfBestIndividuals(
                bestIndividualsPhase1,
                g => g.ValidationFitness.ConsolidatedValue);

            // Check if we should increase the size or stop
            if (_currentMaxSize >= MaximumSize ||
                avgFitness < bestAvgFitness)
            {
                fitnessDecreases++;
            }
            else
            {
                bestAvgFitness = avgFitness;
                chosenSize = _currentMaxSize;
            }

            _currentMaxSize++;
        }

        // Phase 2: Determine model 
        sizeDetermination = false;
        _currentMaxSize = chosenSize;
        individualLists = new IIndividualList[folds];
        foldIndex = 0;

        foreach (var fold in cvResults)
        {
            individualLists[foldIndex] = TrainAndValidate(fold.TrainSet,
                fold.TestSet, featureValueMappings, labelMapping);
            foldIndex++;
        }

        var bestIndividualsPhase2 = BestModelsForGivenSizeAndMetric(
            individualLists,
            _currentMaxSize,
            g => g.TrainingFitness.ConsolidatedValue +
                 5 * g.ValidationFitness.ConsolidatedValue);

        var bestIndividual = DetermineBestIndividual(bestIndividualsPhase2,
            g => g.ValidationFitness.ConsolidatedValue);


        foreach (var list in individualLists) allIndividuals.AddRange(list);

        return (bestIndividual, allIndividuals);
    }

    private (double, int) MaxValueAndIndex(double[] array)
    {
        var maxValue = double.MinValue;
        var maxIndex = -1;
        for (var i = 0; i < array.Length; i++)
        {
            if (!(array[i] > maxValue)) continue;
            maxValue = array[i];
            maxIndex = i;
        }

        return (maxValue, maxIndex);
    }


    private IIndividual DetermineBestIndividual(IIndividualList individuals,
        Func<IValidatableGenotype, double> metric)
    {
        IIndividual? bestIndividual = null;
        var chosenBestFitness = double.MinValue;
        foreach (var individual in individuals)
        {
            if (individual.Genotype is not IValidatableGenotype
                validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var validationFitnessValue = metric(validatable);
            if (validationFitnessValue < chosenBestFitness)
                continue;
            chosenBestFitness = validationFitnessValue;
            bestIndividual = individual;
        }

        return bestIndividual;
    }

    private IIndividualList BestModelsForGivenSizeAndMetric(
        IIndividualList[] individualLists,
        int targetSize, Func<IValidatableGenotype, double> metric)
    {
        IIndividualList bestIndividualsList = new ListBasedPopulation();
        foreach (var list in individualLists)
        {
            var bestFitnessInList = double.MinValue;
            IIndividual? bestIndividualInList = null;
            foreach (var individual in list)
                if (individual.Size == targetSize)
                {
                    if (individual.Genotype is not IValidatableGenotype
                        validatable)
                        throw new ArgumentException(
                            "Expected genotype of type IValidatableGenotype");
                    var fitnessValue =
                        metric(validatable);
                    if (!(fitnessValue > bestFitnessInList)) continue;
                    bestFitnessInList = fitnessValue;
                    bestIndividualInList = individual;
                }

            if (bestIndividualInList != null)
                bestIndividualsList.Add(bestIndividualInList);
        }

        return bestIndividualsList;
    }

    private double CalculateFitnessSumOfBestIndividuals(
        IIndividualList individuals, Func<IValidatableGenotype, double> metric)
    {
        var sumFitness = 0.0;
        foreach (var individual in individuals)
        {
            if (individual.Genotype is not IValidatableGenotype
                validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var validationFitnessValue =
                metric(validatable);
            sumFitness += validationFitnessValue;
        }

        return sumFitness;
    }

    protected override Task<IIndividualList> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            algorithmGraph, new CompleteInitialization(),
            _currentMaxSize,
            generations: sizeDetermination
                ? phase1Generations
                : phase2Generations,
            minMaxWeight: minMaxWeight);
    }
}