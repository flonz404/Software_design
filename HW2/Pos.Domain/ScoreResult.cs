namespace Pos.Domain;

public sealed record ScoreResult(
    decimal? Value,
    bool IsPreliminary,
    int GradedCount,
    int CriterionCount,
    string Message);
