namespace Italbytz.EA.Selection;

public class NegativeTournamentSelection : TournamentSelection
{
    public NegativeTournamentSelection()
    {
        SelectWorst = true;
    }
}