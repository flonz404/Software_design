using Pos.Domain;

namespace Pos.Application;

public sealed class ReportService
{
    private readonly IGradebookRepository _repository;
    private readonly FinalScoreCalculator _calculator;
    private readonly IScoringStrategy _strict;
    private readonly IScoringStrategy _preliminary;
    private readonly IReportExporter _exporter;

    public ReportService(IGradebookRepository repository, FinalScoreCalculator calculator,
        StrictScoringStrategy strict, PreliminaryScoringStrategy preliminary,
        IReportExporter exporter)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(calculator);
        ArgumentNullException.ThrowIfNull(strict);
        ArgumentNullException.ThrowIfNull(preliminary);
        ArgumentNullException.ThrowIfNull(exporter);
        _repository = repository;
        _calculator = calculator;
        _strict = strict;
        _preliminary = preliminary;
        _exporter = exporter;
    }

    public StudentReport GetStudentReport(int studentId, CalculationMode mode)
    {
        IScoringStrategy strategy = GetStrategy(mode);
        Gradebook gradebook = _repository.Load();
        Student student = gradebook.GetStudent(studentId);
        return new StudentReport(student.Id, student.Name,
            _calculator.Calculate(gradebook, studentId, strategy));
    }

    public IReadOnlyCollection<StudentReport> GetReport(CalculationMode mode)
    {
        IScoringStrategy strategy = GetStrategy(mode);
        Gradebook gradebook = _repository.Load();
        List<StudentReport> reports = new List<StudentReport>();
        foreach (Student student in gradebook.GetStudents())
        {
            reports.Add(new StudentReport(student.Id, student.Name,
                _calculator.Calculate(gradebook, student.Id, strategy)));
        }
        return reports.AsReadOnly();
    }

    public void Export(CalculationMode mode, string path)
    {
        _exporter.Export(GetReport(mode), path);
    }

    private IScoringStrategy GetStrategy(CalculationMode mode)
    {
        switch (mode)
        {
            case CalculationMode.Strict:
                return _strict;
            case CalculationMode.Preliminary:
                return _preliminary;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), "Неизвестный режим расчёта.");
        }
    }
}
