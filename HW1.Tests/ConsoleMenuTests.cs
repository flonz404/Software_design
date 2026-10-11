using System.Globalization;
using System.Reflection;
using VyshKat.Services;

namespace VyshKat.Tests;

[Collection("Console demonstration")]
public class ConsoleMenuTests
{
    [Fact]
    public void Run_WhenItemsAreEntered_ReportsAcceptedItemsAndRejectsBrokenVehicle()
    {
        // Arrange: команды, параметры четырёх типов и неисправного велосипеда.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        string input = "5\n1\nГородской\n1\n8\n1\n" +
                       "2\nЭлектро\n2\n5\n1\n1,5\n" +
                       "3\nШлем\n3\n4\nСтанция\n4\n2.0\n" +
                       "1\nСломанный\n5\n9\n0\n5\n0\n";

        // Act.
        string output = RunMenu(inventory, input);

        // Assert: проверяем итоговое состояние и видимый результат сценария.
        Assert.Equal(4, inventory.ItemCount);
        Assert.Equal(3.5, inventory.GetTotalDailyEnergyConsumption(), 6);
        Assert.Equal("Городской", Assert.Single(inventory.GetBeginnerFriendlyVehicles()).Name);
        Assert.Contains("Инвентарь пуст.", output);
        Assert.Contains("Предметов в инвентаре: 4", output);
        Assert.Contains("№4: Станция", output);
        Assert.Contains("Транспорт не принят", output);
        Assert.DoesNotContain("№5:", output);
        Assert.Contains("Работа завершена.", output);
    }

    [Fact]
    public void Run_WhenInputIsInvalid_RepeatsPromptAndAddsCorrectedItem()
    {
        // Arrange: неверная команда, пустое имя, неверные числа и затем исправления.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        string input = "abc\n9\n2\n \nЭлектро\n0\n999999999999999\n1\n" +
                       "0\n11\n6\nда\n1\n-1\nNaN\nInfinity\n1e999\nтекст\n0\n0\n";

        // Act.
        string output = RunMenu(inventory, input);

        // Assert: ошибки не привели к добавлению лишних предметов или остановке.
        Assert.Equal("Электро", Assert.Single(inventory.GetAllItems()).Name);
        Assert.Single(inventory.GetBeginnerFriendlyVehicles());
        Assert.Equal(0, inventory.GetTotalDailyEnergyConsumption());
        Assert.Contains("Название не должно быть пустым.", output);
        Assert.Contains("Введите целое число от 1 до 10.", output);
        Assert.Contains("Введите конечное неотрицательное число.", output);
        Assert.Contains("Работа завершена.", output);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1\n")]
    [InlineData("1\nВелосипед\n1\n8\n")]
    public void Run_WhenInputEnds_ExitsWithoutAddingIncompleteItem(string input)
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());

        // Act.
        string output = RunMenu(inventory, input);

        // Assert.
        Assert.Empty(inventory.GetAllItems());
        Assert.Contains("Ввод завершён. Работа завершена.", output);
    }

    [Theory]
    [InlineData("ru-RU")]
    [InlineData("en-US")]
    public void Run_WhenEnergyUsesCommaOrPoint_AcceptsBothInDifferentCultures(string cultureName)
    {
        // Arrange.
        InventoryService inventory = new InventoryService(new ServiceCenter());
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        // Act: настройки окружения восстанавливаются даже при ошибке.
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            RunMenu(inventory, "4\nСтанция 1\n1\n1,5\n4\nСтанция 2\n2\n2.5\n0\n");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }

        // Assert.
        Assert.Equal(2, inventory.ItemCount);
        Assert.Equal(4, inventory.GetTotalDailyEnergyConsumption());
    }

    [Fact]
    public void Main_WhenStartedWithoutArguments_OpensMenuWithEmptyInventory()
    {
        // Arrange.
        MethodInfo? entryPoint = typeof(InventoryService).Assembly.EntryPoint;
        Assert.NotNull(entryPoint);
        using StringReader input = new StringReader("5\n0\n");
        using StringWriter output = new StringWriter();
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;

        // Act.
        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            entryPoint.Invoke(null, new object[] { Array.Empty<string>() });
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }

        // Assert.
        Assert.Contains("Выберите действие:", output.ToString());
        Assert.Contains("Предметов в инвентаре: 0", output.ToString());
        Assert.Contains("Работа завершена.", output.ToString());
        Assert.DoesNotContain("Городской велосипед", output.ToString());
    }

    private static string RunMenu(InventoryService inventory, string commands)
    {
        using StringReader input = new StringReader(commands);
        using StringWriter output = new StringWriter();
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            ConsoleMenu menu = new ConsoleMenu(inventory);
            menu.Run();
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }

        return output.ToString();
    }
}
