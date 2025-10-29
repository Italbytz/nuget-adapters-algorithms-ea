using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class SetLiteral<TCategory> : ISetLiteral<TCategory>
{
    private readonly bool[] _bitSet;
    private readonly Dictionary<TCategory, int> _categoryIndexMap;

    [JsonConstructor]
    public SetLiteral(int feature,
        IList<TCategory> categories,
        int set,
        SetLiteralType literalType = SetLiteralType.Rudell)
    {
        (Feature, Categories, Set, LiteralType) = (feature,
            categories, set, literalType);
        Label = $"F{feature}";
        _bitSet = new bool[Categories.Count];
        _categoryIndexMap = new Dictionary<TCategory, int>(Categories.Count);

        for (var i = 0; i < Categories.Count; i++)
        {
            _bitSet[i] = (set & (1 << i)) != 0;
            _categoryIndexMap[Categories[i]] = i;
        }
    }

    [JsonInclude] public SetLiteralType LiteralType { get; }

    [JsonInclude] public IList<TCategory> Categories { get; }
    [JsonInclude] public int Feature { get; }
    [JsonInclude] public int Set { get; }

    [JsonIgnore] public string Label { get; }

    public int CompareTo(ILiteral<TCategory>? other)
    {
        return Compare(this, other);
    }

    public bool Evaluate(TCategory[] input)
    {
        var value = input[Feature];
        if (_categoryIndexMap.TryGetValue(value, out var index))
            return index < _bitSet.Length && _bitSet[index];
        return false;
    }

    private static int Compare(ILiteral<TCategory>? x, ILiteral<TCategory>? y)
    {
        if (x is null && y is null) return 0;
        if (x is not SetLiteral<TCategory> literal1) return -1;
        if (y is not SetLiteral<TCategory> literal2) return 1;
        if (literal1.Label != literal2.Label)
            return string.Compare(literal1.Label, literal2.Label,
                StringComparison.Ordinal);
        if (literal1.Set !=
            literal2.Set)
            return literal1.Set.CompareTo(
                literal2.Set);
        return 0;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        if (obj is not SetLiteral<TCategory> other) return false;
        if (other.LiteralType != LiteralType) return false;
        if (other.Label != Label) return false;
        return other.Set == Set;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_bitSet, Label);
    }

    #region ToString

    public override string ToString()
    {
        return LiteralType switch
        {
            SetLiteralType.Dussault => ToDussaultString(),
            SetLiteralType.Rudell => ToRudellString(),
            SetLiteralType.Su => ToSuString(),
            SetLiteralType.LessGreater => ToLessGreaterString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string ToLessGreaterString()
    {
        var sb = new StringBuilder();
        if (_bitSet[0])
        {
            var index = Array.IndexOf(_bitSet, false);
            sb.Append($"({Label} < {Categories[index]})");
        }
        else
        {
            var index = Array.IndexOf(_bitSet, true);
            sb.Append($"({Label} > {Categories[index - 1]})");
        }

        return sb.ToString();
    }

    private string ToSuString()
    {
        var sb = new StringBuilder();
        var firstIndexPositive = Array.IndexOf(_bitSet, true);
        if (firstIndexPositive == -1)
            throw new ArgumentException("No positive value in BitSet");
        var firstIndexNegative = Array.IndexOf(_bitSet, false);
        if (firstIndexNegative == -1)
            throw new ArgumentException("No negative value in BitSet");
        var lastIndexPositive = Array.LastIndexOf(_bitSet, true);
        var lastIndexNegative = Array.LastIndexOf(_bitSet, false);
        var negative = false;
        for (var i = firstIndexPositive; i < lastIndexPositive; i++)
            if (!_bitSet[i])
                negative = true;
        if (negative)
            sb.Append(
                $"({Label} ∉ [{Categories[firstIndexNegative]},{Categories[lastIndexNegative]}])");
        else
            sb.Append(
                $"({Label} ∈ [{Categories[firstIndexPositive]},{Categories[lastIndexPositive]}])");
        return sb.ToString();
    }

    private string ToDussaultString()
    {
        var sb = new StringBuilder();
        var count = _bitSet.Count(bit => bit);
        if (count != 1 && count != _bitSet.Length - 1)
            throw new ArgumentException(
                "Dussault literals must have exactly one or all but one bit set");
        if (count == 1)
            sb.Append(
                $"({Label} = {Categories[Array.IndexOf(_bitSet, true)]})");
        else
            sb.Append(
                $"({Label} \u2260 {Categories[Array.IndexOf(_bitSet, false)]})");
        return sb.ToString();
    }

    private string ToRudellString()
    {
        var sb = new StringBuilder();
        sb.Append($"({Label} ∈ {{");
        for (var j = 0; j < Categories.Count; j++)
            if (_bitSet[j])
                sb.Append(Categories[j] + ",");
        sb.Remove(sb.Length - 1, 1);
        sb.Append("})");
        return sb.ToString();
    }

    #endregion
}