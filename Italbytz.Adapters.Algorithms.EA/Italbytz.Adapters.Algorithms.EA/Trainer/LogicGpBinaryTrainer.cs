using Italbytz.ML;

namespace Italbytz.EA.Trainer;

public class LogicGpBinaryTrainer : LogicGpTrainer<
    BinaryClassificationOutput>
{
    public LogicGpBinaryTrainer(int generations = 100) : base(generations)
    {
    }
}