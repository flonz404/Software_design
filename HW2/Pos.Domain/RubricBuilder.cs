namespace Pos.Domain;

public class RubricBuilder
{
    private readonly List<Criterion> _criteria =
        new List<Criterion>();

    public void AddCriterion(Criterion criterion)
    {
        if (criterion is null)
        {
            throw new ArgumentNullException(nameof(criterion));
        }

        foreach (Criterion existingCriterion in _criteria)
        {
            if (existingCriterion.Id == criterion.Id)
            {
                throw new ArgumentException(
                    $"Критерий с Id {criterion.Id} уже добавлен.",
                    nameof(criterion));
            }
        }

        _criteria.Add(criterion);
    }

    public Rubric Build()
    {
        return new Rubric(_criteria);
    }
}