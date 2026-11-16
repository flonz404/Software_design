using Pos.Domain;
using Xunit;

namespace Pos.Tests;

public class CalculationTests
{
    private readonly FinalScoreCalculator _calculator = new FinalScoreCalculator();

    [Fact]
    public void WeightedScore_AccountsForDifferentMaximumsAndWeights()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 8m);
        gradebook.AssignEvaluation(1, 2, 15m);
        ScoreResult result = _calculator.Calculate(gradebook, 1, new StrictScoringStrategy());
        Assert.Equal(7.625m, decimal.Round(result.Value!.Value, 12));
        Assert.False(result.IsPreliminary);
        Assert.Equal(2, result.GradedCount);
    }

    [Fact]
    public void MissingEvaluation_BlocksStrictButGivesZeroContributionToPreliminary()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 10m);
        ScoreResult strict = _calculator.Calculate(gradebook, 1, new StrictScoringStrategy());
        ScoreResult preliminary = _calculator.Calculate(gradebook, 1, new PreliminaryScoringStrategy());
        Assert.Null(strict.Value);
        Assert.Equal(2.5m, decimal.Round(preliminary.Value!.Value, 12));
        Assert.True(preliminary.IsPreliminary);
        Assert.Single(gradebook.GetEvaluations(1));
    }

    [Fact]
    public void NoEvaluations_PreliminaryIsZeroButStrictIsUnavailable()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        Assert.Equal(0m, _calculator.Calculate(gradebook, 1, new PreliminaryScoringStrategy()).Value);
        Assert.Null(_calculator.Calculate(gradebook, 1, new StrictScoringStrategy()).Value);
    }

    [Fact]
    public void EmptyRubric_DoesNotProduceFinalOrPreliminaryScore()
    {
        Gradebook gradebook = new Gradebook(new RubricBuilder().Build());
        gradebook.AddStudent(new Student(1, "Иван"));
        Assert.Null(_calculator.Calculate(gradebook, 1, new StrictScoringStrategy()).Value);
        Assert.Null(_calculator.Calculate(gradebook, 1, new PreliminaryScoringStrategy()).Value);
    }

    [Fact]
    public void AddingCriterion_MakesPreviousStrictScoreIncomplete()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 10m);
        gradebook.AssignEvaluation(1, 2, 20m);
        Assert.Equal(10m, _calculator.Calculate(gradebook, 1, new StrictScoringStrategy()).Value);
        gradebook.AddCriterion(new Criterion(3, "Активность", 5m, new Weight(1m)));
        ScoreResult result = _calculator.Calculate(gradebook, 1, new StrictScoringStrategy());
        Assert.Null(result.Value);
        Assert.Equal(3, result.CriterionCount);
    }

    [Fact]
    public void CorrectingAndRemovingEvaluation_RecalculatesCurrentResult()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 8m);
        gradebook.AssignEvaluation(1, 2, 15m);
        gradebook.CorrectEvaluation(1, 2, 20m);
        Assert.Equal(9.5m, decimal.Round(_calculator.Calculate(gradebook, 1,
            new StrictScoringStrategy()).Value!.Value, 12));
        gradebook.RemoveEvaluation(1, 2);
        Assert.Null(_calculator.Calculate(gradebook, 1, new StrictScoringStrategy()).Value);
    }

    [Fact]
    public void ZeroIsAnExistingEvaluation()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 0m);
        gradebook.AssignEvaluation(1, 2, 0m);
        Assert.Equal(0m, _calculator.Calculate(gradebook, 1, new StrictScoringStrategy()).Value);
    }

    [Fact]
    public void MaximumDecimalWeights_DoNotOverflow()
    {
        RubricBuilder builder = new RubricBuilder();
        builder.AddCriterion(new Criterion(1, "A", 10m, new Weight(decimal.MaxValue)));
        builder.AddCriterion(new Criterion(2, "B", 10m, new Weight(decimal.MaxValue)));
        Gradebook gradebook = new Gradebook(builder.Build());
        gradebook.AddStudent(new Student(1, "Иван"));
        gradebook.AssignEvaluation(1, 1, 10m);
        gradebook.AssignEvaluation(1, 2, 0m);
        Assert.Equal(5m, _calculator.Calculate(gradebook, 1, new StrictScoringStrategy()).Value);
    }
}
