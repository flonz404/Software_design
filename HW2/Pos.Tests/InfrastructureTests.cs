using Microsoft.VisualBasic.FileIO;
using Pos.Application;
using Pos.ConsoleApp;
using Pos.Domain;
using Pos.Infrastructure;
using Xunit;

namespace Pos.Tests;

public class InfrastructureTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "pos-tests-" + Guid.NewGuid());

    public InfrastructureTests()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        Directory.Delete(_directory, true);
    }

    [Fact]
    public void JsonRepository_RoundTripRestoresDomainAndRejectsInvalidCorrection()
    {
        string path = Path.Combine(_directory, "gradebook.json");
        JsonGradebookRepository repository = new JsonGradebookRepository(path);
        AssessmentFacade facade = CompositionRoot.CreateFacade(repository);
        facade.AddStudent(1, "Иван, \"Петров\"");
        facade.AddCriterion(1, "Проект", 20m, 3m);
        facade.AssignEvaluation(1, 1, 15m);
        string saved = File.ReadAllText(path);

        void InvalidCorrection() { facade.CorrectEvaluation(1, 1, 21m); }
        Assert.Throws<ArgumentOutOfRangeException>(InvalidCorrection);
        Assert.Equal(saved, File.ReadAllText(path));

        AssessmentFacade reopened = CompositionRoot.CreateFacade(new JsonGradebookRepository(path));
        Assert.Equal("Иван, \"Петров\"", Assert.Single(reopened.GetStudents()).Name);
        Assert.Equal(15m, Assert.Single(reopened.GetEvaluations(1)).Score);
        Assert.Equal(7.5m, reopened.GetStudentReport(1, CalculationMode.Strict).Result.Value);
    }

    [Theory]
    [InlineData("broken json")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"SchemaVersion\":2,\"Students\":[],\"Criteria\":[],\"Evaluations\":[]}")]
    [InlineData("{\"SchemaVersion\":1,\"Students\":[null],\"Criteria\":[],\"Evaluations\":[]}")]
    [InlineData("{\"SchemaVersion\":1,\"Students\":[],\"Criteria\":[],\"Evaluations\":[{\"StudentId\":1,\"CriterionId\":1,\"Score\":10}]}")]
    [InlineData("{\"SchemaVersion\":1,\"Students\":[],\"Criteria\":[{\"Id\":1,\"Name\":\"A\",\"MaxScore\":10,\"Weight\":-1}],\"Evaluations\":[]}")]
    public void JsonRepository_RejectsBadFileWithoutOverwritingIt(string contents)
    {
        string path = Path.Combine(_directory, "bad.json");
        File.WriteAllText(path, contents);
        AssessmentFacade facade = CompositionRoot.CreateFacade(new JsonGradebookRepository(path));
        void Act() { facade.AddStudent(2, "Новый студент"); }
        Assert.Throws<InvalidDataException>(Act);
        Assert.Equal(contents, File.ReadAllText(path));
    }

    [Fact]
    public void JsonRepository_RejectsScoreExceedingCriterionMaximum()
    {
        string path = Path.Combine(_directory, "bad-score.json");
        File.WriteAllText(path, """
        {
          "SchemaVersion": 1,
          "Students": [{"Id": 1, "Name": "Иван"}],
          "Criteria": [{"Id": 1, "Name": "Проект", "MaxScore": 10, "Weight": 1}],
          "Evaluations": [{"StudentId": 1, "CriterionId": 1, "Score": 12}]
        }
        """);
        JsonGradebookRepository repository = new JsonGradebookRepository(path);
        void Act() { repository.Load(); }
        Assert.Throws<InvalidDataException>(Act);
    }

    [Fact]
    public void InMemoryRepository_DoesNotPersistChangesWithoutSave()
    {
        InMemoryGradebookRepository repository = new InMemoryGradebookRepository();
        Gradebook loaded = repository.Load();
        loaded.AddStudent(new Student(1, "Иван"));
        Assert.Empty(repository.Load().GetStudents());
        repository.Save(loaded);
        Assert.Single(repository.Load().GetStudents());
    }

    [Fact]
    public void Export_ProducesParseableCsvWithQuotesNewlinesAndMissingScore()
    {
        string path = Path.Combine(_directory, "report.csv");
        AssessmentFacade facade = CompositionRoot.CreateFacade(new InMemoryGradebookRepository());
        string name = "Иван, \"Петров\"\nВторая строка";
        facade.AddStudent(1, name);
        facade.AddStudent(2, "Анна");
        facade.AddCriterion(1, "Проект", 20m, 3m);
        facade.AssignEvaluation(1, 1, 15m);
        facade.Export(CalculationMode.Strict, path);

        using TextFieldParser parser = new TextFieldParser(path);
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        parser.TrimWhiteSpace = false;
        Assert.Equal(6, parser.ReadFields()!.Length);
        string[] first = parser.ReadFields()!;
        Assert.Equal(name, first[1]);
        Assert.Equal("7.5", first[2]);
        string[] second = parser.ReadFields()!;
        Assert.Equal("", second[2]);
        Assert.Equal("Оценивание не завершено.", second[3]);
        Assert.True(parser.EndOfData);
    }

    [Fact]
    public void Export_RejectsJsonDestinationWithoutChangingFile()
    {
        string path = Path.Combine(_directory, "gradebook.json");
        File.WriteAllText(path, "keep");
        CsvReportExporter exporter = new CsvReportExporter();
        void Act() { exporter.Export(Array.Empty<StudentReport>(), path); }
        Assert.Throws<ArgumentException>(Act);
        Assert.Equal("keep", File.ReadAllText(path));
    }

    [Fact]
    public void Facade_UsesFreshGradesForReportAndExport()
    {
        AssessmentFacade facade = CompositionRoot.CreateFacade(new InMemoryGradebookRepository());
        facade.AddStudent(1, "Иван");
        facade.AddCriterion(1, "Проект", 10m, 1m);
        facade.AssignEvaluation(1, 1, 8m);
        Assert.Equal(8m, facade.GetStudentReport(1, CalculationMode.Strict).Result.Value);
        facade.RemoveEvaluation(1, 1);
        Assert.Null(facade.GetStudentReport(1, CalculationMode.Strict).Result.Value);
        Assert.Equal(0m, facade.GetStudentReport(1, CalculationMode.Preliminary).Result.Value);
        Assert.Null(Assert.Single(facade.GetEvaluations(1)).Score);
    }

    [Fact]
    public void InvalidCalculationMode_IsRejectedEvenForEmptyGradebook()
    {
        AssessmentFacade facade = CompositionRoot.CreateFacade(new InMemoryGradebookRepository());
        void Act() { facade.GetReport((CalculationMode)99); }
        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }
}
