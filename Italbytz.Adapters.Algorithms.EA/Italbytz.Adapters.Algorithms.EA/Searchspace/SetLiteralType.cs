namespace Italbytz.EA.Searchspace;

/// <summary>
///     An enumeration representing the different types of literals used in the
///     LogicGP algorithm.
/// </summary>
/// <remarks>
///     The LogicGpLiteralType enumeration defines the types of literals that can
///     be used in the LogicGP algorithm.
///     Each type of literal has its own characteristics and is used in different
///     contexts.
///     The available types are Dussault, Rudell, Su, and LessGreater.
/// </remarks>
public enum SetLiteralType
{
    Dussault,
    Rudell,
    Su,
    LessGreater
}