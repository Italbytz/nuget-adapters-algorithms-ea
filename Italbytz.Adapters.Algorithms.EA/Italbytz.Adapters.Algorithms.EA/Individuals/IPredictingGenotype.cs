namespace Italbytz.EA.Individuals;

public interface IPredictingGenotype : IGenotype
{
    double PredictValue(float[] features);
    string PredictClass(float[] features);
}