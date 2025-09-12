using System;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Operator;

namespace Italbytz.EA.Mutation;

public class DeleteMonomial : GraphOperator
{
    public override Task<IIndividualList> Operate(
        Task<IIndividualList> individuals, IFitnessFunction fitnessFunction)
    {
        var individualList = individuals.Result;
        var newPopulation = new ListBasedPopulation();
        foreach (var individual in individualList)
        {
            var candidate = (IIndividual)individual.Clone();
            if (candidate.Genotype is not ILogicGpMutable mutant)
                throw new InvalidOperationException(
                    "Mutant is not ILogicGpMutable");
            mutant.DeleteRandomMonomial();
            if (!mutant.IsEmpty())
                newPopulation.Add(candidate);
        }

        return Task.FromResult<IIndividualList>(newPopulation);
    }
}