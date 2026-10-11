namespace VyshKat.Domain;

public abstract class Vehicle : InventoryItem
{
    public int BeginnerFriendliness { get; }
    public bool IsOperational { get; }
    protected Vehicle(string name, int id, int beginnerFriendliness, bool isOperational)
        : base(name, id)
    {
        if (beginnerFriendliness < 1 || beginnerFriendliness > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(beginnerFriendliness),
                "Показатель удобства должен быть от 1 до 10.");
        }

        BeginnerFriendliness = beginnerFriendliness;
        IsOperational = isOperational;
    }
}