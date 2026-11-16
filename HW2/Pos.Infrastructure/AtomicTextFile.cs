using System.Text;

namespace Pos.Infrastructure;

internal static class AtomicTextFile
{
    public static void Write(string path, string text)
    {
        string fullPath = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);
        string temporaryPath = Path.Combine(directory, $".pos-{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporaryPath, text, new UTF8Encoding(true));
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
