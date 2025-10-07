using System.Linq;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.PopulationManager;
using Italbytz.EA.SearchSpace;
using Italbytz.EA.StoppingCriterion;

namespace Italbytz.EA;

public class EvolutionaryAlgorithm : IGenerationProvider, IAdaptionCountProvider
{
    public required IFitnessFunction FitnessFunction { get; set; }
    public required ISearchSpace SearchSpace { get; set; }
    public IInitialization? Initialization { get; set; }

    public IPopulationManager PopulationManager { get; set; } =
        new DefaultPopulationManager();

    public IIndividualList Population => PopulationManager.Population;

    public IStoppingCriterion[] StoppingCriteria { get; set; }
    public OperatorGraph AlgorithmGraph { get; set; }
    public int Generation { get; set; }

    public async Task<IIndividualList> Run()
    {
        AlgorithmGraph.Check();
        AlgorithmGraph.FitnessFunction = FitnessFunction;
        Generation = 0;
        PopulationManager.InitPopulation(Initialization);
        var stop = false;
        while (!stop)
        {
            stop = StoppingCriteria.Any(sc => sc.IsMet());
            if (stop) continue;
            var newPopulation = await AlgorithmGraph.Process(Population);
            Generation++;
            foreach (var individual in newPopulation)
                individual.Generation = Generation;
            PopulationManager.Population = newPopulation;
            /*Console.Out.WriteLine(((DefaultPopulationManager)PopulationManager)
                .GetPopulationInfo());*/
        }

        return PopulationManager.Population;
    }

    public int Adaptions { get; set; }
}