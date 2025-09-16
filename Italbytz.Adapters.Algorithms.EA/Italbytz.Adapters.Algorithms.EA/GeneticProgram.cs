using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph.Common;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.PopulationManager;
using Italbytz.EA.SearchSpace;
using Italbytz.EA.StoppingCriterion;

namespace Italbytz.EA;

/// <inheritdoc cref="IGeneticProgram" />
public class GeneticProgram : IGeneticProgram, IGenerationProvider
{
    private EvolutionaryAlgorithm _ea;

    /// <inheritdoc />
    public required IFitnessFunction FitnessFunction { get; set; }

    /// <inheritdoc />
    public required IGraphOperator SelectionForOperator { get; set; }

    /// <inheritdoc />
    public required IGraphOperator SelectionForSurvival { get; set; }

    /// <inheritdoc />
    public required List<IGraphOperator> Mutations { get; set; }

    /// <inheritdoc />
    public required List<IGraphOperator> Crossovers { get; set; }

    /// <inheritdoc />
    public IInitialization Initialization { get; set; }

    /// <inheritdoc />
    public required IPopulationManager PopulationManager { get; set; }

    /// <inheritdoc />
    public ISearchSpace SearchSpace { get; set; }

    /// <inheritdoc />
    public IStoppingCriterion[] StoppingCriteria { get; set; }

    /// <inheritdoc />
    public IIndividualList Population => PopulationManager.Population;

    /// <inheritdoc />
    public int Generation
    {
        get => _ea.Generation;
        set => _ea.Generation = value;
    }

    /// <inheritdoc />
    public void InitPopulation()
    {
        Generation = 0;
        PopulationManager.InitPopulation(Initialization);
    }

    /// <inheritdoc />
    public Task<IIndividualList> Run()
    {
        _ea = new EvolutionaryAlgorithm
        {
            FitnessFunction = FitnessFunction,
            SearchSpace = SearchSpace,
            Initialization = Initialization,
            PopulationManager = PopulationManager,
            StoppingCriteria = StoppingCriteria
        };
        _ea.AlgorithmGraph = new GenericGPGraph(
            SelectionForOperator,
            Mutations,
            Crossovers, SelectionForSurvival);
        return _ea.Run();
    }
}