namespace Pos.Domain;

public sealed class StrictScoringStrategy : IScoringStrategy
{
    public ScoreResult Apply(decimal weightedScore, int gradedCount, int criterionCount)
    {
        if (gradedCount != criterionCount)
        {
            return new ScoreResult(null, false, gradedCount, criterionCount,
                "Оценивание не завершено.");
        }

        return new ScoreResult(weightedScore, false, gradedCount, criterionCount,
            "Окончательный итог");
    }
}
