using Microsoft.ML;

namespace Italbytz.ML;

public static class ModelOperationsCatalogExtensions
{
    public static ITransformer Load(this ModelOperationsCatalog catalog,
        string fileName)
    {
        return catalog.Load(fileName, out var _);
    }
}