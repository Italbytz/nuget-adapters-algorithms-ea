using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Selection;
using Italbytz.EA.Trainer.Gecco;
using Italbytz.ML;
using Microsoft.ML;

namespace Italbytz.EA.Trainer.Bioinformatics;

public class RunStrategy(int generations, double minMaxWeight = 0.0)
    : CommonRunStrategy
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
            individualLists[foldIndex] = TrainAndValidate(fold.TrainSet,
                fold.TestSet, featureValueMappings, labelMapping);
            foldIndex++;
        }

        return SelectionStrategy.Process(individualLists);
    }

    protected override Task<IIndividualList> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            new LogicGpGraph(), new CompleteInitialization(),
            generations: generations, minMaxWeight: minMaxWeight);
    }
}