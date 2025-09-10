using System;
using System.Threading.Tasks;
using Italbytz.AI;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Operator;
using Italbytz.EA.Searchspace;

namespace Italbytz.EA.Crossover;

public class TinyGpCrossover : GraphOperator
{
    public override Task<IIndividualList> Operate(
        Task<IIndividualList> individuals, IFitnessFunction fitnessFunction)
    {
        var individualList = individuals.Result;
        var newPopulation = new Population();
        for (var i = 0; i < individualList.Count - 1; i += 2)
        {
            var parent = individualList[i];
            var offspring = (IIndividual)individualList[i + 1].Clone();
            if (parent.Genotype is not TinyGpGenotype parentGenotype ||
                offspring.Genotype is not TinyGpGenotype offspringGenotype)
                throw new InvalidOperationException(
                    "Individual's genotype is not TinyGpGenotype");
            var xo1Start =
                ThreadSafeRandomNetCore.LocalRandom.Next(parent.Size);
            var xo1End = parentGenotype.Traverse(xo1Start);
            var xo2Start =
                ThreadSafeRandomNetCore.LocalRandom.Next(offspring.Size);
            var xo2End = offspringGenotype.Traverse(xo2Start);
            var newProgram = new char[xo1Start + (xo2End - xo2Start) +
                                      (parent.Size - xo1End)];
            Array.Copy(parentGenotype.Program, 0, newProgram, 0, xo1Start);
            Array.Copy(offspringGenotype.Program, xo2Start, newProgram,
                xo1Start,
                xo2End - xo2Start);
            Array.Copy(parentGenotype.Program, xo1End, newProgram,
                xo1Start + (xo2End - xo2Start), parent.Size - xo1End);
            offspringGenotype.Program = newProgram;
            offspringGenotype.LatestKnownFitness = null;
            newPopulation.Add(offspring);
        }

        return Task.FromResult<IIndividualList>(newPopulation);
    }
}