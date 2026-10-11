using VyshKat.Domain;
using VyshKat.Services;
using VyshKat.Infrastructure;

SimpleContainer container = new SimpleContainer();

container.RegisterSingleton<IServiceCenter>(CreateServiceCenter);
container.RegisterSingleton<InventoryService>(CreateInventoryService);

InventoryService inventory = container.Resolve<InventoryService>();

Bicycle cityBicycle = new Bicycle(
    "Городской велосипед", 1, 8, isOperational: true);

Bicycle brokenBicycle = new Bicycle(
    "Сломанный велосипед", 2, 9, isOperational: false);

Console.WriteLine($"Городской принят: {inventory.AddItem(cityBicycle)}");
Console.WriteLine($"Сломанный принят: {inventory.AddItem(brokenBicycle)}");
ElectricBicycle electricBicycle = new ElectricBicycle(
    "Электровелосипед", 3, 4, 1.5, isOperational: true);

Helmet helmet = new Helmet("Защитный шлем", 4);

ChargingStation station = new ChargingStation(
    "Зарядная станция", 5, 2);

Console.WriteLine($"Электровелосипед принят: {inventory.AddItem(electricBicycle)}");
Console.WriteLine($"Шлем принят: {inventory.AddItem(helmet)}");
Console.WriteLine($"Станция принята: {inventory.AddItem(station)}");
Console.WriteLine();
Console.WriteLine($"Предметов в инвентаре: {inventory.ItemCount}");
Console.WriteLine(
    $"Потребление за сутки: {inventory.GetTotalDailyEnergyConsumption()} кВт·ч");

Console.WriteLine("Весь инвентарь:");

foreach (IInventoryItem item in inventory.GetAllItems())
{
    Console.WriteLine($"№{item.Id}: {item.Name}");
}

Console.WriteLine("Транспорт для новичков:");

foreach (Vehicle vehicle in inventory.GetBeginnerFriendlyVehicles())
{
    Console.WriteLine(
        $"№{vehicle.Id}: {vehicle.Name}, удобство: {vehicle.BeginnerFriendliness}");
}

static IServiceCenter CreateServiceCenter(SimpleContainer container)
{
    return new ServiceCenter();
}

static InventoryService CreateInventoryService(SimpleContainer container)
{
    IServiceCenter serviceCenter = container.Resolve<IServiceCenter>();
    return new InventoryService(serviceCenter);
}
