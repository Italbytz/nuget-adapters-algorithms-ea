using System;
using System.Text;
using Italbytz.AI;
using Italbytz.EA.Individuals;
using Italbytz.EA.Mutation;
using static Italbytz.EA.Searchspace.TinyGpPrimitive;

namespace Italbytz.EA.Searchspace;

public class TinyGpGenotype : IGenotype, IMutable
{
    public TinyGpGenotype(char[] program, double[] constants, int variableCount)
    {
        Program = program;
        Constants = constants;
        VariableCount = variableCount;
    }

    public int VariableCount { get; }

    private double[] Constants { get; }

    public char[] Program { get; set; }

    public object Clone()
    {
        var newProgram = new char[Program.Length];
        Array.Copy(Program, newProgram, Program.Length);
        return new TinyGpGenotype(newProgram, Constants, VariableCount);
    }

    public double[]? LatestKnownFitness { get; set; }
    public int Size => Program.Length;

    public void Mutate(double mutationProbability)
    {
        for (var i = 0; i < Program.Length; i++)
            if (ThreadSafeRandomNetCore.LocalRandom.NextDouble() <
                mutationProbability)
            {
                if (Program[i] < FSET_START) // leaf
                    Program[i] =
                        CreateRandomLeaf(VariableCount, Constants.Length);
                else // function
                    Program[i] = CreateRandomFunctionNode();
            }

        LatestKnownFitness = null;
    }

    public int Traverse(int pos)
    {
        if (Program[pos] < FSET_START)
            return pos + 1;
        pos = Traverse(pos + 1);
        pos = Traverse(pos);
        return pos;
    }

    public static TinyGpGenotype GenerateRandomGenotype(int maxLen, int depth,
        int variableCount,
        double[] constants)
    {
        var noOfConstants = constants.Length;
        var program = new char[maxLen];
        var len = Grow(program, 0, maxLen, depth, variableCount, noOfConstants);
        while (len < 0)
            len = Grow(program, 0, maxLen, depth, variableCount, noOfConstants);
        var individualProgram = new char[len];
        Array.Copy(program, 0, individualProgram, 0, len);
        var genotype =
            new TinyGpGenotype(individualProgram, constants, variableCount);
        return genotype;
    }

    private static int Grow(char[] program, int pos, int maxLen, int depth,
        int variableCount, int noOfConstants)
    {
        while (true)
        {
            var random = ThreadSafeRandomNetCore.LocalRandom;
            var growPrimitive = random.Next(2) == 0;

            if (pos >= maxLen) return -1;

            if (pos == 0) growPrimitive = true; // force function at root

            if (!growPrimitive || depth == 0)
            {
                program[pos] = CreateRandomLeaf(variableCount, noOfConstants);
                return pos + 1;
            }

            // Create a function node
            program[pos] = CreateRandomFunctionNode();
            pos = Grow(program, pos + 1, maxLen, depth - 1, variableCount,
                noOfConstants);
            depth -= 1;
            continue;

            return 0; // should never get here
            break;
        }
    }


    private static char CreateRandomFunctionNode()
    {
        var functionType =
            ThreadSafeRandomNetCore.LocalRandom.Next(FSET_END - FSET_START +
                1) + FSET_START;
        return (char)functionType;
    }

    private static char CreateRandomLeaf(int variableCount, int numberConst)
    {
        if (numberConst == 0 ||
            ThreadSafeRandomNetCore.LocalRandom.Next(2) == 0)
            return (char)ThreadSafeRandomNetCore.LocalRandom
                .Next(variableCount);
        return (char)(variableCount +
                      ThreadSafeRandomNetCore.LocalRandom.Next(numberConst));
    }

    public override string ToString()
    {
        return PrintIndividual(Program, 0).Item2;
    }

    public double Run(double[] variables, ref int pc)
    {
        var primitive = Program[pc++];
        if (primitive < FSET_START)
            return primitive < VariableCount
                ? variables[primitive]
                : Constants[primitive - VariableCount];

        double result;
        switch ((int)primitive)
        {
            case ADD:
                result = Run(variables, ref pc) + Run(variables, ref pc);
                return result;
            case SUB:
                result = Run(variables, ref pc) - Run(variables, ref pc);
                return result;
            case MUL:
                result = Run(variables, ref pc) * Run(variables, ref pc);
                return result;
            case DIV:
                var num = Run(variables, ref pc);
                var den = Run(variables, ref pc);
                if (Math.Abs(den) < 0.0001)
                    result = num; // protect against division by zero
                else
                    result = num / den;
                return result;
        }

        throw new Exception("Unknown primitive");
    }

    private (int, string) PrintIndividual(char[] buffer,
        int buffercounter)
    {
        int a1, a2 = 0;
        string s1, s2;
        var sb = new StringBuilder();
        if (buffer[buffercounter] < FSET_START)
        {
            if (buffer[buffercounter] < VariableCount)
                sb.Append("X" + (buffer[buffercounter] + 1) + " ");
            else
                sb.Append(
                    Constants[buffer[buffercounter] - VariableCount] +
                    " ");
            return (++buffercounter, sb.ToString());
        }

        switch ((int)buffer[buffercounter])
        {
            case ADD:
                sb.Append("(ADD ");
                break;
            case SUB:
                sb.Append("(SUB ");
                break;
            case MUL:
                sb.Append("(MUL ");
                break;
            case DIV:
                sb.Append("(DIV ");
                break;
        }

        (a1, s1) = PrintIndividual(buffer, ++buffercounter);
        sb.Append(s1);
        (a2, s2) = PrintIndividual(buffer, a1);
        sb.Append(s2);
        sb.Append(")");
        return (a2, sb.ToString());
    }
}