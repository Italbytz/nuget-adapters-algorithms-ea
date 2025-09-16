using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.AI;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class TournamentSelection : AbstractSelection
{
    public int TournamentSize { get; set; } = 2;

    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individuals, int noOfIndividualsToSelect)
    {
        var individualList =
            individuals as List<IIndividual> ?? individuals.ToList();
        var selectedIndividuals = new IIndividual[noOfIndividualsToSelect];
        var rnd = ThreadSafeRandomNetCore.Shared;

        if (noOfIndividualsToSelect > 1000) // Schwellenwert anpassen
            Parallel.For(0, noOfIndividualsToSelect, i =>
            {
                IIndividual? fittest = null;
                var highestFitness = double.MinValue;
                for (var j = 0; j < TournamentSize; j++)
                {
                    var individual =
                        individualList[rnd.Next(individualList.Count)];
                    var fitness = individual.LatestKnownFitness.Sum();
                    if (fittest == null || fitness > highestFitness)
                    {
                        fittest = individual;
                        highestFitness = fitness;
                    }
                }

                selectedIndividuals[i] = fittest;
            });
        else
            for (var i = 0; i < noOfIndividualsToSelect; i++)
            {
                IIndividual? fittest = null;
                var highestFitness = double.MinValue;
                for (var j = 0; j < TournamentSize; j++)
                {
                    var individual =
                        individualList[rnd.Next(individualList.Count)];
                    var fitness = individual.LatestKnownFitness.Sum();
                    if (fittest == null || fitness > highestFitness)
                    {
                        fittest = individual;
                        highestFitness = fitness;
                    }
                }

                selectedIndividuals[i] = fittest;
            }

        return selectedIndividuals;
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}