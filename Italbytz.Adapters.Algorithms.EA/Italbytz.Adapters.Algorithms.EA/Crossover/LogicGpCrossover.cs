using System;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Operator;

namespace Italbytz.EA.Crossover;

public class LogicGpCrossover : GraphOperator
{
    public override Task<IIndividualList> Operate(
        Task<IIndividualList> individuals, IFitnessFunction fitnessFunction)
    {
        var individualList = individuals.Result;
        var newPopulation = new ListBasedPopulation();
        for (var i = 0; i < individualList.Count - 1; i += 2)
        {
            var parent = individualList[i];
            var offspring = (IIndividual)individualList[i + 1].Clone();
            if (parent.Genotype is not ILogicGpCrossable parentGenotype ||
                offspring.Genotype is not ILogicGpCrossable offspringGenotype)
                throw new InvalidOperationException(
                    "Individual's genotype is not ILogicGpCrossable");
            offspringGenotype.CrossWith(parentGenotype);
            newPopulation.Add(offspring);
        }

        return Task.FromResult<IIndividualList>(newPopulation);
    }
}