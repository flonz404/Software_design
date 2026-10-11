namespace VyshKat.Domain;

public class Bicycle : Vehicle
{
    public Bicycle(
        string name,
        int id,
        int beginnerFriendliness,
        bool isOperational)
        : base(name, id, beginnerFriendliness, isOperational)
    {
    }
}