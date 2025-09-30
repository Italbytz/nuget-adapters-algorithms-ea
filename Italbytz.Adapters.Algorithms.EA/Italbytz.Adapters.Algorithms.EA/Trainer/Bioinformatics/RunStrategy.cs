using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.Selection;
using Italbytz.EA.Trainer.Gecco;
using Italbytz.ML;
using Microsoft.ML;

namespace Italbytz.EA.Trainer.Bioinformatics;

public class RunStrategy(int generations) : CommonRunStrategy
{
    public IValidatedPopulationSelection SelectionStrategy { get; set; } =
        new FinalCandidatesSelection();

    public override IIndividual Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping)
    {
        const int k = 5; // Number of folds
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var cvResults = mlContext.Data.CrossValidationSplit(input);
        var individualLists = new IIndividualList[k];
        var foldIndex = 0;

        foreach (var fold in cvResults)
        {
            // Train
            var trainSet = fold.TrainSet;
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
            var validationSet = fold.TestSet;
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
                convertedValidationFeatures, convertedValidationLabels);
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

            individualLists[foldIndex] = individuals.Result;
            foldIndex++;
        }

        return SelectionStrategy.Process(individualLists);
    }

    protected override Task<IIndividualList> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            new LogicGpGraph(), new CompleteInitialization(),
            generations: generations);
    }
}