using System.Text.Json;
using Italbytz.EA.Searchspace;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class WeightedMonomialTests
{
    private readonly SetLiteral<int> _literal1;
    private readonly SetLiteral<int> _literal2;
    private readonly WeightedMonomial<SetLiteral<int>, int> _monomial1;

    public WeightedMonomialTests()
    {
        _literal1 = new SetLiteral<int>(5, [0, 1, 2], 6);
        _literal2 = new SetLiteral<int>(6, [0, 1, 2], 1);
        _monomial1 =
            new WeightedMonomial<SetLiteral<int>, int>([_literal1, _literal2]);
    }

    [TestMethod]
    public void TestJSONDeserialization()
    {
        var json = JsonSerializer.Serialize(_monomial1);
        var deserializedMonomial =
            JsonSerializer
                .Deserialize<WeightedMonomial<SetLiteral<int>, int>>(json);
        Assert.AreEqual(_monomial1.ToString(), deserializedMonomial.ToString());
    }
}