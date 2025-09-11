namespace Italbytz.EA.Individuals;

public interface IPredictingGenotype : IGenotype
{
    double PredictValue(double[] features);
    string PredictClass(double[] features);
}