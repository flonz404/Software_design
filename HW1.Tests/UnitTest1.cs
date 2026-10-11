using VyshKat.Domain;
using VyshKat.Infrastructure;
using VyshKat.Services;
using Xunit;

namespace VyshKat.Tests;

public class InventoryServiceTests
{
    [Fact]
    public void AddVehicle_WhenInspectionRejects_DoesNotAddVehicle()
    {
        // Подготовка.
        SimpleContainer container = new SimpleContainer();

        container.RegisterSingleton<IServiceCenter>(
            CreateRejectingServiceCenter);

        container.RegisterSingleton<InventoryService>(
            CreateInventoryService);

        InventoryService inventory =
            container.Resolve<InventoryService>();

        Bicycle bicycle = new Bicycle(
            "Исправный велосипед", 1, 8, isOperational: true);

        // Действие.
        bool result = inventory.AddVehicle(bicycle);

        // Проверка.
        Assert.False(result);
        Assert.Equal(0, inventory.ItemCount);
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
}