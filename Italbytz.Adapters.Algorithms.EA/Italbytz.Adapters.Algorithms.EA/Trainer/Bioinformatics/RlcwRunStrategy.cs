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
    int phase1,
    int phase2Time,
    int folds = 5,
    double minMaxWeight = 0.0)
    : CommonRunStrategy
{
    private int _currentMaxSize;
    private bool sizeDetermination = true;

    public IValidatedPopulationSelection SelectionStrategy { get; set; } =
        new FinalCandidatesSelection();

    private (int, IIndividualList) DetermineModelSize(MLContext mlContext,
        IDataView input, Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping)
    {
        var candidates = new ListBasedPopulation();
        var cvResults = mlContext.Data.CrossValidationSplit(input, folds);


        foreach (var fold in cvResults)
        {
            _currentMaxSize = 15;
            var individuals = TrainAndValidate(fold.TrainSet,
                fold.TestSet, featureValueMappings, labelMapping);
            candidates.AddRange(individuals);
        }

        var paretoFront = ParetoFront(candidates, 15, 0.75);
        var mostCommonSize = GetMostCommonSize(paretoFront);

        var filteredParetoFront = FilterForSize(paretoFront, mostCommonSize);

        return (mostCommonSize, filteredParetoFront);

        /*var medianSize = GetMedianSize(paretoFront);

        foreach (var individual in paretoFront)
        {
            if (individual.Genotype is not IValidatableGenotype genotype)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            Console.WriteLine(
                $"{individual.Size}, {genotype.TrainingFitness.ConsolidatedValue.ToString(CultureInfo.InvariantCulture)}, {genotype.ValidationFitness.ConsolidatedValue.ToString(CultureInfo.InvariantCulture)}");
        }*/
    }

    private IIndividualList FilterForSize(IIndividualList individuals, int size)
    {
        var filteredParetoFront = new ListBasedPopulation();
        foreach (var individual in individuals)
            if (individual.Size == size)
                filteredParetoFront.Add(individual);
        return filteredParetoFront;
    }

    public override (IIndividual, IIndividualList) Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping)
    {
        var allIndividuals = new ListBasedPopulation();

        var mlContext = ThreadSafeMLContext.LocalMLContext;


        // Phase 1: Determine model size
        sizeDetermination = true;
        var chosenSize = phase1 < 0
            ? (-1 * phase1, new ListBasedPopulation())
            : DetermineModelSize(mlContext, input,
                featureValueMappings, labelMapping);
        allIndividuals.AddRange(chosenSize.Item2);
        _currentMaxSize = chosenSize.Item1 + 1;

        // Phase 2: Determine model 
        sizeDetermination = false;
        var individualList = TrainAndValidate(input, input,
            featureValueMappings, labelMapping);

        var filteredList = FilterForSize(individualList, chosenSize.Item1);
        if (filteredList.Count > 0) individualList = filteredList;


        allIndividuals.AddRange(individualList);


        var bestIndividual = DetermineBestIndividual(allIndividuals,
            g => g.TrainingFitness.ConsolidatedValue);


        return (bestIndividual, allIndividuals);
    }

    private int GetMedianSize(IIndividualList paretoFront)
    {
        var sizes = new List<int>();
        foreach (var individual in paretoFront)
            sizes.Add(individual.Size);
        sizes.Sort();
        var mid = sizes.Count / 2;
        return sizes[mid];
    }

    private IIndividualList ParetoFront(IIndividualList individuals,
        int maxSize, double quantile)
    {
        // Remove duplicate individuals with the same size, training fitness, and validation fitness
        var uniqueIndividuals = new ListBasedPopulation();
        var seen = new HashSet<string>();

        foreach (var ind in individuals)
        {
            if (ind.Genotype is not IValidatableGenotype validatable)
                continue;

            var key =
                $"{ind.Size}_{validatable.TrainingFitness.ConsolidatedValue}_{validatable.ValidationFitness.ConsolidatedValue}";
            if (!seen.Contains(key))
            {
                uniqueIndividuals.Add(ind);
                seen.Add(key);
            }
        }

        individuals = uniqueIndividuals;

        // Filter individuals by size
        var sizeFiltered = new ListBasedPopulation();
        foreach (var individual in individuals)
            if (individual.Size <= maxSize)
                sizeFiltered.Add(individual);
        individuals = sizeFiltered;

        // Compute Pareto front
        var paretoFront = new ListBasedPopulation();
        foreach (var individual in individuals)
        {
            if (individual.Genotype is not IValidatableGenotype validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var dominated = false;
            foreach (var otherIndividual in individuals)
            {
                if (otherIndividual == individual) continue;
                if (otherIndividual.Genotype is not IValidatableGenotype
                    otherValidatable)
                    throw new ArgumentException(
                        "Expected genotype of type IValidatableGenotype");
                if (otherValidatable.TrainingFitness.ConsolidatedValue >=
                    validatable.TrainingFitness.ConsolidatedValue &&
                    otherValidatable.ValidationFitness.ConsolidatedValue >=
                    validatable.ValidationFitness.ConsolidatedValue &&
                    otherIndividual.Size <= individual.Size &&
                    (otherValidatable.TrainingFitness.ConsolidatedValue >
                     validatable.TrainingFitness.ConsolidatedValue ||
                     otherValidatable.ValidationFitness.ConsolidatedValue >
                     validatable.ValidationFitness.ConsolidatedValue
                     || otherIndividual.Size < individual.Size))
                {
                    dominated = true;
                    break;
                }
            }

            if (!dominated) paretoFront.Add(individual);
        }

        // Select top quantile based on validation fitness
        var validationFitnessValues = new double[paretoFront.Count];
        for (var i = 0; i < paretoFront.Count; i++)
        {
            if (paretoFront[i].Genotype is not IValidatableGenotype validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            validationFitnessValues[i] =
                validatable.ValidationFitness.ConsolidatedValue;
        }

        Array.Sort(validationFitnessValues);
        var thresholdIndex =
            (int)(quantile * validationFitnessValues.Length);
        var thresholdValue = validationFitnessValues[
            Math.Clamp(thresholdIndex, 0, validationFitnessValues.Length - 1)];
        var finalParetoFront = new ListBasedPopulation();
        foreach (var individual in paretoFront)
        {
            if (individual.Genotype is not IValidatableGenotype validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            if (validatable.ValidationFitness.ConsolidatedValue >=
                thresholdValue)
                finalParetoFront.Add(individual);
        }


        return finalParetoFront;
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

    private double CalculateFitnessMedianOfBestIndividuals(
        IIndividualList individuals, Func<IValidatableGenotype, double> metric)
    {
        var fitnessValues = new List<double>();
        foreach (var individual in individuals)
        {
            if (individual.Genotype is not IValidatableGenotype
                validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var validationFitnessValue =
                metric(validatable);
            fitnessValues.Add(validationFitnessValue);
        }

        if (fitnessValues.Count == 0) return 0.0;
        fitnessValues.Sort();
        var mid = fitnessValues.Count / 2;
        if (fitnessValues.Count % 2 == 0)
            return (fitnessValues[mid - 1] + fitnessValues[mid]) / 2.0;
        return fitnessValues[mid];
    }

    protected override Task<IIndividualList> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            algorithmGraph, new CompleteInitialization(),
            _currentMaxSize,
            generations: int.MaxValue,
            maxTime: sizeDetermination ? phase1 : phase2Time,
            minMaxWeight: minMaxWeight);
    }

    private int GetMostCommonSize(IIndividualList population)
    {
        var sizeCounts = new Dictionary<int, int>();

        foreach (var individual in population)
            if (sizeCounts.ContainsKey(individual.Size))
                sizeCounts[individual.Size]++;
            else
                sizeCounts[individual.Size] = 1;

        var mostCommonSize = 0;
        var highestCount = 0;

        foreach (var pair in sizeCounts)
            if (pair.Value > highestCount)
            {
                highestCount = pair.Value;
                mostCommonSize = pair.Key;
            }

        return mostCommonSize;
    }
}