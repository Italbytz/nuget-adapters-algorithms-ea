using Italbytz.EA.Searchspace;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class SetLiteralTests
{
    private readonly List<SetLiteral<string>> _literals = [];
    
    public required HashSet<string> UniqueValues =
    [
        "1",
        "2",
        "3"
    ];

    [TestMethod]
    public void TestLogicGpAllSuLiterals()
    {
        var set = 0;
        var negativeset = 0;
        for (var i = 1; i <= UniqueValues.Count; i++)
        {
            for (var j = i; j <= UniqueValues.Count; j++)
            {
                if (i == 1 && j == UniqueValues.Count)
                    continue;
                set = set + (1 << (j - 1));
                negativeset = ~set & ((1 << UniqueValues.Count) - 1);
                var literal = new SetLiteral<string>(0, 
                    UniqueValues, set,
                    
                    SetLiteralType.Su);
                if (!_literals.Contains(literal))
                    _literals.Add(literal);
                var negativeliteral = new SetLiteral<string>(0, 
                    UniqueValues, negativeset,
                    
                    SetLiteralType.Su);
                if (!_literals.Contains(negativeliteral))
                    _literals.Add(negativeliteral);
            }

            set = 0;
            negativeset = 0;
        }

        var expectedLiterals = new List<string>
        {
            "(F0 ∈ [1,1])",
            "(F0 ∈ [2,3])",
            "(F0 ∈ [1,2])",
            "(F0 ∈ [3,3])",
            "(F0 ∈ [2,2])",
            "(F0 ∉ [2,2])"
        };
        var actualLiterals = _literals.Select(l => l.ToString()).ToList();
        Assert.AreEqual(expectedLiterals.Count, actualLiterals.Count);
        for (var i = 0; i < expectedLiterals.Count; i++)
            Assert.AreEqual(expectedLiterals[i], actualLiterals[i]);
    }

    [TestMethod]
    public void TestLogicGpAllLessGreaterLiterals()
    {
        var set = 0;
        var negativeset = 0;
        for (var i = 1; i < UniqueValues.Count; i++)
        {
            set = set + (1 << (i - 1));
            negativeset = negativeset + (1 << (UniqueValues.Count - i));

            var literal = new SetLiteral<string>(0, 
                UniqueValues, set,
                
                SetLiteralType.LessGreater);
            _literals.Add(literal);
            var negativeliteral = new SetLiteral<string>(0, 
                UniqueValues, negativeset,
                
                SetLiteralType.LessGreater);
            _literals.Add(negativeliteral);
        }

        var expectedLiterals = new List<string>
        {
            "(F0 < 2)",
            "(F0 > 2)",
            "(F0 < 3)",
            "(F0 > 1)"
        };
        var actualLiterals = _literals.Select(l => l.ToString()).ToList();
        Assert.AreEqual(expectedLiterals.Count, actualLiterals.Count);
        for (var i = 0; i < expectedLiterals.Count; i++)
            Assert.AreEqual(expectedLiterals[i], actualLiterals[i]);
    }

    [TestMethod]
    public void TestLogicGpAllDussaultLiterals()
    {
        for (var i = 1; i <= UniqueValues.Count; i++)
        {
            var set = 1 << (i - 1);
            var negativeset = ~set & ((1 << UniqueValues.Count) - 1);
            var literal = new SetLiteral<string>(0, 
                UniqueValues, set,
                
                SetLiteralType.Dussault);
            _literals.Add(literal);
            var negativeliteral = new SetLiteral<string>(0, 
                UniqueValues, negativeset,
                
                SetLiteralType.Dussault);
            _literals.Add(negativeliteral);
        }

        var expectedLiterals = new List<string>
        {
            "(F0 = 1)",
            "(F0 ≠ 1)",
            "(F0 = 2)",
            "(F0 ≠ 2)",
            "(F0 = 3)",
            "(F0 ≠ 3)"
        };
        var actualLiterals = _literals.Select(l => l.ToString()).ToList();
        Assert.AreEqual(expectedLiterals.Count, actualLiterals.Count);
        for (var i = 0; i < expectedLiterals.Count; i++)
            Assert.AreEqual(expectedLiterals[i], actualLiterals[i]);
    }

    [TestMethod]
    public void TestLogicGpAllRudellLiterals()
    {
        var powerSetCount = 1 << UniqueValues.Count;
        for (var i = 1; i < powerSetCount - 1; i++)
        {
            var literal = new SetLiteral<string>(0, 
                UniqueValues, i);
            _literals.Add(literal);
        }

        var expectedLiterals = new List<string>
        {
            "(F0 ∈ {1})",
            "(F0 ∈ {2})",
            "(F0 ∈ {1,2})",
            "(F0 ∈ {3})",
            "(F0 ∈ {1,3})",
            "(F0 ∈ {2,3})"
        };
        var actualLiterals = _literals.Select(l => l.ToString()).ToList();
        Assert.AreEqual(expectedLiterals.Count, actualLiterals.Count);
        for (var i = 0; i < expectedLiterals.Count; i++)
            Assert.AreEqual(expectedLiterals[i], actualLiterals[i]);
    }
}