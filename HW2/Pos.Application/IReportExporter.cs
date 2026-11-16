namespace Pos.Application;

public interface IReportExporter
{
    void Export(IReadOnlyCollection<StudentReport> reports, string path);
}
