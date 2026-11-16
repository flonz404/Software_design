using Pos.Domain;

namespace Pos.Application;

public sealed class AssessmentService
{
    private readonly IGradebookRepository _repository;

    public AssessmentService(IGradebookRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public void AddStudent(int id, string name)
    {
        Gradebook gradebook = _repository.Load();
        gradebook.AddStudent(new Student(id, name));
        _repository.Save(gradebook);
    }

    public void AddCriterion(int id, string name, decimal maxScore, decimal weight)
    {
        Gradebook gradebook = _repository.Load();
        gradebook.AddCriterion(new Criterion(id, name, maxScore, new Weight(weight)));
        _repository.Save(gradebook);
    }

    public void AssignEvaluation(int studentId, int criterionId, decimal score)
    {
        Gradebook gradebook = _repository.Load();
        gradebook.AssignEvaluation(studentId, criterionId, score);
        _repository.Save(gradebook);
    }

    public void CorrectEvaluation(int studentId, int criterionId, decimal score)
    {
        Gradebook gradebook = _repository.Load();
        gradebook.CorrectEvaluation(studentId, criterionId, score);
        _repository.Save(gradebook);
    }

    public void RemoveEvaluation(int studentId, int criterionId)
    {
        Gradebook gradebook = _repository.Load();
        gradebook.RemoveEvaluation(studentId, criterionId);
        _repository.Save(gradebook);
    }

    public IReadOnlyCollection<Student> GetStudents()
    {
        return _repository.Load().GetStudents();
    }

    public IReadOnlyCollection<Criterion> GetCriteria()
    {
        return _repository.Load().GetCriteria();
    }

    public IReadOnlyCollection<EvaluationView> GetEvaluations(int studentId)
    {
        Gradebook gradebook = _repository.Load();
        Dictionary<int, decimal> scores = new Dictionary<int, decimal>();
        foreach (Evaluation evaluation in gradebook.GetEvaluations(studentId))
        {
            scores.Add(evaluation.CriterionId, evaluation.Score);
        }

        List<EvaluationView> result = new List<EvaluationView>();
        foreach (Criterion criterion in gradebook.GetCriteria())
        {
            decimal? score = null;
            if (scores.TryGetValue(criterion.Id, out decimal value))
            {
                score = value;
            }
            result.Add(new EvaluationView(criterion.Id, criterion.Name,
                criterion.MaxScore, criterion.Weight.Value, score));
        }
        return result.AsReadOnly();
    }
}
