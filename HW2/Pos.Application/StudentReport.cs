using Pos.Domain;

namespace Pos.Application;

public sealed record StudentReport(int StudentId, string StudentName, ScoreResult Result);

public sealed record EvaluationView(int CriterionId, string CriterionName,
    decimal MaxScore, decimal Weight, decimal? Score);

public enum CalculationMode
{
    Strict = 1,
    Preliminary = 2
}
