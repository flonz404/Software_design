using Pos.Domain;

namespace Pos.Tests;

internal static class TestData
{
    public static Gradebook CreateGradebook()
    {
        RubricBuilder builder = new RubricBuilder();
        builder.AddCriterion(new Criterion(1, "Домашняя работа", 10m, new Weight(1m)));
        builder.AddCriterion(new Criterion(2, "Проект", 20m, new Weight(3m)));
        Gradebook gradebook = new Gradebook(builder.Build());
        gradebook.AddStudent(new Student(1, "Иван Петров"));
        return gradebook;
    }
}
