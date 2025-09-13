using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Fitness;

public class LogicGpPareto<TCategory> : IStaticMultiObjectiveFitnessFunction
    where TCategory : notnull
{
    private readonly TCategory[][] _features;
    private readonly string[] _labels;
    private Dictionary<string, int> _labelToIndexMap;

    public LogicGpPareto(TCategory[][] features, string[] labels)
    {
        _features = features;
        _labels = labels;
        NumberOfObjectives =
            new HashSet<string>(_labels.Select(l => l?.ToString())).Count + 1;
    }

    public double[] Evaluate(IIndividual individual)
    {
        if (individual.Genotype is not IPredictingGenotype<TCategory> genotype)
            throw new ArgumentException(
                "Expected genotype of type IPredictingGenotype");

        var featuresLength = _features.Length;
        var objectives = new double[NumberOfObjectives];

        // Für numerische Labels: Erstelle vorher ein Mapping von Label zu Index
        _labelToIndexMap = new Dictionary<string, int>();

        for (var i = 0; i < featuresLength; i++)
        {
            var prediction = genotype.PredictClass(_features[i]);
            var labelStr = _labels[i];

            if (prediction == labelStr)
            {
                // Caching der Index-Berechnung
                if (!_labelToIndexMap.TryGetValue(labelStr, out var index))
                {
                    index = int.Parse(labelStr) - 1;
                    _labelToIndexMap[labelStr] = index;
                }

                objectives[index]++;
            }
        }

        objectives[^1] = -individual.Size;
        individual.LatestKnownFitness = objectives;

        return objectives;
    }

    public int NumberOfObjectives { get; }
}