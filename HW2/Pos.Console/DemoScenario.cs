using Pos.Application;

namespace Pos.ConsoleApp;

public static class DemoScenario
{
    public static void Run(AssessmentFacade facade, TextWriter output, string exportPath)
    {
        output.WriteLine("Демонстрация на отдельной ведомости в памяти.");
        facade.AddStudent(1, "Иван Петров");
        facade.AddStudent(2, "Анна Смирнова");
        facade.AddCriterion(1, "Домашняя работа", 10m, 1m);
        facade.AddCriterion(2, "Проект", 20m, 3m);
        facade.AssignEvaluation(1, 1, 8m);
        facade.AssignEvaluation(1, 2, 15m);
        facade.AssignEvaluation(2, 1, 10m);

        ConsoleMenu menu = new ConsoleMenu(facade, TextReader.Null, output);
        output.WriteLine("Окончательный итог: у Ивана 7,625, у Анны пока отсутствует.");
        menu.ShowReport(CalculationMode.Strict);
        output.WriteLine("Предварительный итог: у Анны 2,5.");
        menu.ShowReport(CalculationMode.Preliminary);

        try
        {
            facade.CorrectEvaluation(1, 2, 21m);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            output.WriteLine($"Ожидаемый отказ: {exception.Message}");
        }
        output.WriteLine("После недопустимого исправления прежняя оценка сохранена:");
        menu.ShowReport(CalculationMode.Strict);

        facade.CorrectEvaluation(1, 2, 20m);
        output.WriteLine("После исправления проекта на 20: итог Ивана 9,5.");
        menu.ShowReport(CalculationMode.Strict);
        facade.RemoveEvaluation(1, 2);
        output.WriteLine("После отмены оценки строгий итог снова недоступен:");
        menu.ShowReport(CalculationMode.Strict);
        facade.AssignEvaluation(1, 2, 15m);
        facade.AssignEvaluation(2, 2, 20m);
        facade.Export(CalculationMode.Strict, exportPath);
        output.WriteLine($"Демо CSV: {Path.GetFullPath(exportPath)}");
    }
}
