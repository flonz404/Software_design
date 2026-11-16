namespace Pos.Domain;

public class Gradebook
{
    private Rubric _rubric;

    private readonly Dictionary<int, Student> _students =
        new Dictionary<int, Student>();

    private readonly Dictionary<
        (int StudentId, int CriterionId),
        Evaluation> _evaluations =
        new Dictionary<(int StudentId, int CriterionId), Evaluation>();

    public Gradebook(Rubric rubric)
    {
        if (rubric is null)
        {
            throw new ArgumentNullException(nameof(rubric));
        }

        _rubric = rubric;
    }

    public void AddStudent(Student student)
    {
        if (student is null)
        {
            throw new ArgumentNullException(nameof(student));
        }

        if (_students.ContainsKey(student.Id))
        {
            throw new ArgumentException(
                $"Студент с Id {student.Id} уже добавлен.",
                nameof(student));
        }

        _students.Add(student.Id, student);
    }

    public void AssignEvaluation(int studentId, int criterionId, decimal score)
    {
        Student student = GetStudent(studentId);
        Criterion criterion = _rubric.GetCriterion(criterionId);
        (int StudentId, int CriterionId) key = (studentId, criterionId);

        if (_evaluations.ContainsKey(key))
        {
            throw new InvalidOperationException(
                "Оценка уже выставлена. Используйте исправление.");
        }

        Evaluation evaluation = new Evaluation(student, criterion, score);
        _evaluations.Add(key, evaluation);
    }

    public Student GetStudent(int studentId)
    {
        if (!_students.TryGetValue(studentId, out Student? student))
        {
            throw new ArgumentException(
                $"Студент с Id {studentId} отсутствует в ведомости.",
                nameof(studentId));
        }

        return student;
    }

    public void CorrectEvaluation(
        int studentId,
        int criterionId,
        decimal score)
    {
        if (!_students.TryGetValue(studentId, out Student? student))
        {
            throw new ArgumentException(
                $"Студент с Id {studentId} отсутствует в ведомости.",
                nameof(studentId));
        }

        Criterion criterion = _rubric.GetCriterion(criterionId);

        (int StudentId, int CriterionId) key = (studentId, criterionId);

        if (!_evaluations.ContainsKey(key))
        {
            throw new InvalidOperationException(
                "Оценка ещё не выставлена. Исправлять нечего.");
        }

        Evaluation replacement = new Evaluation(student, criterion, score);

        _evaluations[key] = replacement;
    }
    public void RemoveEvaluation(int studentId, int criterionId)
    {
        if (!_students.ContainsKey(studentId))
        {
            throw new ArgumentException(
                $"Студент с Id {studentId} отсутствует в ведомости.",
                nameof(studentId));
        }

        _rubric.GetCriterion(criterionId);

        (int StudentId, int CriterionId) key = (studentId, criterionId);

        if (!_evaluations.Remove(key))
        {
            throw new InvalidOperationException(
                "Оценка не выставлена. Отменять нечего.");
        }
    }

    public IReadOnlyCollection<Student> GetStudents()
    {
        return _students.Values.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Evaluation> GetEvaluations(int studentId)
    {
        if (!_students.ContainsKey(studentId))
        {
            throw new ArgumentException(
                $"Студент с Id {studentId} отсутствует в ведомости.",
                nameof(studentId));
        }

        List<Evaluation> result = new List<Evaluation>();

        foreach (Evaluation evaluation in _evaluations.Values)
        {
            if (evaluation.StudentId == studentId)
            {
                result.Add(evaluation);
            }
        }

        return result.AsReadOnly();
    }

    public IReadOnlyCollection<Criterion> GetCriteria()
    {
        return _rubric.GetCriteria();
    }

    public void AddCriterion(Criterion criterion)
    {
        RubricBuilder builder = new RubricBuilder();

        foreach (Criterion existingCriterion in _rubric.GetCriteria())
        {
            builder.AddCriterion(existingCriterion);
        }

        builder.AddCriterion(criterion);

        Rubric replacement = builder.Build();

        _rubric = replacement;
    }
}
