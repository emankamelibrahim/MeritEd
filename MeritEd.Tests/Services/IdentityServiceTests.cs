using MeritEd.Core.Enums;
using MeritEd.Core.Services;
using Xunit;

namespace MeritEd.Tests.Services;

public class IdentityServiceTests
{
    private readonly IdentityService _sut = new();

    private static List<(XPCategory Category, int Amount)> Ledger(
        int assessment = 0, int recovery = 0, int badge = 0)
    {
        var list = new List<(XPCategory, int)>();
        if (assessment > 0) list.Add((XPCategory.Assessment, assessment));
        if (recovery > 0) list.Add((XPCategory.Recovery, recovery));
        if (badge > 0) list.Add((XPCategory.Badge, badge));
        return list;
    }

    [Fact]
    public void Calculate_EmptyLedger_ReturnsNull()
        => Assert.Null(_sut.Calculate(Ledger()));

    [Fact]
    public void Calculate_AllAssessment_ReturnsScholar()
        => Assert.Equal(IdentityType.Scholar, _sut.Calculate(Ledger(assessment: 50)));

    [Fact]
    public void Calculate_RecoveryAbove30Percent_ReturnsChallenger()
        => Assert.Equal(IdentityType.Challenger, _sut.Calculate(Ledger(110, 75, 55)));

    // Regression: 20 XP badge bonus pushes Recovery from 31.25% to 28.8%
    [Fact]
    public void Calculate_RecoveryDropsBelow30AfterBonus_ReturnsContributor()
        => Assert.Equal(IdentityType.Contributor, _sut.Calculate(Ledger(110, 75, 75)));

    [Fact]
    public void Calculate_RecoveryTakesPriorityOverScholar()
        => Assert.Equal(IdentityType.Challenger, _sut.Calculate(Ledger(65, 35)));

    [Fact]
    public void Calculate_RecoveryExactly30Percent_IsNotChallenger()
        => Assert.Equal(IdentityType.Scholar, _sut.Calculate(Ledger(70, 30)));

    [Fact]
    public void Calculate_AssessmentExactly60Percent_IsNotScholar()
        => Assert.Equal(IdentityType.Contributor, _sut.Calculate(Ledger(60, 0, 40)));
}