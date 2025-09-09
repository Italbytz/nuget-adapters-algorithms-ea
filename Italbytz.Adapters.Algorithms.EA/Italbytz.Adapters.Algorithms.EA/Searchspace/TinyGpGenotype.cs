using System;
using System.Text;
using Italbytz.EA.Individuals;
using static Italbytz.EA.Searchspace.TinyGpPrimitive;

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

    public double[]? LatestKnownFitness { get; set; }
    public int Size { get; }

    public override string ToString()
    {
        return PrintIndividual(Program, 0).Item2;
    }

    public double Run(double[] variables, ref int pc)
    {
        var primitive = Program[pc++];
        if (primitive < FSET_START)
        {
            if (primitive < VariableCount)
                /*                Console.WriteLine(
                    $"Evaluating variable X{primitive + 1} with value {variables[primitive]}");*/
                return variables[primitive];

            /*Console.WriteLine(
                $"Evaluating constant C{primitive - VariableCount + 1} with value{primitive - VariableCount + 1}");*/
            return primitive - VariableCount + 1;
        }

        double result;
        switch ((int)primitive)
        {
            case ADD:
                result = Run(variables, ref pc) + Run(variables, ref pc);
                //Console.WriteLine($"Adding: {result}");
                return result;
            case SUB:
                result = Run(variables, ref pc) - Run(variables, ref pc);
                //Console.WriteLine($"Subtracting: {result}");
                return result;
            case MUL:
                result = Run(variables, ref pc) * Run(variables, ref pc);
                //Console.WriteLine($"Multiplying: {result}");
                return result;
            case DIV:
                var num = Run(variables, ref pc);
                var den = Run(variables, ref pc);
                if (Math.Abs(den) < 0.0001)
                    result = num; // protect against division by zero
                else
                    result = num / den;
                //Console.WriteLine($"Dividing: {result}");
                return result;
        }

        throw new Exception("Unknown primitive");
    }

    private static (int, string) PrintIndividual(char[] buffer,
        int buffercounter)
    {
        int a1, a2 = 0;
        string s1, s2 = null;
        var sb = new StringBuilder();
        if (buffer[buffercounter] < FSET_START)
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