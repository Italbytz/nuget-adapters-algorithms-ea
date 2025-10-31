using Microsoft.ML;

namespace Italbytz.EA.Trainer;

public class LogicGpLoadedTrainer<TOutput> : LogicGpTrainer<TOutput>
    where TOutput : class, new()
{
    protected override void PrepareForFit(IDataView input)
    {
        // Intentionally do nothing.
    }
}