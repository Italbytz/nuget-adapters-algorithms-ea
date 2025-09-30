using Italbytz.EA.Trainer;
using Italbytz.ML;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Simulated;

[TestClass]
public class SNPSimulationTests
{
    [TestMethod]
    public void TestSNP()
    {
        GPASSimulation("Simulation1", AppDomain.CurrentDomain.BaseDirectory,
            new LogicGpGpasBinaryTrainer(10000));
    }

    private void GPASSimulation(string simulationName,
        string baseDirectory, IEstimator<ITransformer> trainer)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var dataPath = Path.Combine(baseDirectory, "Data",
            $"Simulations/{simulationName}",
            "SNPglm_1.csv");
        var data = mlContext.Data.LoadFromTextFile<SNPModelInput>(
            dataPath, hasHeader: true, separatorChar: ',');
        var pipeline = BuildPipeline(mlContext, trainer);
        var model = pipeline.Fit(data);
        var predictions = model.Transform(data);
        var metrics = mlContext.BinaryClassification.Evaluate(predictions);
        // Just ensure the process completes
        Assert.IsNotNull(metrics);
    }

    public EstimatorChain<ITransformer> BuildPipeline(MLContext mlContext,
        IEstimator<ITransformer> trainer)
    {
        var lookupData = new[]
        {
            new LookupMap<uint>(0),
            new LookupMap<uint>(1)
        };
        // Convert to IDataView
        var lookupIdvMap = mlContext.Data.LoadFromEnumerable(lookupData);

        return mlContext.Transforms.ReplaceMissingValues(new[]
            {
                new InputOutputColumnPair(@"SNP1", @"SNP1"),
                new InputOutputColumnPair(@"SNP2", @"SNP2"),
                new InputOutputColumnPair(@"SNP3", @"SNP3"),
                new InputOutputColumnPair(@"SNP4", @"SNP4"),
                new InputOutputColumnPair(@"SNP5", @"SNP5"),
                new InputOutputColumnPair(@"SNP6", @"SNP6"),
                new InputOutputColumnPair(@"SNP7", @"SNP7"),
                new InputOutputColumnPair(@"SNP8", @"SNP8"),
                new InputOutputColumnPair(@"SNP9", @"SNP9"),
                new InputOutputColumnPair(@"SNP10", @"SNP10"),
                new InputOutputColumnPair(@"SNP11", @"SNP11"),
                new InputOutputColumnPair(@"SNP12", @"SNP12"),
                new InputOutputColumnPair(@"SNP13", @"SNP13"),
                new InputOutputColumnPair(@"SNP14", @"SNP14"),
                new InputOutputColumnPair(@"SNP15", @"SNP15"),
                new InputOutputColumnPair(@"SNP16", @"SNP16"),
                new InputOutputColumnPair(@"SNP17", @"SNP17"),
                new InputOutputColumnPair(@"SNP18", @"SNP18"),
                new InputOutputColumnPair(@"SNP19", @"SNP19"),
                new InputOutputColumnPair(@"SNP20", @"SNP20"),
                new InputOutputColumnPair(@"SNP21", @"SNP21"),
                new InputOutputColumnPair(@"SNP22", @"SNP22"),
                new InputOutputColumnPair(@"SNP23", @"SNP23"),
                new InputOutputColumnPair(@"SNP24", @"SNP24"),
                new InputOutputColumnPair(@"SNP25", @"SNP25"),
                new InputOutputColumnPair(@"SNP26", @"SNP26"),
                new InputOutputColumnPair(@"SNP27", @"SNP27"),
                new InputOutputColumnPair(@"SNP28", @"SNP28"),
                new InputOutputColumnPair(@"SNP29", @"SNP29"),
                new InputOutputColumnPair(@"SNP30", @"SNP30"),
                new InputOutputColumnPair(@"SNP31", @"SNP31"),
                new InputOutputColumnPair(@"SNP32", @"SNP32"),
                new InputOutputColumnPair(@"SNP33", @"SNP33"),
                new InputOutputColumnPair(@"SNP34", @"SNP34"),
                new InputOutputColumnPair(@"SNP35", @"SNP35"),
                new InputOutputColumnPair(@"SNP36", @"SNP36"),
                new InputOutputColumnPair(@"SNP37", @"SNP37"),
                new InputOutputColumnPair(@"SNP38", @"SNP38"),
                new InputOutputColumnPair(@"SNP39", @"SNP39"),
                new InputOutputColumnPair(@"SNP40", @"SNP40"),
                new InputOutputColumnPair(@"SNP41", @"SNP41"),
                new InputOutputColumnPair(@"SNP42", @"SNP42"),
                new InputOutputColumnPair(@"SNP43", @"SNP43"),
                new InputOutputColumnPair(@"SNP44", @"SNP44"),
                new InputOutputColumnPair(@"SNP45", @"SNP45"),
                new InputOutputColumnPair(@"SNP46", @"SNP46"),
                new InputOutputColumnPair(@"SNP47", @"SNP47"),
                new InputOutputColumnPair(@"SNP48", @"SNP48"),
                new InputOutputColumnPair(@"SNP49", @"SNP49"),
                new InputOutputColumnPair(@"SNP50", @"SNP50")
            })
            .Append(mlContext.Transforms.Concatenate(@"Features", @"SNP1",
                @"SNP2", @"SNP3", @"SNP4", @"SNP5", @"SNP6", @"SNP7",
                @"SNP8",
                @"SNP9", @"SNP10", @"SNP11", @"SNP12", @"SNP13", @"SNP14",
                @"SNP15", @"SNP16", @"SNP17", @"SNP18", @"SNP19", @"SNP20",
                @"SNP21", @"SNP22", @"SNP23", @"SNP24", @"SNP25", @"SNP26",
                @"SNP27", @"SNP28", @"SNP29", @"SNP30", @"SNP31", @"SNP32",
                @"SNP33", @"SNP34", @"SNP35", @"SNP36", @"SNP37", @"SNP38",
                @"SNP39", @"SNP40", @"SNP41", @"SNP42", @"SNP43", @"SNP44",
                @"SNP45", @"SNP46", @"SNP47", @"SNP48", @"SNP49", @"SNP50"))
            .Append(mlContext.Transforms.Conversion.MapValueToKey(@"Label",
                @"y",
                keyData: lookupIdvMap))
            .Append(trainer);
    }
}

