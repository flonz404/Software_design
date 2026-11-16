using System.Text.Json;
using Pos.Application;
using Pos.Domain;

namespace Pos.Infrastructure;

public sealed class JsonGradebookRepository : IGradebookRepository
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true };

    public JsonGradebookRepository(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }

    public Gradebook Load()
    {
        if (Directory.Exists(_path))
        {
            throw new IOException("Вместо файла ведомости указан каталог.");
        }
        if (!File.Exists(_path))
        {
            return new Gradebook(new RubricBuilder().Build());
        }

        string json = File.ReadAllText(_path);
        try
        {
            GradebookData? data = JsonSerializer.Deserialize<GradebookData>(json, _options);
            if (data is null)
            {
                throw new InvalidDataException("Файл ведомости содержит null.");
            }
            return data.ToDomain();
        }
        catch (Exception exception) when (exception is JsonException
            or ArgumentException or InvalidOperationException)
        {
            throw new InvalidDataException(
                $"Файл ведомости повреждён: {exception.Message}", exception);
        }
    }

    public void Save(Gradebook gradebook)
    {
        ArgumentNullException.ThrowIfNull(gradebook);
        string json = JsonSerializer.Serialize(GradebookData.FromDomain(gradebook), _options);
        AtomicTextFile.Write(_path, json);
    }
}
