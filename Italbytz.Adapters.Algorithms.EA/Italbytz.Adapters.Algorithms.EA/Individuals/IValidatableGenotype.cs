using Italbytz.EA.Fitness;

namespace Italbytz.EA.Individuals;

public interface IValidatableGenotype
{
    IFitnessValue? TrainingFitness { get; set; }
    IFitnessValue? ValidationFitness { get; set; }
}