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
}