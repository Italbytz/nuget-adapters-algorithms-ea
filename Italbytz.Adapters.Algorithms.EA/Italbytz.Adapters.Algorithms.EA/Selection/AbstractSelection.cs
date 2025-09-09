using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Operator;

namespace Italbytz.EA.Selection;

public abstract class AbstractSelection : GraphOperator
{
    public override int MaxParents { get; } = int.MaxValue;
    public int NoOfIndividualsToSelect { get; } = 1;

    public override Task<IIndividualList> Operate(
        Task<IIndividualList> individuals, IFitnessFunction fitnessFunction)
    {
        var individualList = individuals.Result;
        var newPopulation = new Population();
        // Update LatestKnownFitness
        foreach (var individual in individualList)
            individual.LatestKnownFitness ??=
                fitnessFunction.Evaluate(individual);
        var selectedIndividuals = Select(individualList);
        foreach (var individual in selectedIndividuals)
            newPopulation.Add(individual);
        return Task.FromResult<IIndividualList>(newPopulation);
    }

    protected abstract IEnumerable<IIndividual> Select(
        IIndividualList individualList);
}