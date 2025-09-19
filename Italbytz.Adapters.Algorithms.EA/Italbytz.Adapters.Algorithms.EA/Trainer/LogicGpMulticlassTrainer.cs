namespace Italbytz.EA.Trainer;

public class LogicGpMulticlassTrainer<TOutput> : LogicGpTrainer<
    TOutput> where TOutput : class, new()
{
    public LogicGpMulticlassTrainer(int generations = 100) : base(generations)
    {
    }
}