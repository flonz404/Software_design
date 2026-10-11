using VyshKat.Domain;
using VyshKat.Services;
using VyshKat.Infrastructure;

SimpleContainer container = new SimpleContainer();

container.RegisterSingleton<IServiceCenter>(CreateServiceCenter);
container.RegisterSingleton<InventoryService>(CreateInventoryService);

InventoryService inventory = container.Resolve<InventoryService>();

Console.WriteLine("1. Пустой инвентарь");
PrintInventoryReport(inventory);

Console.WriteLine("2. Приём транспорта и вещей");
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
PrintInventoryReport(inventory);

Console.WriteLine("3. Граница для новичков и нулевое потребление");
Bicycle ratingFive = new Bicycle("Велосипед с удобством 5", 6, 5, true);
ElectricBicycle ratingSix = new ElectricBicycle(
    "Электровелосипед с удобством 6", 7, 6, 0, true);

Console.WriteLine($"Велосипед с удобством 5 принят: {inventory.AddVehicle(ratingFive)}");
Console.WriteLine($"Электровелосипед с удобством 6 принят: {inventory.AddVehicle(ratingSix)}");
PrintInventoryReport(inventory);

Console.WriteLine("4. Недопустимое удобство для новичков");
foreach (int rating in new int[] { 0, 11 })
{
    try
    {
        Bicycle invalidBicycle = new Bicycle("Ошибка удобства", 8, rating, true);
        Console.WriteLine($"Велосипед принят: {inventory.AddVehicle(invalidBicycle)}");
    }
    catch (ArgumentOutOfRangeException exception)
    {
        Console.WriteLine($"Удобство {rating}: {exception.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("5. Недопустимое потребление энергии");
foreach (double energy in new double[]
         { -1.0, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
{
    try
    {
        ElectricBicycle invalidBicycle = new ElectricBicycle(
            "Ошибка потребления", 8, 8, energy, true);
        Console.WriteLine($"Электровелосипед принят: {inventory.AddItem(invalidBicycle)}");
    }
    catch (ArgumentOutOfRangeException exception)
    {
        Console.WriteLine($"Потребление {energy}: {exception.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("6. Инвентарь после отклонения некорректных данных");
PrintInventoryReport(inventory);

static void PrintInventoryReport(InventoryService inventory)
{
    Console.WriteLine($"Предметов в инвентаре: {inventory.ItemCount}");
    Console.WriteLine(
        $"Потребление за сутки: {inventory.GetTotalDailyEnergyConsumption()} кВт·ч");

    List<IInventoryItem> items = inventory.GetAllItems();
    Console.WriteLine("Весь инвентарь:");
    if (items.Count == 0)
    {
        Console.WriteLine("Инвентарь пуст.");
    }

    foreach (IInventoryItem item in items)
    {
        Console.WriteLine($"№{item.Id}: {item.Name}");
    }

    List<Vehicle> beginnerVehicles = inventory.GetBeginnerFriendlyVehicles();
    Console.WriteLine("Транспорт для новичков:");
    if (beginnerVehicles.Count == 0)
    {
        Console.WriteLine("Подходящего транспорта нет.");
    }

    foreach (Vehicle vehicle in beginnerVehicles)
    {
        Console.WriteLine(
            $"№{vehicle.Id}: {vehicle.Name}, удобство: {vehicle.BeginnerFriendliness}");
    }

    Console.WriteLine();
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
