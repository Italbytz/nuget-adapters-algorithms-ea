namespace Italbytz.EA.Mutation;

public interface ILogicGpMutable
{
    void DeleteRandomLiteral();
    bool IsEmpty();
    void DeleteRandomMonomial();
    void InsertRandomLiteral();
    void InsertRandomMonomial();
    void ReplaceRandomLiteral();
}