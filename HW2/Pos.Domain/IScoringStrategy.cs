namespace Pos.Domain;

public interface IScoringStrategy
{
    ScoreResult Apply(decimal weightedScore, int gradedCount, int criterionCount);
}
