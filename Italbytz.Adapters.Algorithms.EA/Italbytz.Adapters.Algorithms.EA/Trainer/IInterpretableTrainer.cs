using Italbytz.EA.Individuals;

namespace Italbytz.EA.Trainer;

public interface IInterpretableTrainer
{
    public IIndividualList FinalPopulation { get; }
    public IIndividual Model { get; }
}