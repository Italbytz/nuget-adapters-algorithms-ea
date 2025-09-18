using System;
using System.Collections.Generic;
using Italbytz.EA.Individuals;
using Microsoft.ML;

namespace Italbytz.EA.Trainer;

public interface IRunStrategy
{
    IIndividual Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping);
}