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

public class RlcwRunStrategy(
    int phase1Generations,
    int phase2Generations,
    double minMaxWeight = 0.0)
    : CommonRunStrategy
{
    private const int MaximumSize = 50;
    private int _currentMaxSize;
    private bool sizeDetermination = true;

    public IValidatedPopulationSelection SelectionStrategy { get; set; } =
        new FinalCandidatesSelection();

    public override (IIndividual, IIndividualList) Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping)
    {
        // Phase 1: Determine model size
        sizeDetermination = true;
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var fitnessIncrease = true;
        var previousAvgFitness = 0.0;
        _currentMaxSize = 1;
        const int k = 5; // Number of folds
        var cvResults = mlContext.Data.CrossValidationSplit(input);
        IIndividualList[] individualLists;
        int foldIndex;
        while (fitnessIncrease)
        {
            individualLists = new IIndividualList[k];
            foldIndex = 0;

            foreach (var fold in cvResults)
            {
                individualLists[foldIndex] = TrainAndValidate(fold.TrainSet,
                    fold.TestSet, featureValueMappings, labelMapping);
                foldIndex++;
            }

            var avgFitness =
                DetermineDesiredBestIndividualAndFitness(individualLists,
                    _currentMaxSize, false).Item2;

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

        // Phase 2: Determine model 
        sizeDetermination = false;
        individualLists = new IIndividualList[k];
        foldIndex = 0;

        foreach (var fold in cvResults)
        {
            individualLists[foldIndex] = TrainAndValidate(fold.TrainSet,
                fold.TestSet, featureValueMappings, labelMapping);
            foldIndex++;
        }

        var bestIndividual =
            DetermineDesiredBestIndividualAndFitness(individualLists,
                _currentMaxSize).Item1;

        var allIndividuals = new ListBasedPopulation();
        foreach (var list in individualLists) allIndividuals.AddRange(list);

        return (bestIndividual, allIndividuals);
    }

    private (IIndividual, double) DetermineDesiredBestIndividualAndFitness(
        IIndividualList[] individualLists,
        int targetSize, bool bestOfAll = true)
    {
        var chosenBestFitness = bestOfAll ? double.MinValue : double.MaxValue;
        IIndividual? chosenBestIndividual = null;

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
                        //validatable.TrainingFitness.ConsolidatedValue +
                        validatable.ValidationFitness.ConsolidatedValue;
                    if (!(fitnessValue > bestFitnessInList)) continue;
                    bestFitnessInList = fitnessValue;
                    bestIndividualInList = individual;
                }

            if (bestIndividualInList.Genotype is not IValidatableGenotype
                validatableBest)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var validationFitnessValue =
                validatableBest.ValidationFitness.ConsolidatedValue;
            //Console.WriteLine(bestIndividualInList.ToString());
            if ((validationFitnessValue < chosenBestFitness && !bestOfAll) || (
                    validationFitnessValue > chosenBestFitness && bestOfAll))
            {
                chosenBestFitness = validationFitnessValue;
                chosenBestIndividual = bestIndividualInList;
            }
        }

        return (chosenBestIndividual, chosenBestFitness);
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

    private double CalculateFitnessSumOfBestIndividuals(
        IIndividualList[] individualLists,
        int targetSize)
    {
        var sumFitness = 0.0;
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
                        //validatable.TrainingFitness.ConsolidatedValue +
                        validatable.ValidationFitness.ConsolidatedValue;
                    if (!(fitnessValue > bestFitnessInList)) continue;
                    bestFitnessInList = fitnessValue;
                    bestIndividualInList = individual;
                }

            if (bestIndividualInList.Genotype is not IValidatableGenotype
                validatableBest)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var validationFitnessValue =
                validatableBest.ValidationFitness.ConsolidatedValue;
            sumFitness += validationFitnessValue;
        }

        return sumFitness;
    }


    protected override Task<IIndividualList> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            new Gecco.LogicGpGraph(), new CompleteInitialization(),
            _currentMaxSize,
            generations: sizeDetermination
                ? phase1Generations
                : phase2Generations,
            minMaxWeight: minMaxWeight);
    }
}