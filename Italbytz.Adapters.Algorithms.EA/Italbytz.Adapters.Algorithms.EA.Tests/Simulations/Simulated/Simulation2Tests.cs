using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;
using Italbytz.EA.SearchSpace;
using Italbytz.ML;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Simulated;

[TestClass]
public class Simulation2Tests
{
    private readonly List<ILiteral<int>> _allLiterals;
    private readonly SetLiteral<int> _literal1;
    private readonly SetLiteral<int> _literal2;
    private readonly SetLiteral<int> _literal3;
    private readonly SetLiteral<int> _literal4;
    private readonly SetLiteral<int> _literal5;
    private readonly SetLiteral<int> _literal6;
    private readonly WeightedMonomial<int> _monomial1;
    private readonly WeightedMonomial<int> _monomial2;
    private readonly WeightedMonomial<int> _monomial3;
    private readonly WeightedMonomial<int> _monomial4;
    private readonly WeightedMonomial<int> _monomial5;
    private readonly WeightedMonomial<int> _monomial6;
    private readonly WeightedPolynomial<int> _polynomial;
    private readonly IIndividual realModel;

    public Simulation2Tests()
    {
        _literal1 = new SetLiteral<int>(0, [0, 1, 2], 6);
        _literal2 = new SetLiteral<int>(1, [0, 1, 2], 6);
        _literal3 = new SetLiteral<int>(2, [0, 1, 2], 6);
        _literal4 = new SetLiteral<int>(3, [0, 1, 2], 6);
        _literal5 = new SetLiteral<int>(4, [0, 1, 2], 6);
        _literal6 = new SetLiteral<int>(5, [0, 1, 2], 6);
        _allLiterals =
            [_literal1, _literal2, _literal3, _literal4, _literal5, _literal6];
        _monomial1 = new WeightedMonomial<int>([_literal1]);
        _monomial2 = new WeightedMonomial<int>([_literal2]);
        _monomial3 = new WeightedMonomial<int>([_literal3]);
        _monomial4 = new WeightedMonomial<int>([_literal4]);
        _monomial5 = new WeightedMonomial<int>([_literal5]);
        _monomial6 = new WeightedMonomial<int>([_literal6]);
        _polynomial = new WeightedPolynomial<int>([
            _monomial1, _monomial2,
            _monomial3, _monomial4, _monomial5, _monomial6
        ]);
        var genotype =
            new PolynomialGenotype<int>(_polynomial,
                _allLiterals,
                Weighting.Computed);
        realModel = new Individual(genotype, null);
    }


    [TestMethod]
    public void TestRealModel()
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var data =
            SNPHelper.LoadData(AppDomain.CurrentDomain.BaseDirectory, 2, 1);
        var pipeline = SNPHelper.BuildPreprocessingPipeline(mlContext);
        var processed = pipeline.Fit(data).Transform(data);
        var excerpt = processed.GetDataExcerpt();
        var features = excerpt.Features;
        var labels = excerpt.Labels;
        var featureMapping =
            MappingHelper.CreateFeatureValueMappings(features).Item1;
        var labelMapping = MappingHelper.CreateLabelMapping(labels).Item1;
        var mappedFeatures =
            MappingHelper.MapFeatures(features, featureMapping);
        var mappedLabels = MappingHelper.MapLabels(labels, labelMapping);

        var searchSpace =
            new LogicGpSearchSpace<int>(mappedFeatures, mappedLabels)
            {
                Weighting = Weighting.Computed
            };
        var allLiterals = searchSpace.Literals;


        var fitnessFunction =
            new ConfusionAndSizeFitnessFunction<int>(mappedFeatures,
                mappedLabels);
        ConfusionAndSizeFitnessValue.UsedMetric =
            (ClassMetric.Accuracy, Averaging.Micro);

        var fitness = fitnessFunction.Evaluate(realModel);
        realModel.LatestKnownFitness = fitness;
        Console.WriteLine(realModel.ToString());

        var geno1 = new PolynomialGenotype<int>(_literal1,
            _allLiterals,
            Weighting.Computed);
        var ind1 = new Individual(geno1, null);
        var fitness1 = fitnessFunction.Evaluate(ind1);
        ind1.LatestKnownFitness = fitness1;
        Console.WriteLine(ind1.ToString());
        var geno2 = new PolynomialGenotype<int>(_literal2,
            _allLiterals,
            Weighting.Computed);
        var ind2 = new Individual(geno2, null);
        var fitness2 = fitnessFunction.Evaluate(ind2);
        ind2.LatestKnownFitness = fitness2;
        Console.WriteLine(ind2.ToString());
        var geno3 = new PolynomialGenotype<int>(_literal3,
            _allLiterals,
            Weighting.Computed);
        var ind3 = new Individual(geno3, null);
        var fitness3 = fitnessFunction.Evaluate(ind3);
        ind3.LatestKnownFitness = fitness3;
        Console.WriteLine(ind3.ToString());
        var geno4 = new PolynomialGenotype<int>(_literal4,
            _allLiterals,
            Weighting.Computed);
        var ind4 = new Individual(geno4, null);
        var fitness4 = fitnessFunction.Evaluate(ind4);
        ind4.LatestKnownFitness = fitness4;
        Console.WriteLine(ind4.ToString());
        var geno5 = new PolynomialGenotype<int>(_literal5,
            _allLiterals,
            Weighting.Computed);
        var ind5 = new Individual(geno5, null);
        var fitness5 = fitnessFunction.Evaluate(ind5);
        ind5.LatestKnownFitness = fitness5;
        Console.WriteLine(ind5.ToString());
        var geno6 = new PolynomialGenotype<int>(_literal6,
            _allLiterals,
            Weighting.Computed);
        var ind6 = new Individual(geno6, null);
        var fitness6 = fitnessFunction.Evaluate(ind6);
        ind6.LatestKnownFitness = fitness6;
        Console.WriteLine(ind6.ToString());
        Console.WriteLine("--- All  Literals---");


        foreach (var literal in allLiterals)
        {
            var geno = new PolynomialGenotype<int>(literal,
                allLiterals,
                Weighting.Computed);
            var ind = new Individual(geno, null);
            var fit = fitnessFunction.Evaluate(ind);
            ind.LatestKnownFitness = fit;
            Console.WriteLine(ind.ToString());
        }
    }
}