public class SNPModelInput
{
    [LoadColumn(0)] [ColumnName(@"y")] public uint Y { get; set; }

    [LoadColumn(1)] [ColumnName(@"SNP1")] public float SNP1 { get; set; }

    [LoadColumn(2)] [ColumnName(@"SNP2")] public float SNP2 { get; set; }

    [LoadColumn(3)] [ColumnName(@"SNP3")] public float SNP3 { get; set; }

    [LoadColumn(4)] [ColumnName(@"SNP4")] public float SNP4 { get; set; }

    [LoadColumn(5)] [ColumnName(@"SNP5")] public float SNP5 { get; set; }

    [LoadColumn(6)] [ColumnName(@"SNP6")] public float SNP6 { get; set; }

    [LoadColumn(7)] [ColumnName(@"SNP7")] public float SNP7 { get; set; }

    [LoadColumn(8)] [ColumnName(@"SNP8")] public float SNP8 { get; set; }

    [LoadColumn(9)] [ColumnName(@"SNP9")] public float SNP9 { get; set; }

    [LoadColumn(10)]
    [ColumnName(@"SNP10")]
    public float SNP10 { get; set; }

    [LoadColumn(11)]
    [ColumnName(@"SNP11")]
    public float SNP11 { get; set; }

    [LoadColumn(12)]
    [ColumnName(@"SNP12")]
    public float SNP12 { get; set; }

    [LoadColumn(13)]
    [ColumnName(@"SNP13")]
    public float SNP13 { get; set; }

    [LoadColumn(14)]
    [ColumnName(@"SNP14")]
    public float SNP14 { get; set; }

