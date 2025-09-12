namespace Italbytz.EA.Individuals;

public interface IPredictingGenotype<TCategory> : IGenotype
{
    double PredictValue(float[] features);
    string PredictClass(TCategory[] features);
}