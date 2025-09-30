using System.Threading.Tasks;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;

namespace Italbytz.EA.Trainer.Gecco;

public class FlrwRunStrategy(int generations, double minMaxWeight = 0.0)
    : RunStrategy(generations, minMaxWeight)
{
    protected override Task<IIndividualList> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            new LogicGpGraph(), new CompleteInitialization(),
            generations: generations, minMaxWeight: minMaxWeight);
    }
}