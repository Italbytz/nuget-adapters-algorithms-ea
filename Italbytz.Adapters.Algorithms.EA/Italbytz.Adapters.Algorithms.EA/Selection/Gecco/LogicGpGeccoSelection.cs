using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class LogicGpGeccoSelection : IValidatedPopulationSelection
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
                    individual.LatestKnownFitness = genotype.TrainingFitness;
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
            return new GeccoFinalModelSelection().Process(candidatePopulation)[0];
    }

    private IIndividual? ChooseBestIndividual(Task<IIndividualList> individuals)
    {
        var population = individuals.Result;
        IIndividual? bestIndividual = null;
        var bestFitness = double.MinValue;
        foreach (var individual in population)
        {
            var fitness = individual.LatestKnownFitness.Sum();
            if (fitness > bestFitness)
            {
                bestFitness = fitness;
                bestIndividual = individual;
            }
        }

        return bestIndividual;
    }
}