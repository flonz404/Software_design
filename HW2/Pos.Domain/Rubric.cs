namespace Pos.Domain;

public class Rubric
{
    private readonly Dictionary<int, Criterion> _criteria =
        new Dictionary<int, Criterion>();

    public Rubric(IEnumerable<Criterion> criteria)
    {
        if (criteria is null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        foreach (Criterion criterion in criteria)
        {
            if (criterion is null)
            {
                throw new ArgumentException(
                    "Критерий не должен быть null.",
                    nameof(criteria));
            }

            if (_criteria.ContainsKey(criterion.Id))
            {
                throw new ArgumentException(
                    $"Критерий с Id {criterion.Id} уже существует.",
                    nameof(criteria));
            }

            _criteria.Add(criterion.Id, criterion);
        }
    }

    public Criterion GetCriterion(int criterionId)
    {
        if (!_criteria.TryGetValue(criterionId, out Criterion? criterion))
        {
            throw new ArgumentException(
                $"Критерий с Id {criterionId} отсутствует в рубрикаторе.",
                nameof(criterionId));
        }

        return criterion;
    }
    
    public IReadOnlyCollection<Criterion> GetCriteria()
    {
        return _criteria.Values.ToList().AsReadOnly();
    }
}