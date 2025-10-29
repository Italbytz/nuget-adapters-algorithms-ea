using System.Text.Json;
using Italbytz.EA.Searchspace;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Simulated;

[TestClass]
public class WeightedPolynomialGenotypeTests
{
    private readonly WeightedPolynomialGenotype<SetLiteral<int>, int>
        _genotype1;

    private readonly SetLiteral<int> _literal1;
    private readonly SetLiteral<int> _literal2;
    private readonly SetLiteral<int> _literal3;
    private readonly SetLiteral<int> _literal4;
    private readonly SetLiteral<int> _literal5;
    private readonly WeightedMonomial<SetLiteral<int>, int> _monomial1;
    private readonly WeightedMonomial<SetLiteral<int>, int> _monomial2;
    private readonly WeightedPolynomial<SetLiteral<int>, int> _polynomial1;

    public WeightedPolynomialGenotypeTests()
    {
        _literal1 = new SetLiteral<int>(5, [0, 1, 2], 6);
        _literal2 = new SetLiteral<int>(6, [0, 1, 2], 1);
        _literal3 = new SetLiteral<int>(2, [0, 1, 2], 1);
        _literal4 = new SetLiteral<int>(8, [0, 1, 2], 1);
        _literal5 = new SetLiteral<int>(9, [0, 1, 2], 1);
        _monomial1 =
            new WeightedMonomial<SetLiteral<int>, int>([_literal1, _literal2]);
        _monomial2 =
            new WeightedMonomial<SetLiteral<int>, int>([
                _literal3, _literal4, _literal5
            ]);
        _polynomial1 =
            new WeightedPolynomial<SetLiteral<int>, int>([
                _monomial1, _monomial2
            ]);
        _genotype1 =
            new WeightedPolynomialGenotype<SetLiteral<int>, int>(_polynomial1,
                null, Weighting.Fixed);
    }

    [TestMethod]
    public void TestJSONDeserialization()
    {
        var json = JsonSerializer.Serialize(_genotype1);
        var deserializedGenotype =
            JsonSerializer
                .Deserialize<WeightedPolynomialGenotype<SetLiteral<int>, int>>(
                    json);
        Assert.AreEqual(_polynomial1.ToString(),
            deserializedGenotype.ToString());
    }
}