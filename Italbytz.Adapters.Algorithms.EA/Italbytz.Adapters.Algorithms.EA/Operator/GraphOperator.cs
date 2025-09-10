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
            await Task.WhenAll(ParentTasks);

            var results = ParentTasks.Select(t => t.Result).ToList();
            var totalSize = results.Sum(r => r.Count);
            var combinedIndividuals = new Population(totalSize);

            foreach (var parentIndividuals in results)
            foreach (var individual in parentIndividuals)
                combinedIndividuals.Add(individual);

            individuals = Task.FromResult<IIndividualList>(combinedIndividuals);
            ParentTasks.Clear();
        }

        // This method is called to process the individuals through the operator.
        var operationResult = Operate(individuals, fitnessFunction);
        // After the operation, we process the result through the children.
        // If there are no children, we return the result directly.
        if (Children.Count == 0)
            return await operationResult;

        var childTasks = Children
            .Select(child => child.Process(operationResult, fitnessFunction))
            .Where(task => task != null)
            .Cast<Task<IIndividualList>>()
            .ToList();

        return await childTasks.Last();
    }

    public virtual Task<IIndividualList> Operate(
        Task<IIndividualList> individuals, IFitnessFunction fitnessFunction)
    {
        return individuals;
    }
}