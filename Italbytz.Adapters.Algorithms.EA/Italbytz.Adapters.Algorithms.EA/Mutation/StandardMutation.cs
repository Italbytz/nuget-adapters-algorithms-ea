using System;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Operator;

namespace Italbytz.EA.Mutation;

public class StandardMutation : GraphOperator
{
    public double MutationProbability { get; set; } = 1.0 / 64.0;

    public override Task<IIndividualList> Operate(
        Task<IIndividualList> individuals, IFitnessFunction fitnessFunction)
    {
        var individualList = individuals.Result;
        var newPopulation = new Population();
        foreach (var individual in individualList)
        {
            var candidate = (IIndividual)individual.Clone();
            if (candidate.Genotype is not IMutable mutant)
                throw new InvalidOperationException("Mutant is not IMutable");
            mutant.Mutate(MutationProbability);
            newPopulation.Add(candidate);
        }

        return Task.FromResult<IIndividualList>(newPopulation);
    }
}