using System;
using Italbytz.EA.Individuals;
using Microsoft.ML;

namespace Italbytz.EA.Fitness;

public class Accuracy : IStaticSingleObjectiveFitnessFunction
{
    private IDataView _data;

    public Accuracy(IDataView data)
    {
        _data = data;
    }

    public string LabelColumnName { get; set; } = "Label";

    public int NumberOfObjectives { get; } = 1;

    double[] IFitnessFunction.Evaluate(IIndividual individual)
    {
        throw new NotImplementedException();
    }
}