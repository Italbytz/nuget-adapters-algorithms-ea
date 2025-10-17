using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.StoppingCriterion;
using Italbytz.ML;
using Microsoft.ML;

namespace Italbytz.EA;

public abstract class CommonRunStrategy : IRunStrategy
{
    protected int NumberOfObjectives { get; set; }

    public abstract (IIndividual, IIndividualList) Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping);


    protected abstract Task<IIndividualList> RunSpecificLogicGp(
        int[][] features,
        int[] labels);

    protected virtual Task<IIndividualList> RunLogicGp(int[][] features,
        int[] labels, OperatorGraph algorithmGraph,
        IInitialization initialization,
        int maxModelSize = int.MaxValue,
        Weighting weighting = Weighting.Computed,
        int generations = int.MaxValue, int maxTime = 60,
        double minMaxWeight = 0.0)
    {
        var logicGp = new EvolutionaryAlgorithm
        {
            FitnessFunction =
                new ConfusionAndSizeFitnessFunction<int>(features, labels,
                    NumberOfObjectives)
                {
                    MaxSize = maxModelSize
                },
            SearchSpace =
                new LogicGpSearchSpace<int>(features, labels, minMaxWeight)
                {
                    Weighting = weighting
                },
            AlgorithmGraph = algorithmGraph
        };
        logicGp.Initialization = initialization;
        initialization.SearchSpace = logicGp.SearchSpace;

        logicGp.StoppingCriteria =
        [
            new GenerationStoppingCriterion(logicGp)
            {
                Limit = generations
            },
            new TimeStoppingCriterion(maxTime)
        ];
        var population = logicGp.Run();
        return population;
    }

    public IIndividualList TrainAndValidate(IDataView trainSet,
        IDataView validationSet, Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping)
    {
        NumberOfObjectives = labelMapping.Count;
        // Train
        var trainExcerpt = trainSet.GetDataExcerpt();
        var trainFeatures = trainExcerpt.Features;
        var trainLabels = trainExcerpt.Labels;
        var convertedTrainFeatures = MappingHelper.MapFeatures(
            trainFeatures,
            featureValueMappings);
        var convertedTrainLabels = MappingHelper.MapLabels(
            trainLabels,
            labelMapping);
        var individuals =
            RunSpecificLogicGp(convertedTrainFeatures,
                convertedTrainLabels);
        individuals.Result.Freeze();
        // Validate
        var validationExcerpt = validationSet.GetDataExcerpt();
        var validationFeatures = validationExcerpt.Features;
        var validationLabels = validationExcerpt.Labels;
        var convertedValidationFeatures =
            MappingHelper.MapFeatures(validationFeatures,
                featureValueMappings);
        var convertedValidationLabels = MappingHelper.MapLabels(
            validationLabels,
            labelMapping);
        var fitness = new ConfusionAndSizeFitnessFunction<int>(
            convertedValidationFeatures, convertedValidationLabels,
            NumberOfObjectives);
        foreach (var individual in individuals.Result)
        {
            var oldFitness =
                (IFitnessValue?)individual.LatestKnownFitness.Clone();
            var newFitness = fitness.Evaluate(individual);
            if (individual.Genotype is not IValidatableGenotype genotype)
                continue;
            genotype.TrainingFitness = oldFitness;
            genotype.ValidationFitness = (IFitnessValue?)newFitness.Clone();
        }

        return individuals.Result;
    }
}