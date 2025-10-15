using Italbytz.EA.Searchspace;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Simulated;

[TestClass]
public class WeightedPolynomialTests
{
    private readonly SetLiteral<int> _literal1;
    private readonly SetLiteral<int> _literal2;
    private readonly SetLiteral<int> _literal3;
    private readonly SetLiteral<int> _literal4;
    private readonly SetLiteral<int> _literal5;
    private readonly WeightedMonomial<int> _monomial1;
    private readonly WeightedMonomial<int> _monomial2;
    private readonly WeightedPolynomial<int> _polynomial1;
    private readonly WeightedPolynomial<int> _polynomial2;

    public WeightedPolynomialTests()
    {
        _literal1 = new SetLiteral<int>(5, [0, 1, 2], 6);
        _literal2 = new SetLiteral<int>(6, [0, 1, 2], 1);
        _literal3 = new SetLiteral<int>(2, [0, 1, 2], 1);
        _literal4 = new SetLiteral<int>(8, [0, 1, 2], 1);
        _literal5 = new SetLiteral<int>(9, [0, 1, 2], 1);
        _monomial1 = new WeightedMonomial<int>([_literal1, _literal2]);
        _monomial2 =
            new WeightedMonomial<int>([_literal3, _literal4, _literal5]);
        _polynomial1 = new WeightedPolynomial<int>([_monomial1, _monomial2]);
        _polynomial2 =
            new WeightedPolynomial<int>([_monomial1, _monomial1, _monomial2]);
    }


    [TestMethod]
    public void TestSizes()
    {
        Assert.AreEqual(5, _polynomial1.Size);
        Assert.AreEqual(5, _polynomial2.Size);
    }
}