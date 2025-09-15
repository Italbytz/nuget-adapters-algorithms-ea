namespace Italbytz.EA.Individuals;

public interface IPredictingGenotype<TCategory> : IGenotype
{
    float PredictValue(float[] features);

    float[] PredictValues(float[][] features, float[] labels);
    int PredictClass(TCategory[] features);

    int[] PredictClasses(TCategory[][] features, int[] labels);
}