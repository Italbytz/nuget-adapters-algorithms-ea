using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;

namespace Italbytz.EA.PopulationManager;

/// <inheritdoc cref="IPopulationManager" />
public class DefaultPopulationManager : IPopulationManager
{
    /// <inheritdoc />
    public IIndividualList? Population { get; set; }

    /// <inheritdoc />
    public void InitPopulation(IInitialization initialization)
    {
        Population = initialization.Process(null, null).Result;
    }

    public string GetPopulationInfo()
    {
        /*return Population?.GetRandomIndividual().ToString() ??
               "Population not initialized.";*/
        return Population?.ToString() ??
               "Population not initialized.";
    }
}