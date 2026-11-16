namespace Pos.Domain;

public sealed class PreliminaryScoringStrategy : IScoringStrategy
{
    public ScoreResult Apply(decimal weightedScore, int gradedCount, int criterionCount)
    {
        return new ScoreResult(weightedScore, true, gradedCount, criterionCount,
            "Предварительный итог (пропуски дают нулевой вклад)");
    }
}
