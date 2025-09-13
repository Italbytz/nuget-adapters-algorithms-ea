namespace Italbytz.EA.Individuals;

public interface IPredictingGenotype<TCategory> : IGenotype
{
    float PredictValue(float[] features);

    float[] PredictValues(float[][] features, float[] labels);
    string PredictClass(TCategory[] features);

    string[] PredictClass(TCategory[][] features, string[] labels);
}