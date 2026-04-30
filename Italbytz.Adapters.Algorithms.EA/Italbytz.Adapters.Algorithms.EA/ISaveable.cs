using System.IO;

namespace Italbytz.AI;

public interface ISaveable
{
    void Save(Stream stream);
}
