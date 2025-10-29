using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;

namespace Italbytz.EA.Trainer.Gecco;

/// <summary>
///     A class representing the final model selection process in the LogicGP
///     algorithm.
///     It implements the ISelection interface and provides methods for processing
///     a list of individuals and selecting the best model based on their fitness.
/// </summary>
/// <remarks>
///     The FinalModelSelection class is used to select the best model from a list
///     of individuals in the LogicGP algorithm.
///     It groups the individuals by their literal signature and selects the best
///     model based on their fitness.
///     The class also provides methods for processing the individuals and
///     selecting the best model.
/// </remarks>
/// <seealso cref="ISelection" />
public class FinalModelSelection
{
    public IIndividualList Process(IIndividualList individuals)
    {
        var allCandidates = individuals.ToList();
        var groups = allCandidates
            .GroupBy(i =>
                ((WeightedPolynomialGenotype<SetLiteral<int>, int>)i.Genotype)
                .LiteralSignature())
            .Where(group => group.Count() > 0)
            .OrderByDescending(group => group.FirstOrDefault().Size);
        var largestSize = groups.FirstOrDefault()!.FirstOrDefault().Size;

        var bestModels = new List<IIndividual>[largestSize];
        foreach (var group in groups)
        {
            var groupSize = group.FirstOrDefault().Size;
            if (bestModels[groupSize - 1] == null)
            {
                bestModels[groupSize - 1] = group.ToList();
            }
            else
            {
                var accumulatedFitness = group.Average(element =>
                    element.LatestKnownFitness.ConsolidatedValue
                );
                var bestFitness = bestModels[groupSize - 1].Average(element =>
                    element.LatestKnownFitness.ConsolidatedValue
                );
                if (accumulatedFitness > bestFitness)
                    bestModels[groupSize - 1] = group.ToList();
            }
        }

        var chosenGroup = bestModels.FirstOrDefault(group => group != null);
        var bestAccuracy = 0.0;

        for (var i = largestSize - 1; i >= 0; i--)
        {
            if (bestModels[i] == null || bestModels[i].Count == 0) continue;
            var accumulatedFitness = bestModels[i].Average(element =>
                element.LatestKnownFitness.ConsolidatedValue
            );
            if (accumulatedFitness < bestAccuracy) continue;
            var bestSmallerFitness = 0.0;
            for (var k = i - 1; k > 0; k--)
            {
                if (bestModels[k] == null || bestModels[k].Count == 0)
                    continue;
                if (bestModels[k].Average(element =>
                        element.LatestKnownFitness.ConsolidatedValue
                    ) > bestSmallerFitness)
                    bestSmallerFitness = bestModels[k].Average(element =>
                        element.LatestKnownFitness.ConsolidatedValue
                    );
            }

            var gain = accumulatedFitness / bestSmallerFitness;
            if (gain < 1.01) continue;
            chosenGroup = bestModels[i];
            bestAccuracy = accumulatedFitness;
        }

        var bestFitnessInChosenGroup = chosenGroup.Max(element =>
            element.LatestKnownFitness
        );
        var chosenIndividual = chosenGroup.Where(element =>
            element.LatestKnownFitness.CompareTo(bestFitnessInChosenGroup) == 0
        ).ToList().FirstOrDefault();


        return new ListBasedPopulation { chosenIndividual };
    }
}