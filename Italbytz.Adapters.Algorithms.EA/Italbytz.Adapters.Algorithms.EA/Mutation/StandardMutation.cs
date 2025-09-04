using System;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Operator;
using Italbytz.EA.Searchspace;

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
            var mutant = (IIndividual)individual.Clone();
            DoMutation(mutant);
            newPopulation.Add(mutant);
        }

        return Task.FromResult<IIndividualList>(newPopulation);
    }

    private void DoMutation(IIndividual mutant)
    {
        if (mutant.Genotype is not BitStringGenotype bitStringGenotype)
            throw new InvalidOperationException(
                "StandardMutation can currently only be applied to BitStringGenotype.");
        for (var i = 0; i < bitStringGenotype.BitArray.Count; i++)
            if (Random.Shared.NextDouble() < MutationProbability)
                bitStringGenotype.BitArray[i] = !bitStringGenotype.BitArray[i];
    }
}