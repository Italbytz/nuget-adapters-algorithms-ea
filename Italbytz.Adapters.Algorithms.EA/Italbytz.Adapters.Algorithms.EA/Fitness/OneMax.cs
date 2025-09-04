using System.Linq;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;
using Microsoft.ML;

namespace Italbytz.EA.Fitness;

public class OneMax : IStaticSingleObjectiveFitnessFunction
{
    public double[] Evaluate(IIndividual individual, IDataView data)
    {
        var result = ((BitStringGenotype)individual.Genotype).BitArray;
        var count = result.Cast<bool>().Count(bit => bit);
        return [count];
    }

    public int NumberOfObjectives { get; }
    public string LabelColumnName { get; set; }
}