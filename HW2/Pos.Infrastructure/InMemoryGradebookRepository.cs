using Pos.Application;
using Pos.Domain;

namespace Pos.Infrastructure;

public sealed class InMemoryGradebookRepository : IGradebookRepository
{
    private GradebookData _data = GradebookData.FromDomain(new Gradebook(new RubricBuilder().Build()));

    public Gradebook Load()
    {
        return _data.ToDomain();
    }

    public void Save(Gradebook gradebook)
    {
        ArgumentNullException.ThrowIfNull(gradebook);
        _data = GradebookData.FromDomain(gradebook);
    }
}
