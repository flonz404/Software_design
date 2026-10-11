using VyshKat.Domain;
using VyshKat.Services;
using Xunit;

namespace VyshKat.Tests;

public class ServiceCenterTests
{
    [Fact]
    public void Inspect_WhenVehicleIsOperational_AcceptsVehicle()
    {
        // Arrange.
        ServiceCenter serviceCenter = new ServiceCenter();
        Bicycle bicycle = new Bicycle("Велосипед", 1, 5, true);

        // Act.
        bool result = serviceCenter.Inspect(bicycle);

        // Assert: удобство для новичков не определяет исправность.
        Assert.True(result);
    }

    [Fact]
    public void Inspect_WhenVehicleIsBroken_RejectsVehicle()
    {
        // Arrange.
        ServiceCenter serviceCenter = new ServiceCenter();
        ElectricBicycle bicycle = new ElectricBicycle("Велосипед", 1, 10, 1.5, false);

        // Act.
        bool result = serviceCenter.Inspect(bicycle);

        // Assert.
        Assert.False(result);
    }
}
