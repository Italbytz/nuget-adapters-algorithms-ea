using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.Selection;
using Italbytz.EA.StoppingCriterion;
using Italbytz.EA.Trainer;
using Italbytz.ML;
using Microsoft.ML;

namespace Italbytz.EA.Gecco;

public class RunStrategy(int generations) : IRunStrategy
{
    public IValidatedPopulationSelection SelectionStrategy { get; set; } = new FinalCandidatesSelection();
    private Dictionary<float, int>[] _featureValueMappings;
    private Dictionary<uint, int> _labelMapping;

    public IIndividual Run(IDataView input, Dictionary<float, int>[] featureValueMappings, Dictionary<uint, int> labelMapping)
    {
        _featureValueMappings = featureValueMappings;
        _labelMapping = labelMapping;
        
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
            var convertedTrainFeatures = PrepareForLogicGp(trainFeatures);
            var convertedTrainLabels = PrepareForLogicGp(trainLabels);
            var individuals = RunLogicGp(convertedTrainFeatures, convertedTrainLabels);
            individuals.Result.Freeze();
            // Validate
            var validationSet = fold.TestSet;
            var validationExcerpt = validationSet.GetDataExcerpt();
            var validationFeatures = validationExcerpt.Features;
            var validationLabels = validationExcerpt.Labels;
            var convertedValidationFeatures = PrepareForLogicGp(validationFeatures);
            var convertedValidationLabels = PrepareForLogicGp(validationLabels);
            var fitness = new ClassPredictionsAndSize<int>(convertedValidationFeatures, convertedValidationLabels);
            foreach (var individual in individuals.Result)
            {
                var oldFitness = (IFitnessValue?)individual.LatestKnownFitness.Clone();
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
    

    private int[][] PrepareForLogicGp(List<float[]> features)
    {
        var result = new int[features.Count][];
        for (var i = 0; i < features.Count; i++)
        {
            var featureRow = features[i];
            var intRow = new int[featureRow.Length];
            for (var j = 0; j < featureRow.Length; j++)
                intRow[j] = _featureValueMappings[j][featureRow[j]];
            result[i] = intRow;
        }

        return result;
    }


    private int[] PrepareForLogicGp(List<uint> labels)
    {
        var result = new int[labels.Count];
        for (var i = 0; i < labels.Count; i++)
            result[i] = _labelMapping[labels[i]];
        return result;
    }


    private Task<IIndividualList> RunLogicGp(int[][] features, int[] labels)
    {
        var logicGp = new EvolutionaryAlgorithm
        {
            FitnessFunction =
                new ClassPredictionsAndSize<int>(features, labels)
                {
                    MaxSize = int.MaxValue
                },
            SearchSpace =
                new LogicGpSearchSpace<int>(features, labels)
                {
                    Weighting = Weighting.Computed
                },
            AlgorithmGraph = new LogicGpGraph()
        };
        logicGp.Initialization = new RandomInitialization(logicGp.SearchSpace)
        {
            Size = 10
        };

        logicGp.StoppingCriteria =
        [
            new GenerationStoppingCriterion(logicGp)
            {
                Limit = generations
            }
        ];
        var population = logicGp.Run();
        return population;
    }
    
}