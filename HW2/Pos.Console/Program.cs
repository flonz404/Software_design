using System.Text;
using Pos.Application;
using Pos.Infrastructure;

namespace Pos.ConsoleApp;

public static class Program
{
    public static int Main(string[] args)
    {
        System.Console.InputEncoding = Encoding.UTF8;
        System.Console.OutputEncoding = Encoding.UTF8;
        try
        {
            if (args.Length == 1 && args[0] == "--help")
            {
                PrintUsage();
                return 0;
            }
            if (args.Length >= 1 && args[0] == "--demo" && args.Length <= 2)
            {
                string exportPath = Path.Combine("data", $"demo-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.csv");
                if (args.Length == 2)
                {
                    exportPath = args[1];
                }
                if (File.Exists(exportPath))
                {
                    throw new IOException("Демо-файл уже существует. Укажите новое имя.");
                }
                AssessmentFacade demoFacade = CompositionRoot.CreateFacade(new InMemoryGradebookRepository());
                DemoScenario.Run(demoFacade, System.Console.Out, exportPath);
                return 0;
            }

            string dataPath = Path.Combine("data", "gradebook.json");
            if (args.Length == 2 && args[0] == "--data")
            {
                dataPath = args[1];
            }
            else if (args.Length != 0)
            {
                PrintUsage();
                return 1;
            }
            if (!string.Equals(Path.GetExtension(dataPath), ".json", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Хранилище должно иметь расширение .json.");
            }

            JsonGradebookRepository repository = new JsonGradebookRepository(dataPath);
            repository.Load();
            AssessmentFacade facade = CompositionRoot.CreateFacade(repository);
            System.Console.WriteLine($"Данные автоматически сохраняются: {Path.GetFullPath(dataPath)}");
            ConsoleMenu menu = new ConsoleMenu(facade, System.Console.In, System.Console.Out);
            menu.Run();
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidOperationException or IOException or InvalidDataException or UnauthorizedAccessException)
        {
            System.Console.Error.WriteLine($"Ошибка: {exception.Message}");
            return 1;
        }
    }

    private static void PrintUsage()
    {
        System.Console.WriteLine("ПОС: без аргументов - меню; --data <файл.json> - выбрать хранилище;");
        System.Console.WriteLine("--demo [новый-файл.csv] - демонстрация; --help - справка.");
    }
}
