namespace Pos.Domain;

public sealed class FinalScoreCalculator
{
    public ScoreResult Calculate(Gradebook gradebook, int studentId, IScoringStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(gradebook);
        ArgumentNullException.ThrowIfNull(strategy);

        IReadOnlyCollection<Evaluation> evaluations = gradebook.GetEvaluations(studentId);
        IReadOnlyCollection<Criterion> criteria = gradebook.GetCriteria();
        if (criteria.Count == 0)
        {
            return new ScoreResult(null, false, 0, 0,
                "Расчёт невозможен: рубрикатор пуст.");
        }

        Dictionary<int, decimal> scores = new Dictionary<int, decimal>();
        foreach (Evaluation evaluation in evaluations)
        {
            scores.Add(evaluation.CriterionId, evaluation.Score);
        }

        decimal largestWeight = 0;
        foreach (Criterion criterion in criteria)
        {
            largestWeight = Math.Max(largestWeight, criterion.Weight.Value);
        }

        decimal weightedSum = 0;
        decimal weightSum = 0;
        foreach (Criterion criterion in criteria)
        {
            decimal scaledWeight = criterion.Weight.Value / largestWeight;
            weightSum += scaledWeight;
            if (scores.TryGetValue(criterion.Id, out decimal score))
            {
                weightedSum += score / criterion.MaxScore * scaledWeight;
            }
        }

        decimal result = weightedSum / weightSum * 10m;
        return strategy.Apply(result, evaluations.Count, criteria.Count);
    }
}
