using System;
using System.Collections.Generic;
using Italbytz.EA.Individuals;
using Microsoft.ML;

namespace Italbytz.EA.Trainer;

public interface IRunStrategy
{
    public int Classes { get;  }
    
    public Dictionary<int, uint> ReverseLabelMapping { get; }
    
    public Dictionary<float, int>[] FeatureValueMappings { get;  }

    IIndividual Run(IDataView input);
}