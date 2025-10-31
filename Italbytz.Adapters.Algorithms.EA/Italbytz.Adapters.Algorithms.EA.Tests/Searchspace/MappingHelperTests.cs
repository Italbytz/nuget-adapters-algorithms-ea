using System.Text.Json;
using Italbytz.EA.Searchspace;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class MappingHelperTests
{
    private readonly float feature1 = 0f;
    private readonly float feature2 = 1f / 3f;
    private readonly float feature3 = 2f / 3f;
    private readonly float feature4 = 1f;

    [TestMethod]
    public void TestSerialization()
    {
        List<float[]> features =
            [[feature1], [feature2], [feature3], [feature4]];
        var (mappings, reverseMappings) =
            MappingHelper.CreateFeatureValueMappings(features);
        Assert.AreEqual(0, mappings[0][feature1]);
        Assert.AreEqual(1, mappings[0][feature2]);
        Assert.AreEqual(2, mappings[0][feature3]);
        Assert.AreEqual(3, mappings[0][feature4]);
        var json = JsonSerializer.Serialize(mappings);
        var deserialized =
            JsonSerializer.Deserialize<Dictionary<float, int>[]>(json);
        Assert.AreEqual(0, deserialized[0][feature1]);
        Assert.AreEqual(1, deserialized[0][feature2]);
        Assert.AreEqual(2, deserialized[0][feature3]);
        Assert.AreEqual(3, deserialized[0][feature4]);
    }
}