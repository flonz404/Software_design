namespace VyshKat.Domain;

public class ChargingStation : InventoryItem, IEnergyConsumer
{
    public double DailyEnergyConsumption { get; }
    
    public ChargingStation(
        string name,
        int id,
        double dailyEnergyConsumption)
        : base(name, id)
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