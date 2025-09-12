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
    private readonly int _feature;
    private readonly LogicGpLiteralType _literalType;

    public LogicGpLiteral(int feature,
        IEnumerable<TCategory> categories,
        int set,
        LogicGpLiteralType literalType = LogicGpLiteralType.Rudell)
    {
        Label = $"F{feature}";
        _categories = categories.ToList();
        _feature = feature;
        _literalType = literalType;
        _bitSet = new bool[_categories.Count];
        for (var i = 0; i < _categories.Count; i++)
            _bitSet[i] = (set & (1 << i)) != 0;
    }

    public int CompareTo(ILiteral<TCategory>? other)
    {
        throw new NotImplementedException();
    }

    public bool Evaluate(TCategory[] input)
    {
        var value = input[_feature];
        var index = _categories.IndexOf(value);
        return index > -1 && index < _bitSet.Length && _bitSet[index];
    }

    public void GeneratePredictions(List<TCategory> data)
    {
        throw new NotImplementedException();
    }

    public bool[] Predictions { get; set; }
    public string Label { get; set; }

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