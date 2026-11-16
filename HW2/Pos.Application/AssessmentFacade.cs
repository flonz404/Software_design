using Pos.Domain;

namespace Pos.Application;

public sealed class AssessmentFacade
{
    private readonly AssessmentService _assessments;
    private readonly ReportService _reports;

    public AssessmentFacade(AssessmentService assessments, ReportService reports)
    {
        ArgumentNullException.ThrowIfNull(assessments);
        ArgumentNullException.ThrowIfNull(reports);
        _assessments = assessments;
        _reports = reports;
    }

    public void AddStudent(int id, string name)
    {
        _assessments.AddStudent(id, name);
    }

    public void AddCriterion(int id, string name, decimal maxScore, decimal weight)
    {
        _assessments.AddCriterion(id, name, maxScore, weight);
    }

    public void AssignEvaluation(int studentId, int criterionId, decimal score)
    {
        _assessments.AssignEvaluation(studentId, criterionId, score);
    }

    public void CorrectEvaluation(int studentId, int criterionId, decimal score)
    {
        _assessments.CorrectEvaluation(studentId, criterionId, score);
    }

    public void RemoveEvaluation(int studentId, int criterionId)
    {
        _assessments.RemoveEvaluation(studentId, criterionId);
    }

    public IReadOnlyCollection<Student> GetStudents()
    {
        return _assessments.GetStudents();
    }

    public IReadOnlyCollection<Criterion> GetCriteria()
    {
        return _assessments.GetCriteria();
    }

    public IReadOnlyCollection<EvaluationView> GetEvaluations(int studentId)
    {
        return _assessments.GetEvaluations(studentId);
    }

    public StudentReport GetStudentReport(int studentId, CalculationMode mode)
    {
        return _reports.GetStudentReport(studentId, mode);
    }

    public IReadOnlyCollection<StudentReport> GetReport(CalculationMode mode)
    {
        return _reports.GetReport(mode);
    }

    public void Export(CalculationMode mode, string path)
    {
        _reports.Export(mode, path);
    }
}
