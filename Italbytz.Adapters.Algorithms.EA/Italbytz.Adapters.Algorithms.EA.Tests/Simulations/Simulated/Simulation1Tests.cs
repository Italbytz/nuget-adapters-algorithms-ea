using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;
using Italbytz.ML;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Simulated;

[TestClass]
public class Simulation1Tests
{
    private readonly IIndividual realModel;

    public Simulation1Tests()
    {
        var literal1 =
            new SetLiteral<int>(5, [0, 1, 2], 6);
        var literal2 =
            new SetLiteral<int>(6, [0, 1, 2], 1);
        var monomial1 =
            new WeightedMonomial<int>([literal1, literal2]);
        var literal3 =
            new SetLiteral<int>(2, [0, 1, 2], 1);
        var literal4 =
            new SetLiteral<int>(8, [0, 1, 2], 1);
        var literal5 =
            new SetLiteral<int>(9, [0, 1, 2], 1);
        var monomial2 =
            new WeightedMonomial<int>([literal3, literal4, literal5]);
        var polynomial =
            new WeightedPolynomial<int>([monomial1, monomial2]);
        var genotype =
            new Genotype<int>(polynomial,
                [literal1, literal2, literal3, literal4, literal5],
                Weighting.Computed);
        realModel = new Individual(genotype, null);
    }

    [TestMethod]
    public void TestRealModel()
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var data =
            SNPHelper.LoadData(AppDomain.CurrentDomain.BaseDirectory, 1, 1);
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
        var fitnessFunction =
            new ConfusionAndSizeFitnessFunction<int>(mappedFeatures,
                mappedLabels);
        ConfusionAndSizeFitnessValue.UsedMetric = Metric.MicroAccuracy;

        var fitness = fitnessFunction.Evaluate(realModel);
        realModel.LatestKnownFitness = fitness;
        Console.WriteLine(realModel.ToString());
    }
}