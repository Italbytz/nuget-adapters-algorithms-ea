using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.EA.Crossover;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Mutation;
using Italbytz.EA.PopulationManager;
using Italbytz.EA.SearchSpace;
using Italbytz.EA.Selection;
using Italbytz.EA.StoppingCriterion;
using Microsoft.ML;

namespace Italbytz.EA;

/// <inheritdoc cref="IGeneticProgram" />
public class GeneticProgram : IGeneticProgram
{
    /// <inheritdoc />
    public required IFitnessFunction FitnessFunction { get; set; }

    /// <inheritdoc />
    public required ISelection SelectionForOperator { get; set; }

    /// <inheritdoc />
    public required ISelection SelectionForSurvival { get; set; }

    /// <inheritdoc />
    public required List<IMutation> Mutations { get; set; }

    /// <inheritdoc />
    public required List<ICrossover> Crossovers { get; set; }

    /// <inheritdoc />
    public required IInitialization Initialization { get; set; }

    /// <inheritdoc />
    public required IPopulationManager PopulationManager { get; set; }

    /// <inheritdoc />
    public required ISearchSpace SearchSpace { get; set; }

    /// <inheritdoc />
    public required IStoppingCriterion[] StoppingCriteria { get; set; }

    /// <inheritdoc />
    public required IDataView TrainingData { get; set; }

    /// <inheritdoc />
    public IIndividualList Population => PopulationManager.Population;

    /// <inheritdoc />
    public int Generation { get; set; }

    /// <inheritdoc />
    public void InitPopulation()
    {
        Generation = 0;
        PopulationManager.InitPopulation(Initialization);
    }

    /// <inheritdoc />
    public IIndividualList Run()
    {
        InitPopulation();
        var stop = false;
        while (!stop)
        {
            stop = StoppingCriteria.Any(sc => sc.IsMet());
            if (stop) continue;
            var newPopulation = new Population();
            foreach (var crossover in Crossovers)
            {
                SelectionForOperator.Size = 2;
                var selected = SelectionForOperator
                    .Process(Task.FromResult(Population), null).Result;
                var children = crossover
                    .Process(Task.FromResult(selected), null).Result;
                foreach (var child in children)
                    newPopulation.Add(child);
            }

            foreach (var mutation in Mutations)
            {
                SelectionForOperator.Size = 1;
                var selected = SelectionForOperator.Process(
                    Task.FromResult(Population), null);
                var mutated = mutation.Process(selected, null);
                foreach (var mutant in mutated.Result)
                    newPopulation.Add(mutant);
            }

            Generation++;
            foreach (var individual in newPopulation)
                individual.Generation = Generation;
            foreach (var individual in PopulationManager.Population)
                newPopulation.Add(individual);
            PopulationManager.Population = newPopulation;

            UpdatePopulationFitness();

            var newGeneration =
                SelectionForSurvival.Process(Task.FromResult(Population), null);
            PopulationManager.Population = newGeneration.Result;
        }

        return PopulationManager.Population;
    }

    private void UpdatePopulationFitness()
    {
        foreach (var individual in Population)
        {
            if (individual.LatestKnownFitness != null) continue;
            var fitness =
                FitnessFunction.Evaluate(individual,
                    TrainingData);
            individual.LatestKnownFitness = fitness;
        }
    }
}