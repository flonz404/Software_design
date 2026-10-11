using VyshKat.Domain;
using Xunit;

namespace VyshKat.Tests;

public class DomainValidationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Bicycle_WhenRatingIsOutsideAllowedRange_Throws(int rating)
    {
        // Arrange: именованная функция откладывает создание до проверки.
        void CreateBicycle()
        {
            _ = new Bicycle("Велосипед", 1, rating, true);
        }

        // Act + Assert: Assert.Throws сам вызывает функцию и перехватывает исключение.
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(CreateBicycle);
        Assert.Equal("beginnerFriendliness", exception.ParamName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void Bicycle_WhenRatingIsAtAllowedBoundary_CreatesVehicle(int rating)
    {
        // Arrange.
        const string name = "Велосипед";

        // Act.
        Bicycle bicycle = new Bicycle(name, 1, rating, true);

        // Assert: обе границы диапазона включены.
        Assert.Equal(rating, bicycle.BeginnerFriendliness);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ElectricBicycle_WhenEnergyIsInvalid_Throws(double energy)
    {
        // Arrange.
        void CreateBicycle()
        {
            _ = new ElectricBicycle("Электровелосипед", 1, 8, energy, true);
        }

        // Act + Assert.
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(CreateBicycle);
        Assert.Equal("dailyEnergyConsumption", exception.ParamName);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.5)]
    public void ElectricBicycle_WhenEnergyIsValid_CreatesVehicle(double energy)
    {
        // Arrange.
        const string name = "Электровелосипед";

        // Act.
        ElectricBicycle bicycle = new ElectricBicycle(name, 1, 8, energy, true);

        // Assert: ноль и дробное потребление допустимы.
        Assert.Equal(energy, bicycle.DailyEnergyConsumption);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ChargingStation_WhenEnergyIsInvalid_Throws(double energy)
    {
        // Arrange.
        void CreateStation()
        {
            _ = new ChargingStation("Станция", 1, energy);
        }

        // Act + Assert.
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(CreateStation);
        Assert.Equal("dailyEnergyConsumption", exception.ParamName);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(2.5)]
    public void ChargingStation_WhenEnergyIsValid_CreatesStation(double energy)
    {
        // Arrange.
        const string name = "Станция";

        // Act.
        ChargingStation station = new ChargingStation(name, 1, energy);

        // Assert.
        Assert.Equal(energy, station.DailyEnergyConsumption);
    }
}
