using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.EA.Searchspace;

public static class MappingHelper
{
    public static (Dictionary<float, int>[], Dictionary<int, float>[])
        CreateFeatureValueMappings(
            List<float[]> features)
    {
        if (features.Count == 0)
            return (Array.Empty<Dictionary<float, int>>(),
                Array.Empty<Dictionary<int, float>>());

        var columnCount = features[0].Length;
        var mappings = new Dictionary<float, int>[columnCount];
        var reverseMappings = new Dictionary<int, float>[columnCount];

        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            var columnValues =
                features.Select(row => row[columnIndex]).ToArray();
            var uniqueValues = new HashSet<float>(columnValues);
            var categoryList = uniqueValues.OrderBy(c => c).ToList();
            var mapping = new Dictionary<float, int>();
            var reverseMapping = new Dictionary<int, float>();
            for (var i = 0; i < categoryList.Count; i++)
            {
                mapping[categoryList[i]] = i;
                reverseMapping[i] = categoryList[i];
            }

            mappings[columnIndex] = mapping;
            reverseMappings[columnIndex] = reverseMapping;
        }

        return (mappings, reverseMappings);
    }

    public static (Dictionary<uint, int>, Dictionary<int, uint>)
        CreateLabelMapping(List<uint> labels)
    {
        var uniqueLabels = labels.Distinct().OrderBy(l => l).ToList();
        var mapping = new Dictionary<uint, int>();
        for (var i = 0; i < uniqueLabels.Count; i++)
            mapping[uniqueLabels[i]] = i;
        var reverseMapping =
            mapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        return (mapping, reverseMapping);
    }

    public static int[][] MapFeatures(List<float[]> features,
        Dictionary<float, int>[] featureValueMappings)
    {
        var result = new int[features.Count][];
        for (var i = 0; i < features.Count; i++)
        {
            var featureRow = features[i];
            var intRow = new int[featureRow.Length];
            for (var j = 0; j < featureRow.Length; j++)
                intRow[j] = featureValueMappings[j][featureRow[j]];
            result[i] = intRow;
        }

        return result;
    }


    public static int[] MapLabels(List<uint> labels,
        Dictionary<uint, int> labelMapping)
    {
        var result = new int[labels.Count];
        for (var i = 0; i < labels.Count; i++)
            result[i] = labelMapping[labels[i]];
        return result;
    }
}