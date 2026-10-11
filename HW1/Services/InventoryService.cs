namespace VyshKat.Services;
using VyshKat.Domain;

public class InventoryService
{
    private readonly IServiceCenter  _serviceCenter;

    public InventoryService(IServiceCenter serviceCenter)
    {
        _serviceCenter = serviceCenter;
    }
    private readonly List<IInventoryItem> _items =
        new List<IInventoryItem>();
    public bool AddVehicle(Vehicle vehicle)
    {
        if (!_serviceCenter.Inspect(vehicle))
        {
            return false;
        }

        _items.Add(vehicle);
        return true;
    }

    public bool AddItem(IInventoryItem item)
    {
        if (item is Vehicle vehicle)
        {
            return AddVehicle(vehicle);
        }
        _items.Add(item);
        return true;
    }
    public int ItemCount
    {
        get
        {
            return _items.Count;
        }
    }
    
    public double GetTotalDailyEnergyConsumption()
    {
        double total = 0;
        foreach (IInventoryItem item in _items)
        {
            if (item is IEnergyConsumer consumer)
            {
                total += consumer.DailyEnergyConsumption;
            }
        }
        return total;
    }

    public List<Vehicle> GetBeginnerFriendlyVehicles()
    {
        List<Vehicle> result = new List<Vehicle>();

        foreach (IInventoryItem item in _items)
        {
            if (item is Vehicle vehicle
                && vehicle.BeginnerFriendliness >= 6)
            {
                result.Add(vehicle);
            }
        }

        return result;
    }
    
    public List<IInventoryItem> GetAllItems()
    {
        return new List<IInventoryItem>(_items);
    }
    
}
