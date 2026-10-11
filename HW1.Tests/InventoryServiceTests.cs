using VyshKat.Domain;
using VyshKat.Services;
using Xunit;

namespace VyshKat.Tests;

public class InventoryServiceTests
{
    [Fact]
    public void AddVehicle_WhenVehicleIsOperational_AddsVehicle()
    {
        // Arrange — подготовка.
        IServiceCenter serviceCenter = new ServiceCenter();
        InventoryService inventory =
            new InventoryService(serviceCenter);

        Bicycle bicycle = new Bicycle(
            "Городской велосипед", 1, 8, isOperational: true);

        // Act — действие.
        bool result = inventory.AddVehicle(bicycle);

        // Assert — проверка.
        Assert.True(result);

        IInventoryItem addedItem =
            Assert.Single(inventory.GetAllItems());

        Assert.Equal(bicycle.Id, addedItem.Id);
    }

    [Fact]
    public void AddVehicle_WhenVehicleIsBroken_PreservesExistingInventory()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        Helmet helmet = new Helmet("Шлем", 1);
        inventory.AddItem(helmet);
        Bicycle bicycle = new Bicycle("Сломанный велосипед", 2, 8, false);

        // Act.
        bool result = inventory.AddVehicle(bicycle);

        // Assert.
        Assert.False(result);
        Assert.Equal(1, inventory.ItemCount);
        IInventoryItem remainingItem = Assert.Single(inventory.GetAllItems());
        Assert.Equal(helmet.Id, remainingItem.Id);
    }

    [Fact]
    public void AddItem_WhenVehicleIsOperational_AddsVehicle()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        Bicycle bicycle = new Bicycle("Велосипед", 1, 8, true);

        // Act.
        bool result = inventory.AddItem(bicycle);

        // Assert.
        Assert.True(result);
        IInventoryItem addedItem = Assert.Single(inventory.GetAllItems());
        Assert.Equal(bicycle.Id, addedItem.Id);
    }

    [Fact]
    public void AddItem_WhenVehicleIsBroken_DoesNotBypassInspection()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        Bicycle bicycle = new Bicycle("Сломанный велосипед", 1, 8, false);

        // Act.
        bool result = inventory.AddItem(bicycle);

        // Assert.
        Assert.False(result);
        Assert.Equal(0, inventory.ItemCount);
        Assert.Empty(inventory.GetAllItems());
    }

    [Fact]
    public void AddItem_WhenHelmetIsProvided_AddsInventoryItem()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        Helmet helmet = new Helmet("Защитный шлем", 42);

        // Act.
        bool result = inventory.AddItem(helmet);

        // Assert.
        Assert.True(result);
        IInventoryItem addedItem = Assert.Single(inventory.GetAllItems());
        Assert.Equal(helmet.Id, addedItem.Id);
        Assert.Equal(helmet.Name, addedItem.Name);
    }

    [Fact]
    public void AddItem_WhenChargingStationIsProvided_AddsInventoryItem()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        ChargingStation station = new ChargingStation("Зарядная станция", 7, 2);

        // Act.
        bool result = inventory.AddItem(station);

        // Assert.
        Assert.True(result);
        IInventoryItem addedItem = Assert.Single(inventory.GetAllItems());
        Assert.Equal(station.Id, addedItem.Id);
        Assert.Equal(station.Name, addedItem.Name);
    }

    [Fact]
    public void GetTotalDailyEnergyConsumption_WhenInventoryIsEmpty_ReturnsZero()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());

        // Act.
        double total = inventory.GetTotalDailyEnergyConsumption();

        // Assert.
        Assert.Equal(0.0, total);
    }

    [Fact]
    public void GetTotalDailyEnergyConsumption_WhenInventoryIsMixed_SumsEnergyConsumers()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        inventory.AddVehicle(new Bicycle("Велосипед", 1, 8, true));
        inventory.AddVehicle(new ElectricBicycle("Электровелосипед", 2, 4, 1.5, true));
        inventory.AddItem(new Helmet("Шлем", 3));
        inventory.AddItem(new ChargingStation("Зарядная станция", 4, 2.0));

        // Act.
        double total = inventory.GetTotalDailyEnergyConsumption();

        // Assert.
        Assert.Equal(3.5, total, precision: 6);
    }

    [Fact]
    public void GetTotalDailyEnergyConsumption_WhenVehicleIsRejected_ExcludesItsConsumption()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        inventory.AddItem(new ChargingStation("Зарядная станция", 1, 2.0));
        inventory.AddVehicle(new ElectricBicycle("Неисправный", 2, 8, 10.0, false));

        // Act.
        double total = inventory.GetTotalDailyEnergyConsumption();

        // Assert.
        Assert.Equal(2.0, total, precision: 6);
    }

    [Fact]
    public void GetBeginnerFriendlyVehicles_WhenRatingIsFive_ReturnsEmpty()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        inventory.AddVehicle(new Bicycle("Велосипед", 1, 5, true));

        // Act.
        List<Vehicle> result = inventory.GetBeginnerFriendlyVehicles();

        // Assert.
        Assert.Empty(result);
    }

    [Fact]
    public void GetBeginnerFriendlyVehicles_WhenRatingIsSix_IncludesVehicle()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        Bicycle bicycle = new Bicycle("Велосипед", 1, 6, true);
        inventory.AddVehicle(bicycle);

        // Act.
        List<Vehicle> result = inventory.GetBeginnerFriendlyVehicles();

        // Assert.
        Vehicle beginnerVehicle = Assert.Single(result);
        Assert.Equal(bicycle.Id, beginnerVehicle.Id);
    }

    [Fact]
    public void GetBeginnerFriendlyVehicles_WhenInventoryIsMixed_ReturnsOnlyEligibleTransport()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        inventory.AddVehicle(new Bicycle("Сложный велосипед", 1, 5, true));
        inventory.AddVehicle(new Bicycle("Неисправный велосипед", 2, 9, false));
        ElectricBicycle suitable = new ElectricBicycle("Удобный", 3, 10, 1.5, true);
        inventory.AddVehicle(suitable);
        inventory.AddItem(new Helmet("Шлем", 4));
        inventory.AddItem(new ChargingStation("Станция", 5, 2));

        // Act.
        List<Vehicle> result = inventory.GetBeginnerFriendlyVehicles();

        // Assert.
        Vehicle beginnerVehicle = Assert.Single(result);
        Assert.Equal(suitable.Id, beginnerVehicle.Id);
    }

    [Fact]
    public void GetBeginnerFriendlyVehicles_WhenInventoryIsEmpty_ReturnsEmpty()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());

        // Act.
        List<Vehicle> result = inventory.GetBeginnerFriendlyVehicles();

        // Assert.
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllItems_WhenInventoryIsEmpty_ReturnsEmpty()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());

        // Act.
        List<IInventoryItem> result = inventory.GetAllItems();

        // Assert.
        Assert.Empty(result);
        Assert.Equal(0, inventory.ItemCount);
    }

    [Fact]
    public void GetAllItems_WhenReturnedListIsChanged_PreservesInventory()
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        Helmet helmet = new Helmet("Шлем", 1);
        inventory.AddItem(helmet);
        List<IInventoryItem> result = inventory.GetAllItems();

        // Act.
        result.Clear();
        result.Add(new Bicycle("Не прошёл осмотр", 2, 8, false));

        // Assert.
        Assert.Equal(1, inventory.ItemCount);
        IInventoryItem storedItem = Assert.Single(inventory.GetAllItems());
        Assert.Equal(helmet.Id, storedItem.Id);
    }
}
