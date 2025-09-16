using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;

namespace Italbytz.EA.Individuals;

/// <inheritdoc cref="IIndividualList" />
public class ListBasedPopulation : IIndividualList
{
    private readonly List<IIndividual> _individuals = [];

    public ListBasedPopulation()
    {
    }

    public ListBasedPopulation(int capacity)
    {
        _individuals = new List<IIndividual>(capacity);
    }

    public void Freeze()
    {
        if (_individuals.First().Genotype is not IFreezable freezable) return;
        foreach (var individual in _individuals)
            (individual.Genotype as IFreezable)?.Freeze();
    }

    public int Count => _individuals.Count;
    public IIndividual this[int index] => _individuals[index];

    /// <inheritdoc />
    public void Add(IIndividual individual)
    {
        _individuals.Add(individual);
    }

    public void RemoveAt(int index)
    {
        _individuals.RemoveAt(index);
    }

    public void Clear()
    {
        _individuals.Clear();
    }

    public bool Remove(IIndividual individual)
    {
        return _individuals.Remove(individual);
    }

    /// <inheritdoc />
    public IIndividual GetRandomIndividual()
    {
        return _individuals[
            ThreadSafeRandomNetCore.Shared.Next(_individuals.Count)];
    }

    public void AddRange(IEnumerable<IIndividual> individuals)
    {
        _individuals.AddRange(individuals);
    }

    public void Add(IIndividualList individuals)
    {
        _individuals.AddRange(individuals);
    }

    /// <inheritdoc />
    public IEnumerator<IIndividual> GetEnumerator()
    {
        return _individuals.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public List<IIndividual> ToList()
    {
        return [.._individuals];
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Join("\n", _individuals);
    }
}