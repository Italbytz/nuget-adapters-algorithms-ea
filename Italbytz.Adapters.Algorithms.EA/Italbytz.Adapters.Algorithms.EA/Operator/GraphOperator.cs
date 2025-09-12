using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Operator;

public abstract class GraphOperator : IGraphOperator
{
    private readonly List<Task<IIndividualList>> ParentTasks = [];
    public virtual int MaxParents { get; } = 1;
    public virtual int MaxChildren { get; } = 1;

    public List<IGraphOperator> Children { get; } = [];
    public List<IGraphOperator> Parents { get; } = [];

    public void AddChildren(params IGraphOperator[] children)
    {
        foreach (var child in children)
        {
            Children.Add(child);
            child.Parents.Add(this);
        }
    }

    public void Check()
    {
        if (Children.Count > MaxChildren)
            throw new InvalidOperationException(
                $"Operator cannot have more than {MaxChildren} children.");
        if (Parents.Count > MaxParents)
            throw new InvalidOperationException(
                $"Operator cannot have more than {MaxParents} parents.");
    }

    public async Task<IIndividualList>? Process(
        Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction)
    {
        if (Parents.Count > 1)
        {
            ParentTasks.Add(individuals);
            if (ParentTasks.Count < Parents.Count) return null;

            var results = (await Task.WhenAll(ParentTasks)).ToList();

            var totalSize = results.Sum(r => r.Count);
            var combinedIndividuals = new ListBasedPopulation(totalSize);

            foreach (var parentIndividuals in results)
                combinedIndividuals.AddRange(parentIndividuals);

            individuals = Task.FromResult<IIndividualList>(combinedIndividuals);
            ParentTasks.Clear();
        }

        var operationResult = Operate(individuals, fitnessFunction);

        if (Children.Count == 0)
            return await operationResult;

        if (Children.Count == 1)
            return await Children[0].Process(operationResult, fitnessFunction);

        Task<IIndividualList>? task = null;
        Task<IIndividualList>? chosenTask = null;
        foreach (var child in Children)
        {
            task = child.Process(operationResult, fitnessFunction);
            if (task?.Result == null) continue;
            chosenTask = task;
        }

        return await chosenTask;
    }

    public virtual Task<IIndividualList> Operate(
        Task<IIndividualList> individuals, IFitnessFunction fitnessFunction)
    {
        return individuals;
    }
}