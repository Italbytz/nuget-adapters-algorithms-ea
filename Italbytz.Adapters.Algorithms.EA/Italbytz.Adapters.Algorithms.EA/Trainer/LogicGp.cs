using System.IO;

namespace Italbytz.EA.Trainer;

public static class LogicGp
{
    public static LogicGpTrainer<TOutput>? LoadTrainer<TOutput>(Stream stream)
        where TOutput : class, new()
    {
        return null;
        //return LogicGpTrainer<TOutput>.Load(stream);
    }
}