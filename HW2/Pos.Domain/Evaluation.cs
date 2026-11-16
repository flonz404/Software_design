namespace Pos.Domain;

public class Evaluation
{
    public int StudentId { get; }
    public int CriterionId { get; }
    public decimal Score { get; }

    internal Evaluation(
        Student student,
        Criterion criterion,
        decimal score)
    {
        if (student is null)
        {
            throw new ArgumentNullException(nameof(student));
        }

        if (criterion is null)
        {
            throw new ArgumentNullException(nameof(criterion));
        }

        if (score < 0 || score > criterion.MaxScore)
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                $"Балл должен быть от 0 до {criterion.MaxScore}.");
        }

        StudentId = student.Id;
        CriterionId = criterion.Id;
        Score = score;
    }
}