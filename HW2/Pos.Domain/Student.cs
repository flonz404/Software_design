namespace Pos.Domain;

public class Student
{
    public int Id { get; }
    public string Name { get; }

    public Student(int id, string name)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "Идентификатор должен быть положительным.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Имя студента не должно быть пустым.",
                nameof(name));
        }

        Id = id;
        Name = name.Trim();
    }
}
