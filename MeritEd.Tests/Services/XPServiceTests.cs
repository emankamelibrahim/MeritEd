using MeritEd.Core.Services;
using Xunit;

namespace MeritEd.Tests.Services;

public class XPServiceTests
{
    private readonly XPService _sut = new();

    [Fact]
    public void CalculateXP_OnTime_ReturnsFullBaseXP()
        => Assert.Equal(100, _sut.CalculateXP(100, false, 0.70m));

    [Theory]
    [InlineData(100, 0.70, 70)]
    [InlineData(100, 0.75, 75)]
    [InlineData(50, 0.75, 37)]
    [InlineData(0, 0.70, 0)]
    public void CalculateXP_Recovery_AppliesMultiplierAndFloors(int baseXP, double multiplier, int expected)
        => Assert.Equal(expected, _sut.CalculateXP(baseXP, true, (decimal)multiplier));

    [Fact]
    public void IsRecoveryEligible_PastDueAndAllowed_ReturnsTrue()
        => Assert.True(_sut.IsRecoveryEligible(DateTime.UtcNow.AddDays(-1), true));

    [Fact]
    public void IsRecoveryEligible_RecoveryNotAllowed_ReturnsFalse()
        => Assert.False(_sut.IsRecoveryEligible(DateTime.UtcNow.AddDays(-1), false));

    [Fact]
    public void IsRecoveryEligible_NoDueDate_ReturnsFalse()
        => Assert.False(_sut.IsRecoveryEligible(null, true));

    [Fact]
    public void IsRecoveryEligible_NotYetDue_ReturnsFalse()
        => Assert.False(_sut.IsRecoveryEligible(DateTime.UtcNow.AddDays(1), true));

    [Theory]
    [InlineData(100, 3, 3, false, 0.70, 100)]
    [InlineData(50, 2, 3, false, 0.70, 33)]
    [InlineData(100, 0, 4, false, 0.70, 0)]
    [InlineData(100, 1, 2, true, 0.70, 35)]
    [InlineData(50, 2, 3, true, 0.75, 25)]
    public void CalculateScoredXP_ScalesByFractionCorrect(
        int baseXP, int correct, int total, bool isRecovery, double multiplier, int expected)
        => Assert.Equal(expected,
            _sut.CalculateScoredXP(baseXP, correct, total, isRecovery, (decimal)multiplier));

    [Fact]
    public void CalculateScoredXP_ZeroQuestions_ReturnsZero()
        => Assert.Equal(0, _sut.CalculateScoredXP(100, 0, 0, false, 0.70m));
}