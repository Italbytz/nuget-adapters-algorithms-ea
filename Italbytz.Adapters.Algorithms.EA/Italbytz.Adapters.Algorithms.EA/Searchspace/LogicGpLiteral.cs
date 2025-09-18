using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpLiteral<TCategory> : ILiteral<TCategory>
{
    private readonly bool[] _bitSet;
    private readonly IList<TCategory> _categories;
    private readonly Dictionary<TCategory, int> _categoryIndexMap;
    private readonly int _feature;
    private readonly LogicGpLiteralType _literalType;
    private readonly int _set;

    public LogicGpLiteral(int feature,
        IEnumerable<TCategory> categories,
        int set,
        LogicGpLiteralType literalType = LogicGpLiteralType.Rudell)
    {
        Label = $"F{feature}";
        _categories = categories.ToList();
        _feature = feature;
        _literalType = literalType;
        _set = set;
        _bitSet = new bool[_categories.Count];
        _categoryIndexMap = new Dictionary<TCategory, int>(_categories.Count);

        for (var i = 0; i < _categories.Count; i++)
        {
            _bitSet[i] = (set & (1 << i)) != 0;
            _categoryIndexMap[_categories[i]] = i;
        }
    }

    public int CompareTo(ILiteral<TCategory>? other)
    {
        return Compare(this, other);
    }
    
    private static int Compare(ILiteral<TCategory>? x, ILiteral<TCategory>? y)
    {
        if (x is null && y is null) return 0;
        if (x is not LogicGpLiteral<TCategory> literal1) return -1;
        if (y is not LogicGpLiteral<TCategory> literal2) return 1;
        if (x.Label != y.Label)
            return string.Compare(x.Label, y.Label, StringComparison.Ordinal);
        if (literal1._set !=
            literal2._set)
            return literal1._set.CompareTo(
                literal2._set);
        return 0;
    }

    public bool Evaluate(TCategory[] input)
    {
        var value = input[_feature];
        if (_categoryIndexMap.TryGetValue(value, out var index))
            return index < _bitSet.Length && _bitSet[index];
        return false;
    }
    
    public string Label { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        if (obj is not LogicGpLiteral<TCategory> other) return false;
        if (other._literalType != _literalType) return false;
        if (other.Label != Label) return false;
        return other._set == _set;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_bitSet, Label);
    }

    #region ToString

    public override string ToString()
    {
        return _literalType switch
        {
            LogicGpLiteralType.Dussault => ToDussaultString(),
            LogicGpLiteralType.Rudell => ToRudellString(),
            LogicGpLiteralType.Su => ToSuString(),
            LogicGpLiteralType.LessGreater => ToLessGreaterString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string ToLessGreaterString()
    {
        var sb = new StringBuilder();
        if (_bitSet[0])
        {
            var index = Array.IndexOf(_bitSet, false);
            sb.Append($"({Label} < {_categories[index]})");
        }
        else
        {
            var index = Array.IndexOf(_bitSet, true);
            sb.Append($"({Label} > {_categories[index - 1]})");
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
                $"({Label} ∉ [{_categories[firstIndexNegative]},{_categories[lastIndexNegative]}])");
        else
            sb.Append(
                $"({Label} ∈ [{_categories[firstIndexPositive]},{_categories[lastIndexPositive]}])");
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
                $"({Label} = {_categories[Array.IndexOf(_bitSet, true)]})");
        else
            sb.Append(
                $"({Label} \u2260 {_categories[Array.IndexOf(_bitSet, false)]})");
        return sb.ToString();
    }

    private string ToRudellString()
    {
        var sb = new StringBuilder();
        sb.Append($"({Label} ∈ {{");
        for (var j = 0; j < _categories.Count; j++)
            if (_bitSet[j])
                sb.Append(_categories[j] + ",");
        sb.Remove(sb.Length - 1, 1);
        sb.Append("})");
        return sb.ToString();
    }

    #endregion
}