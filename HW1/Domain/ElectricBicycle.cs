namespace VyshKat.Domain;

public class ElectricBicycle : Vehicle, IEnergyConsumer
{
    public double DailyEnergyConsumption { get; }

    public ElectricBicycle(
        string name,
        int id,
        int beginnerFriendliness,
        double dailyEnergyConsumption,
        bool isOperational)
        : base(name, id, beginnerFriendliness, isOperational)
    {
        
        if (!double.IsFinite(dailyEnergyConsumption)
            || dailyEnergyConsumption < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dailyEnergyConsumption),
                "Потребление должно быть конечным неотрицательным числом.");
        }

        DailyEnergyConsumption = dailyEnergyConsumption;
    }
}