using System;
using System.Threading.Tasks;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class LogicGpGeccoSelection : IValidatedPopulationSelection
{
    private IIndividual Process(IIndividualList[] populations)
    {
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
        }

        return null;
    }
}