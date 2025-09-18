using Italbytz.EA.Individuals;
using Microsoft.ML;

namespace Italbytz.EA.Trainer;

public interface IRunStrategy
{
    IIndividual Run(IDataView input);
}