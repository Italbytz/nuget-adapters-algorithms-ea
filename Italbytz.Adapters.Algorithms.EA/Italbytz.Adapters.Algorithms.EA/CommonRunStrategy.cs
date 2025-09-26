using System.Collections.Generic;
using Italbytz.EA.Control;
using Italbytz.EA.Individuals;
using Microsoft.ML;

namespace Italbytz.EA;

public abstract class CommonRunStrategy : IRunStrategy
{
    protected Dictionary<float, int>[] FeatureValueMappings;
    protected Dictionary<uint, int> LabelMapping;

    public abstract IIndividual Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping);

    protected int[][] PrepareForLogicGp(List<float[]> features)
    {
        var result = new int[features.Count][];
        for (var i = 0; i < features.Count; i++)
        {
            var featureRow = features[i];
            var intRow = new int[featureRow.Length];
            for (var j = 0; j < featureRow.Length; j++)
                intRow[j] = FeatureValueMappings[j][featureRow[j]];
            result[i] = intRow;
        }

        return result;
    }


    protected int[] PrepareForLogicGp(List<uint> labels)
    {
        var result = new int[labels.Count];
        for (var i = 0; i < labels.Count; i++)
            result[i] = LabelMapping[labels[i]];
        return result;
    }
}