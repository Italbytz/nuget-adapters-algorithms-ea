namespace Italbytz.EA.Individuals;

public interface IValidatableGenotype
{
    double[]? TrainingFitness { get; set; }
    double[]? ValidationFitness { get; set; }
}