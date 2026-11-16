using Pos.Domain;
using Xunit;

namespace Pos.Tests;

public class DomainTests
{
    [Theory]
    [InlineData(0, "Иван")]
    [InlineData(-1, "Иван")]
    [InlineData(1, "")]
    [InlineData(1, "  ")]
    [InlineData(1, null)]
    public void Student_RejectsInvalidInput(int id, string? name)
    {
        void Act() { _ = new Student(id, name!); }
        Assert.ThrowsAny<ArgumentException>(Act);
    }

    [Fact]
    public void Student_TrimsName()
    {
        Student student = new Student(1, "  Иван Петров  ");
        Assert.Equal("Иван Петров", student.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Weight_RejectsNonpositiveValue(int value)
    {
        void Act() { _ = new Weight(value); }
        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }

    [Fact]
    public void Weight_HasValueEquality()
    {
        Assert.Equal(new Weight(2m), new Weight(2m));
        Assert.NotEqual(new Weight(2m), new Weight(3m));
    }

    [Theory]
    [InlineData(0, "Критерий", 10)]
    [InlineData(1, " ", 10)]
    [InlineData(1, "Критерий", 0)]
    [InlineData(1, "Критерий", -10)]
    public void Criterion_RejectsInvalidInput(int id, string name, int maximum)
    {
        void Act() { _ = new Criterion(id, name, maximum, new Weight(1m)); }
        Assert.ThrowsAny<ArgumentException>(Act);
    }

    [Fact]
    public void Criterion_RejectsMissingWeight()
    {
        void Act() { _ = new Criterion(1, "Проект", 10m, null!); }
        Assert.Throws<ArgumentNullException>(Act);
    }

    [Fact]
    public void Rubric_RejectsDuplicatesEvenWithoutBuilder()
    {
        Criterion criterion = new Criterion(1, "Проект", 10m, new Weight(1m));
        void Act() { _ = new Rubric(new[] { criterion, criterion }); }
        Assert.Throws<ArgumentException>(Act);
    }

    [Fact]
    public void Builder_RejectsDuplicatesWithoutChangingBuiltRubric()
    {
        RubricBuilder builder = new RubricBuilder();
        Criterion criterion = new Criterion(1, "Проект", 10m, new Weight(1m));
        builder.AddCriterion(criterion);
        Rubric first = builder.Build();
        void Duplicate() { builder.AddCriterion(criterion); }
        Assert.Throws<ArgumentException>(Duplicate);
        builder.AddCriterion(new Criterion(2, "Экзамен", 10m, new Weight(2m)));
        Assert.Single(first.GetCriteria());
        Assert.Equal(2, builder.Build().GetCriteria().Count);
    }

    [Fact]
    public void Gradebook_RejectsDuplicateStudent()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        void Act() { gradebook.AddStudent(new Student(1, "Другой студент")); }
        Assert.Throws<ArgumentException>(Act);
        Assert.Equal("Иван Петров", gradebook.GetStudent(1).Name);
    }

    [Theory]
    [InlineData(99, 1)]
    [InlineData(1, 99)]
    public void Assign_RejectsUnknownStudentOrCriterion(int studentId, int criterionId)
    {
        Gradebook gradebook = TestData.CreateGradebook();
        void Act() { gradebook.AssignEvaluation(studentId, criterionId, 5m); }
        Assert.Throws<ArgumentException>(Act);
        Assert.Empty(gradebook.GetEvaluations(1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Assign_RejectsScoreOutsideRange(int score)
    {
        Gradebook gradebook = TestData.CreateGradebook();
        void Act() { gradebook.AssignEvaluation(1, 1, score); }
        Assert.Throws<ArgumentOutOfRangeException>(Act);
        Assert.Empty(gradebook.GetEvaluations(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Assign_AcceptsBothBoundaries(int score)
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, score);
        Assert.Equal((decimal)score, Assert.Single(gradebook.GetEvaluations(1)).Score);
    }

    [Fact]
    public void Assign_RejectsDuplicateWithoutReplacingScore()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 8m);
        void Act() { gradebook.AssignEvaluation(1, 1, 9m); }
        Assert.Throws<InvalidOperationException>(Act);
        Assert.Equal(8m, Assert.Single(gradebook.GetEvaluations(1)).Score);
    }

    [Fact]
    public void InvalidCorrection_PreservesOldEvaluation()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 8m);
        void Act() { gradebook.CorrectEvaluation(1, 1, 12m); }
        Assert.Throws<ArgumentOutOfRangeException>(Act);
        Assert.Equal(8m, Assert.Single(gradebook.GetEvaluations(1)).Score);
    }

    [Fact]
    public void Correction_RequiresExistingEvaluation()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        void Act() { gradebook.CorrectEvaluation(1, 1, 8m); }
        Assert.Throws<InvalidOperationException>(Act);
        Assert.Empty(gradebook.GetEvaluations(1));
    }

    [Fact]
    public void Removal_RemovesZeroAndRejectsRepeatedRemoval()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 0m);
        gradebook.RemoveEvaluation(1, 1);
        Assert.Empty(gradebook.GetEvaluations(1));
        void Act() { gradebook.RemoveEvaluation(1, 1); }
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void EvaluationSnapshot_IsReadOnlyAndDoesNotChangeAfterCorrection()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 8m);
        IReadOnlyCollection<Evaluation> snapshot = gradebook.GetEvaluations(1);
        void ClearSnapshot() { ((ICollection<Evaluation>)snapshot).Clear(); }
        Assert.Throws<NotSupportedException>(ClearSnapshot);
        gradebook.CorrectEvaluation(1, 1, 9m);
        Assert.Equal(8m, Assert.Single(snapshot).Score);
        Assert.Equal(9m, Assert.Single(gradebook.GetEvaluations(1)).Score);
    }

    [Fact]
    public void DuplicateCriterion_DoesNotReplaceRubricOrInvalidateExistingGrade()
    {
        Gradebook gradebook = TestData.CreateGradebook();
        gradebook.AssignEvaluation(1, 1, 8m);
        void Act() { gradebook.AddCriterion(new Criterion(1, "Другой", 5m, new Weight(2m))); }
        Assert.Throws<ArgumentException>(Act);
        Assert.Equal(2, gradebook.GetCriteria().Count);
        Assert.Equal(8m, Assert.Single(gradebook.GetEvaluations(1)).Score);
    }
}
