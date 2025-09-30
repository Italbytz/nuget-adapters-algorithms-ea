using System.Threading.Tasks;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;

namespace Italbytz.EA.Trainer.Gecco;

public class GPASRunStrategy(int generations) : RunStrategy(generations)
{
    protected override Task<IIndividualList> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            new LogicGpGraph(), new RandomInitialization { Size = 2 },
            weighting: Weighting.ComputedBinary, generations: generations);
    }
}