using Italbytz.EA.Trainer.Gecco;

namespace Italbytz.EA.Trainer;

public class LogicGpGpasBinaryTrainer : LogicGpBinaryTrainer
{
    public LogicGpGpasBinaryTrainer(int generations)
    {
        RunStrategy = new RunStrategy(generations);
    }
}