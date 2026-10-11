using VyshKat.Domain;
using VyshKat.Infrastructure;
using VyshKat.Services;
using Xunit;

namespace VyshKat.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddVehicle_WhenInjectedServiceRejects_DoesNotAddVehicle()
    {
        // Arrange.
        SimpleContainer container = new SimpleContainer();

        container.RegisterSingleton<IServiceCenter>(
            CreateRejectingServiceCenter);

        container.RegisterSingleton<InventoryService>(
            CreateInventoryService);

        InventoryService inventory =
            container.Resolve<InventoryService>();

        Bicycle bicycle = new Bicycle(
            "Исправный велосипед", 1, 8, isOperational: true);

        // Act.
        bool result = inventory.AddVehicle(bicycle);

        // Assert.
        Assert.False(result);
        Assert.Empty(inventory.GetAllItems());
    }

    [Fact]
    public void AddItem_WhenInjectedServiceRejects_DoesNotBypassInspection()
    {
        // Arrange.
        SimpleContainer container = new SimpleContainer();
        container.RegisterSingleton<IServiceCenter>(CreateRejectingServiceCenter);
        container.RegisterSingleton<InventoryService>(CreateInventoryService);
        InventoryService inventory = container.Resolve<InventoryService>();
        Bicycle bicycle = new Bicycle("Исправный велосипед", 1, 8, true);

        // Act.
        bool result = inventory.AddItem(bicycle);

        // Assert.
        Assert.False(result);
        Assert.Empty(inventory.GetAllItems());
    }

    [Fact]
    public void AddVehicle_WhenInjectedServiceAccepts_UsesItsDecision()
    {
        // Arrange: заглушка разрешает приём независимо от состояния техники.
        SimpleContainer container = new SimpleContainer();
        container.RegisterSingleton<IServiceCenter>(CreateAcceptingServiceCenter);
        container.RegisterSingleton<InventoryService>(CreateInventoryService);
        InventoryService inventory = container.Resolve<InventoryService>();
        Bicycle bicycle = new Bicycle("Неисправный велосипед", 1, 8, false);

        // Act.
        bool result = inventory.AddVehicle(bicycle);

        // Assert.
        Assert.True(result);
        IInventoryItem addedItem = Assert.Single(inventory.GetAllItems());
        Assert.Equal(bicycle.Id, addedItem.Id);
    }

    [Fact]
    public void AddItem_WhenInspectionRejectsTransport_StillAcceptsThings()
    {
        // Arrange.
        SimpleContainer container = new SimpleContainer();
        container.RegisterSingleton<IServiceCenter>(CreateRejectingServiceCenter);
        container.RegisterSingleton<InventoryService>(CreateInventoryService);
        InventoryService inventory = container.Resolve<InventoryService>();
        Helmet helmet = new Helmet("Шлем", 1);

        // Act.
        bool result = inventory.AddItem(helmet);

        // Assert.
        Assert.True(result);
        IInventoryItem addedItem = Assert.Single(inventory.GetAllItems());
        Assert.Equal(helmet.Id, addedItem.Id);
    }

    private static IServiceCenter CreateAcceptingServiceCenter(
        SimpleContainer container)
    {
        return new AcceptingServiceCenter();
    }

    private static IServiceCenter CreateRejectingServiceCenter(
        SimpleContainer container)
    {
        return new RejectingServiceCenter();
    }

    private static InventoryService CreateInventoryService(
        SimpleContainer container)
    {
        return new InventoryService(
            container.Resolve<IServiceCenter>());
    }

    private class RejectingServiceCenter : IServiceCenter
    {
        public bool Inspect(Vehicle vehicle)
        {
            return false;
        }
    }

    private class AcceptingServiceCenter : IServiceCenter
    {
        public bool Inspect(Vehicle vehicle)
        {
            return true;
        }
    }
}
