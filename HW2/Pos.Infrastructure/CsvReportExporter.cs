using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

public sealed class CsvReportExporter : IReportExporter
{
    public void Export(IReadOnlyCollection<StudentReport> reports, string path)
    {
        ArgumentNullException.ThrowIfNull(reports);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!string.Equals(Path.GetExtension(path), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Укажите имя файла с расширением .csv.", nameof(path));
        }

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("StudentId,StudentName,Score,Status,GradedCount,CriterionCount");
        foreach (StudentReport report in reports)
        {
            string score = "";
            if (report.Result.Value.HasValue)
            {
                score = decimal.Round(report.Result.Value.Value, 3, MidpointRounding.AwayFromZero)
                    .ToString("0.###", CultureInfo.InvariantCulture);
            }
            csv.Append(report.StudentId.ToString(CultureInfo.InvariantCulture)).Append(',');
            csv.Append(Escape(report.StudentName)).Append(',');
            csv.Append(score).Append(',');
            csv.Append(Escape(report.Result.Message)).Append(',');
            csv.Append(report.Result.GradedCount.ToString(CultureInfo.InvariantCulture)).Append(',');
            csv.Append(report.Result.CriterionCount.ToString(CultureInfo.InvariantCulture)).AppendLine();
        }
        AtomicTextFile.Write(path, csv.ToString());
    }

    private static string Escape(string value)
    {
        string trimmed = value.TrimStart();
        if (trimmed.Length > 0 && "=+-@".Contains(trimmed[0]))
        {
            value = "'" + value;
        }
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
