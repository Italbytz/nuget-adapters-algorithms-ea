using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public interface IValidatedPopulationSelection
{
    public IIndividual Process(IIndividualList[] populations);
}