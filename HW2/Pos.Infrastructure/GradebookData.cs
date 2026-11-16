using Pos.Domain;

namespace Pos.Infrastructure;

internal sealed class GradebookData
{
    public required int SchemaVersion { get; set; }
    public required List<StudentData> Students { get; set; }
    public required List<CriterionData> Criteria { get; set; }
    public required List<EvaluationData> Evaluations { get; set; }

    public static GradebookData FromDomain(Gradebook gradebook)
    {
        GradebookData data = new GradebookData
        {
            SchemaVersion = 1,
            Students = new List<StudentData>(),
            Criteria = new List<CriterionData>(),
            Evaluations = new List<EvaluationData>()
        };
        foreach (Criterion criterion in gradebook.GetCriteria())
        {
            data.Criteria.Add(new CriterionData
            {
                Id = criterion.Id, Name = criterion.Name,
                MaxScore = criterion.MaxScore, Weight = criterion.Weight.Value
            });
        }
        foreach (Student student in gradebook.GetStudents())
        {
            data.Students.Add(new StudentData { Id = student.Id, Name = student.Name });
            foreach (Evaluation evaluation in gradebook.GetEvaluations(student.Id))
            {
                data.Evaluations.Add(new EvaluationData
                {
                    StudentId = evaluation.StudentId,
                    CriterionId = evaluation.CriterionId, Score = evaluation.Score
                });
            }
        }
        return data;
    }

    public Gradebook ToDomain()
    {
        if (SchemaVersion != 1 || Students is null || Criteria is null || Evaluations is null)
        {
            throw new InvalidDataException("Неверная структура или версия файла ведомости.");
        }

        RubricBuilder builder = new RubricBuilder();
        foreach (CriterionData criterion in Criteria)
        {
            ArgumentNullException.ThrowIfNull(criterion);
            builder.AddCriterion(new Criterion(criterion.Id, criterion.Name,
                criterion.MaxScore, new Weight(criterion.Weight)));
        }
        Gradebook gradebook = new Gradebook(builder.Build());
        foreach (StudentData student in Students)
        {
            ArgumentNullException.ThrowIfNull(student);
            gradebook.AddStudent(new Student(student.Id, student.Name));
        }
        foreach (EvaluationData evaluation in Evaluations)
        {
            ArgumentNullException.ThrowIfNull(evaluation);
            gradebook.AssignEvaluation(evaluation.StudentId, evaluation.CriterionId, evaluation.Score);
        }
        return gradebook;
    }
}

internal sealed class StudentData
{
    public required int Id { get; set; }
    public required string Name { get; set; }
}

internal sealed class CriterionData
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required decimal MaxScore { get; set; }
    public required decimal Weight { get; set; }
}

internal sealed class EvaluationData
{
    public required int StudentId { get; set; }
    public required int CriterionId { get; set; }
    public required decimal Score { get; set; }
}
