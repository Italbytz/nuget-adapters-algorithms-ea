using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Selection;

namespace Italbytz.EA.Gecco;

public class FinalCandidatesSelection : IValidatedPopulationSelection
{
    public IIndividual Process(IIndividualList[] populations)
    {
        var allFilteredIndividuals = new List<IIndividualList>();
            
            foreach (var population in populations)
            {
                foreach (var individual in population)
                {
                    if (individual.Genotype is not IValidatableGenotype genotype)
                        throw new InvalidOperationException(
                            "Genotype does not implement IValidatableGenotype");
                    individual.LatestKnownFitness = (IFitnessValue?)genotype.ValidationFitness.Clone();
                }
            
                var filteringSelection = new BestModelForEachSizeSelection();
                var filteredPopulation =
                    filteringSelection.Process(Task.FromResult(population), null);
                allFilteredIndividuals.Add(filteredPopulation.Result);
            }
            var allCandidates = allFilteredIndividuals.SelectMany(i => i).ToList();
            var candidatePopulation = new ListBasedPopulation();
            foreach (var candidate in allCandidates)
                candidatePopulation.Add(candidate);
            return new FinalModelSelection().Process(candidatePopulation)[0];
    }

    private IIndividual? ChooseBestIndividual(Task<IIndividualList> individuals)
    {
        var population = individuals.Result;
        IIndividual? bestIndividual = null;
        IFitnessValue? bestFitness = null;
        foreach (var individual in population)
        {
            var fitness = individual.LatestKnownFitness;
            if (fitness == null) continue;
            if (bestFitness != null && fitness.CompareTo(bestFitness) <= 0)
                continue;
                bestFitness = fitness;
                bestIndividual = individual;
        }

        return bestIndividual;
    }
}