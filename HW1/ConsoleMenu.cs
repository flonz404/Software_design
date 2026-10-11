using System.Globalization;
using VyshKat.Domain;
using VyshKat.Services;

namespace VyshKat;

public class ConsoleMenu
{
    private readonly InventoryService _inventory;

    public ConsoleMenu(InventoryService inventory)
    {
        _inventory = inventory;
    }

    public void Run()
    {
        Console.WriteLine("ВышКат. Данные хранятся только до выхода из программы.");
        try
        {
            while (true)
            {
                Console.WriteLine("\n1 - Добавить велосипед");
                Console.WriteLine("2 - Добавить электровелосипед");
                Console.WriteLine("3 - Добавить шлем");
                Console.WriteLine("4 - Добавить зарядную станцию");
                Console.WriteLine("5 - Показать инвентарь, энергопотребление и транспорт для новичков");
                Console.WriteLine("0 - Выход");

                int command = ReadInteger("Выберите действие: ", 0, 5);
                if (command == 0)
                {
                    Console.WriteLine("Работа завершена.");
                    return;
                }

                if (command == 5)
                {
                    PrintReport();
                }
                else
                {
                    AddItem(command);
                }
            }
        }
        catch (EndOfStreamException)
        {
            // Ctrl+D завершает ввод
            Console.WriteLine("\nВвод завершён. Работа завершена.");
        }
    }

    private void AddItem(int command)
    {
        string name = ReadName();
        int id = ReadInteger("Инвентарный номер (от 1): ", 1, int.MaxValue);
        IInventoryItem item;

        if (command == 1 || command == 2)
        {
            int friendliness = ReadInteger("Удобство для новичков (1-10): ", 1, 10);
            bool operational = ReadInteger("Исправен? 1 - да, 0 - нет: ", 0, 1) == 1;
            if (command == 1)
            {
                item = new Bicycle(name, id, friendliness, operational);
            }
            else
            {
                double energy = ReadEnergy();
                item = new ElectricBicycle(name, id, friendliness, energy, operational);
            }
        }
        else if (command == 3)
        {
            item = new Helmet(name, id);
        }
        else
        {
            double energy = ReadEnergy();
            item = new ChargingStation(name, id, energy);
        }

        if (_inventory.AddItem(item))
        {
            Console.WriteLine($"Предмет №{item.Id} «{item.Name}» добавлен.");
        }
        else
        {
            Console.WriteLine("Транспорт не принят: техосмотр не пройден.");
        }
    }

    private static string ReadInput(string prompt)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (input is null)
        {
            throw new EndOfStreamException();
        }

        return input.Trim();
    }

    private static string ReadName()
    {
        while (true)
        {
            string name = ReadInput("Название: ");
            if (name.Length > 0)
            {
                return name;
            }

            Console.WriteLine("Название не должно быть пустым.");
        }
    }

    private static int ReadInteger(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            string input = ReadInput(prompt);
            if (int.TryParse(input, out int value) && value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Введите целое число от {minimum} до {maximum}.");
        }
    }

    private static double ReadEnergy()
    {
        while (true)
        {
            string input = ReadInput("Потребление за сутки, кВт·ч (например, 1,5): ");
            input = input.Replace(',', '.');
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out double energy) && double.IsFinite(energy) && energy >= 0)
            {
                return energy;
            }

            Console.WriteLine("Введите конечное неотрицательное число.");
        }
    }

    public void PrintReport()
    {
        Console.WriteLine($"Предметов в инвентаре: {_inventory.ItemCount}");
        Console.WriteLine(
            $"Потребление за сутки: {_inventory.GetTotalDailyEnergyConsumption()} кВт·ч");

        List<IInventoryItem> items = _inventory.GetAllItems();
        Console.WriteLine("Весь инвентарь:");
        if (items.Count == 0)
        {
            Console.WriteLine("Инвентарь пуст.");
        }

        foreach (IInventoryItem item in items)
        {
            Console.WriteLine($"№{item.Id}: {item.Name}");
        }

        List<Vehicle> beginnerVehicles = _inventory.GetBeginnerFriendlyVehicles();
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
}
