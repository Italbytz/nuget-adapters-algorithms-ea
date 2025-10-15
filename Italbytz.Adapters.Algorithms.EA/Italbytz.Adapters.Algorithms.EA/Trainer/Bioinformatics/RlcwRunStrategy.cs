using System;
using System.Collections.Generic;
using System.Globalization;
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
    int phase1Time,
    int phase2Time,
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
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var cvResults = mlContext.Data.CrossValidationSplit(input);

        IIndividual bestCandidate = null;
        foreach (var fold in cvResults)
        {
            _currentMaxSize = 15;
            var individuals = TrainAndValidate(fold.TrainSet,
                fold.TestSet, featureValueMappings, labelMapping);
            allIndividuals.AddRange(individuals);

            /*
            for (var i = 1; i <= _currentMaxSize; i++)
            {
                var candidates = BestModelsForGivenSizeAndMetric([individuals],
                    i, g => g.TrainingFitness.ConsolidatedValue);
                if (candidates.Count == 0) continue;
                bestCandidate = candidates[0];
                if (bestCandidate.Genotype is not IValidatableGenotype gen)
                    throw new ArgumentException(
                        "Expected genotype of type IValidatableGenotype");
                Console.WriteLine(
                    $"{bestCandidate.Size}, {gen.TrainingFitness.ConsolidatedValue.ToString(CultureInfo.InvariantCulture)}, \"Training\"");
                Console.WriteLine(
                    $"{bestCandidate.Size}, {gen.ValidationFitness.ConsolidatedValue.ToString(CultureInfo.InvariantCulture)}, \"Validation\"");
            }*/
        }

        var paretoFront = ParetoFront(allIndividuals);
        foreach (var individual in paretoFront)
        {
            if (individual.Genotype is not IValidatableGenotype genotype)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            Console.WriteLine(
                $"{individual.Size}, {genotype.TrainingFitness.ConsolidatedValue.ToString(CultureInfo.InvariantCulture)}, {genotype.ValidationFitness.ConsolidatedValue.ToString(CultureInfo.InvariantCulture)}");
        }

        return (paretoFront[0], paretoFront);

        return (bestCandidate, allIndividuals);

        // Phase 1: Determine model size
        sizeDetermination = true;
        var fitnessDecreases = 0;
        var bestMedianFitness = 0.0;
        var chosenSize = 1;
        _currentMaxSize = 1;
        IIndividualList[] individualLists;
        int foldIndex;
        IIndividualList? chosenIndividualsPhase1 = null;
        while (fitnessDecreases < 4)
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
                         4 * g.ValidationFitness.ConsolidatedValue);


            var medianFitness = CalculateFitnessMedianOfBestIndividuals(
                bestIndividualsPhase1,
                g => g.ValidationFitness.ConsolidatedValue);

            // Check if we should increase the size or stop
            if (_currentMaxSize >= MaximumSize ||
                medianFitness < bestMedianFitness)
            {
                fitnessDecreases++;
            }
            else
            {
                fitnessDecreases = 0;
                bestMedianFitness = medianFitness;
                chosenSize = _currentMaxSize;
                chosenIndividualsPhase1 = bestIndividualsPhase1;
            }

            _currentMaxSize++;
        }

        var bestIndividualPhase1 = DetermineBestIndividual(
            chosenIndividualsPhase1,
            g => g.ValidationFitness.ConsolidatedValue);
        allIndividuals.Add(bestIndividualPhase1);

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

        foreach (var list in individualLists) allIndividuals.AddRange(list);

        var bestIndividualsPhase2 = BestModelsForGivenSizeAndMetric(
            individualLists,
            _currentMaxSize,
            g => g.TrainingFitness.ConsolidatedValue +
                 5 * g.ValidationFitness.ConsolidatedValue);

        bestIndividualsPhase2.Add(bestIndividualPhase1);

        var bestIndividual = DetermineBestIndividual(bestIndividualsPhase2,
            g => g.ValidationFitness.ConsolidatedValue);


        return (bestIndividual, allIndividuals);
    }

    private IIndividualList ParetoFront(IIndividualList individuals)
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

        return paretoFront;
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
            maxTime: sizeDetermination ? phase1Time : phase2Time,
            minMaxWeight: minMaxWeight);
    }
}