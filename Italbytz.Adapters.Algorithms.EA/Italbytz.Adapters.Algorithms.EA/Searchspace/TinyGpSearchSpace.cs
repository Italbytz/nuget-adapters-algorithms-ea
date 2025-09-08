using System;
using Italbytz.AI;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class TinyGpSearchSpace : ISearchSpace
{
    public int Depth { get; set; } = 5;

    public int MaxLen { get; set; } = 10000;

    public int NumberConst { get; set; } = 100;

    public IGenotype GetRandomGenotype()
    {
        var program = new char[MaxLen];
        var len = Grow(program, 0, MaxLen, Depth);
        while (len < 0)
            len = Grow(program, 0, MaxLen, Depth);
        var individualProgram = new char[len];
        Array.Copy(program, 0, individualProgram, 0, len);
        var genotype = new TinyGpGenotype(individualProgram);
        var genString = genotype.ToString();
        return genotype;
    }

    public IIndividualList GetAStartingPopulation()
    {
        throw new NotImplementedException();
    }

    private int Grow(char[] program, int pos, int maxLen,
        int depth)
    {
        var random = ThreadSafeRandomNetCore.LocalRandom;
        var growPrimitive = random.Next(2) == 0;

        if (pos >= maxLen)
            return -1;

        if (pos == 0)
            growPrimitive = true; // force function at root

        if (!growPrimitive || depth == 0)
        {
            program[pos] = CreateRandomLeaf();
            return pos + 1;
        }

        // Create a function node
        var functionType = ThreadSafeRandomNetCore.LocalRandom.Next(
            TinyGpPrimitive.FSET_END -
            TinyGpPrimitive.FSET_START + 1) + TinyGpPrimitive.FSET_START;
        program[pos] = (char)functionType;
        return Grow(program,
            Grow(program, pos + 1, maxLen, depth - 1), maxLen, depth - 1);

        return 0; // should never get here
    }

    private char CreateRandomLeaf()
    {
        if (NumberConst == 0 ||
            ThreadSafeRandomNetCore.LocalRandom.Next(2) == 0)
            return (char)ThreadSafeRandomNetCore.LocalRandom
                .Next(TinyGpGenotype.VariableCount);
        return (char)(TinyGpGenotype.VariableCount +
                      ThreadSafeRandomNetCore.LocalRandom.Next(NumberConst));
    }
}