using Pos.Domain;

namespace Pos.Application;

public interface IGradebookRepository
{
    Gradebook Load();
    void Save(Gradebook gradebook);
}
