using System.Collections;
using System.Collections.Generic;
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

    public int Count => _individuals.Count;
    public IIndividual this[int index] => _individuals[index];

    /// <inheritdoc />
    public void Add(IIndividual individual)
    {
        _individuals.Add(individual);
    }

    /// <inheritdoc />
    public IIndividual GetRandomIndividual()
    {
        return _individuals[
            ThreadSafeRandomNetCore.LocalRandom.Next(_individuals.Count)];
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