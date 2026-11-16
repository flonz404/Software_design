namespace Pos.Domain;

public sealed record Weight
{
    public decimal Value { get; }

    public Weight(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Вес должен быть положительным.");
        }

        Value = value;
    }
}