    [LoadColumn(15)]
    [ColumnName(@"SNP15")]
    public float SNP15 { get; set; }

    [LoadColumn(16)]
    [ColumnName(@"SNP16")]
    public float SNP16 { get; set; }

    [LoadColumn(17)]
    [ColumnName(@"SNP17")]
    public float SNP17 { get; set; }

    [LoadColumn(18)]
    [ColumnName(@"SNP18")]
    public float SNP18 { get; set; }

    [LoadColumn(19)]
    [ColumnName(@"SNP19")]
    public float SNP19 { get; set; }

    [LoadColumn(20)]
    [ColumnName(@"SNP20")]
    public float SNP20 { get; set; }

    [LoadColumn(21)]
    [ColumnName(@"SNP21")]
    public float SNP21 { get; set; }

    [LoadColumn(22)]
    [ColumnName(@"SNP22")]
    public float SNP22 { get; set; }

    [LoadColumn(23)]
    [ColumnName(@"SNP23")]
    public float SNP23 { get; set; }

    [LoadColumn(24)]
    [ColumnName(@"SNP24")]
    public float SNP24 { get; set; }

    [LoadColumn(25)]
    [ColumnName(@"SNP25")]
    public float SNP25 { get; set; }

    [LoadColumn(26)]
    [ColumnName(@"SNP26")]
    public float SNP26 { get; set; }

    [LoadColumn(27)]
    [ColumnName(@"SNP27")]
    public float SNP27 { get; set; }

    [LoadColumn(28)]
    [ColumnName(@"SNP28")]
    public float SNP28 { get; set; }

    [LoadColumn(29)]
    [ColumnName(@"SNP29")]
    public float SNP29 { get; set; }

    [LoadColumn(30)]
    [ColumnName(@"SNP30")]
    public float SNP30 { get; set; }

    [LoadColumn(31)]
    [ColumnName(@"SNP31")]
    public float SNP31 { get; set; }

    [LoadColumn(32)]
    [ColumnName(@"SNP32")]
    public float SNP32 { get; set; }

    [LoadColumn(33)]
    [ColumnName(@"SNP33")]
    public float SNP33 { get; set; }

    [LoadColumn(34)]
    [ColumnName(@"SNP34")]
    public float SNP34 { get; set; }

    [LoadColumn(35)]
    [ColumnName(@"SNP35")]
    public float SNP35 { get; set; }

    [LoadColumn(36)]
    [ColumnName(@"SNP36")]
    public float SNP36 { get; set; }

    [LoadColumn(37)]
    [ColumnName(@"SNP37")]
    public float SNP37 { get; set; }

    [LoadColumn(38)]
    [ColumnName(@"SNP38")]
    public float SNP38 { get; set; }

    [LoadColumn(39)]
    [ColumnName(@"SNP39")]
    public float SNP39 { get; set; }

    [LoadColumn(40)]
    [ColumnName(@"SNP40")]
    public float SNP40 { get; set; }

    [LoadColumn(41)]
    [ColumnName(@"SNP41")]
    public float SNP41 { get; set; }

    [LoadColumn(42)]
    [ColumnName(@"SNP42")]
    public float SNP42 { get; set; }

    [LoadColumn(43)]
    [ColumnName(@"SNP43")]
    public float SNP43 { get; set; }

    [LoadColumn(44)]
    [ColumnName(@"SNP44")]
    public float SNP44 { get; set; }

    [LoadColumn(45)]
    [ColumnName(@"SNP45")]
    public float SNP45 { get; set; }

    [LoadColumn(46)]
    [ColumnName(@"SNP46")]
    public float SNP46 { get; set; }

    [LoadColumn(47)]
    [ColumnName(@"SNP47")]
    public float SNP47 { get; set; }

    [LoadColumn(48)]
    [ColumnName(@"SNP48")]
    public float SNP48 { get; set; }

    [LoadColumn(49)]
    [ColumnName(@"SNP49")]
    public float SNP49 { get; set; }

    [LoadColumn(50)]
    [ColumnName(@"SNP50")]
    public float SNP50 { get; set; }
}