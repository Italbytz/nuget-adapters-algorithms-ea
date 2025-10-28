using System;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.ML;

public static class TransformerChainExtensions
{
    public static void Save<TLastTransformer>(
        this TransformerChain<TLastTransformer> chain, string path)
        where TLastTransformer : class, ITransformer
    {
        foreach (var transformer in chain)
        {
            if (transformer is ICanSaveModel saveable)
            {
                using var fileStream = System.IO.File.Create(path);
            }
        }
        
    }
}