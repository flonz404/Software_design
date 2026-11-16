using Pos.Application;
using Pos.ConsoleApp;
using Pos.Infrastructure;
using Xunit;

namespace Pos.Tests;

public class ConsoleTests
{
    [Fact]
    public void Menu_ReportsCorruptedStorageAndRemainsUsable()
    {
        string path = Path.Combine(Path.GetTempPath(), $"pos-corrupt-{Guid.NewGuid()}.json");
        try
        {
            File.WriteAllText(path, "broken");
            AssessmentFacade facade = CompositionRoot.CreateFacade(new JsonGradebookRepository(path));
            using StringReader reader = new StringReader("2\n0\n");
            using StringWriter output = new StringWriter();
            new ConsoleMenu(facade, reader, output).Run();
            Assert.Contains("Файл ведомости повреждён", output.ToString());
            Assert.Contains("Работа завершена", output.ToString());
            Assert.Equal("broken", File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Menu_CompletesAssessmentWorkflowAndRecoversAfterDomainError()
    {
        AssessmentFacade facade = CompositionRoot.CreateFacade(new InMemoryGradebookRepository());
        string input = string.Join('\n', new[]
        {
            "1", "1", "Иван", "2",
            "3", "1", "Проект", "10", "1", "4",
            "5", "1", "1", "8", "8", "1", "9", "1", "1",
            "6", "1", "1", "12",
            "6", "1", "1", "9", "10", "1",
            "7", "1", "1", "9", "1", "1", "0"
        });
        using StringReader reader = new StringReader(input);
        using StringWriter output = new StringWriter();
        new ConsoleMenu(facade, reader, output).Run();
        string text = output.ToString();
        Assert.Contains("Оценка выставлена.", text);
        Assert.Contains("Ошибка:", text);
        Assert.Contains("Оценка исправлена.", text);
        Assert.Contains("Оценка отменена.", text);
        Assert.Contains("Оценивание не завершено.", text);
        Assert.Contains("Работа завершена.", text);
        Assert.Null(Assert.Single(facade.GetEvaluations(1)).Score);
    }

    [Fact]
    public void Menu_RetriesInvalidInputAndAcceptsCommaAndDot()
    {
        AssessmentFacade facade = CompositionRoot.CreateFacade(new InMemoryGradebookRepository());
        string input = "wrong\n99\n3\n0\n1\n \nПроект\nNaN\n0\n10,5\n-1\n1.5\n0\n";
        using StringReader reader = new StringReader(input);
        using StringWriter output = new StringWriter();
        new ConsoleMenu(facade, reader, output).Run();
        Assert.Equal(10.5m, Assert.Single(facade.GetCriteria()).MaxScore);
        Assert.Equal(1.5m, Assert.Single(facade.GetCriteria()).Weight.Value);
        Assert.Contains("Введите целое число", output.ToString());
        Assert.Contains("Введите непустое значение", output.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("1\n1\n")]
    [InlineData("3\n1\nПроект\n10\n")]
    public void EndOfInput_DoesNotPerformIncompleteOperation(string input)
    {
        AssessmentFacade facade = CompositionRoot.CreateFacade(new InMemoryGradebookRepository());
        using StringReader reader = new StringReader(input);
        using StringWriter output = new StringWriter();
        new ConsoleMenu(facade, reader, output).Run();
        Assert.Empty(facade.GetStudents());
        Assert.Empty(facade.GetCriteria());
        Assert.Contains("Ввод завершён", output.ToString());
    }

    [Fact]
    public void ExportCancellation_PreservesExistingFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"pos-existing-{Guid.NewGuid()}.csv");
        try
        {
            File.WriteAllText(path, "keep");
            AssessmentFacade facade = CompositionRoot.CreateFacade(new InMemoryGradebookRepository());
            using StringReader reader = new StringReader($"11\n1\n{path}\n0\n0\n");
            using StringWriter output = new StringWriter();
            new ConsoleMenu(facade, reader, output).Run();
            Assert.Equal("keep", File.ReadAllText(path));
            Assert.Contains("Экспорт отменён", output.ToString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Demo_ExportsFinalResultsAndDoesNotUsePersistentRepository()
    {
        string path = Path.Combine(Path.GetTempPath(), $"pos-demo-{Guid.NewGuid()}.csv");
        try
        {
            AssessmentFacade facade = CompositionRoot.CreateFacade(new InMemoryGradebookRepository());
            using StringWriter output = new StringWriter();
            DemoScenario.Run(facade, output, path);
            Assert.Contains("Ожидаемый отказ", output.ToString());
            Assert.Equal(2, facade.GetReport(CalculationMode.Strict).Count);
            Assert.Contains("7.625", File.ReadAllText(path));
            Assert.Equal(10m, facade.GetStudentReport(2, CalculationMode.Strict).Result.Value);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
