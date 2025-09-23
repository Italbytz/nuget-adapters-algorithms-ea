namespace Italbytz.EA.Selection;

public class FitnessTournamentSelection : TournamentSelection
{
    public override bool UseDomination { get; } = false;
}