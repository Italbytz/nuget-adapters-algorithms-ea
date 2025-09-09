using System;
using System.Text;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Searchspace;

public class TinyGpGenotype : IGenotype
{
    public TinyGpGenotype(char[] program)
    {
        Program = program;
    }

    public static int VariableCount { get; set; } = 1;

    public char[] Program { get; }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public void UpdatePredictions()
    {
        throw new NotImplementedException();
    }

    public float[][] Predictions { get; }
    public double[]? LatestKnownFitness { get; set; }
    public int Size { get; }

    public override string ToString()
    {
        return PrintIndividual(Program, 0).Item2;
    }

    private static (int, string) PrintIndividual(char[] buffer,
        int buffercounter)
    {
        int a1, a2 = 0;
        string s1, s2 = null;
        var sb = new StringBuilder();
        if (buffer[buffercounter] < TinyGpPrimitive.FSET_START)
        {
            if (buffer[buffercounter] < VariableCount)
                sb.Append("X" + (buffer[buffercounter] + 1) + " ");
            else
                sb.Append("C" +
                          (buffer[buffercounter] - VariableCount + 1) +
                          " ");
            return (++buffercounter, sb.ToString());
        }

        switch ((int)buffer[buffercounter])
        {
            case TinyGpPrimitive.ADD:
                sb.Append("(ADD ");
                break;
            case TinyGpPrimitive.SUB:
                sb.Append("(SUB ");
                break;
            case TinyGpPrimitive.MUL:
                sb.Append("(MUL ");
                break;
            case TinyGpPrimitive.DIV:
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