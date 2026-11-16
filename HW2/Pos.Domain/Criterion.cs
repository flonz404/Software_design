namespace Pos.Domain;

public class Criterion
{
    public int Id { get; }
    public string Name { get; }
    public decimal MaxScore { get; }
    public Weight Weight { get; }

    public Criterion(
        int id,
        string name,
        decimal maxScore,
        Weight weight)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "Идентификатор должен быть положительным.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Название критерия не должно быть пустым.",
                nameof(name));
        }

        if (maxScore <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxScore),
                "Максимальный балл должен быть положительным.");
        }

        if (weight is null)
        {
            throw new ArgumentNullException(nameof(weight));
        }

        Id = id;
        Name = name.Trim();
        MaxScore = maxScore;
        Weight = weight;
    }
}