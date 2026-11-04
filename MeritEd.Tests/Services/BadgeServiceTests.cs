using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Services;
using Xunit;

namespace MeritEd.Tests.Services;

public class BadgeServiceTests
{
    private readonly BadgeService _sut = new();

    private static Badge MakeBadge(BadgeTriggerType type, int value = 0)
        => new() { Id = Guid.NewGuid(), Name = type.ToString(), TriggerType = type, TriggerValue = value };

    private static Enrollment MakeEnrollment(int xp = 0, IdentityType? identity = null)
        => new() { Id = Guid.NewGuid(), TotalXP = xp, IdentityType = identity };

    private List<Badge> Evaluate(Enrollment e, List<Badge> badges,
        IEnumerable<Guid>? earned = null, bool firstSubmission = false, bool firstPost = false)
        => _sut.EvaluateTriggers(e, badges, earned ?? Enumerable.Empty<Guid>(),
            firstSubmission, firstPost).ToList();

    [Theory]
    [InlineData(99, false)]
    [InlineData(100, true)]
    [InlineData(250, true)]
    public void XPMilestone_AwardedAtOrAboveThreshold(int xp, bool expected)
    {
        var badge = MakeBadge(BadgeTriggerType.XPMilestone, 100);
        var result = Evaluate(MakeEnrollment(xp), new() { badge });
        Assert.Equal(expected, result.Contains(badge));
    }

    [Fact]
    public void AlreadyEarnedBadge_IsNotAwardedAgain()
    {
        var badge = MakeBadge(BadgeTriggerType.XPMilestone, 100);
        var result = Evaluate(MakeEnrollment(500), new() { badge }, earned: new[] { badge.Id });
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void FirstSubmission_DependsOnFlag(bool flag, int expectedCount)
    {
        var badge = MakeBadge(BadgeTriggerType.FirstSubmission);
        Assert.Equal(expectedCount, Evaluate(MakeEnrollment(), new() { badge }, firstSubmission: flag).Count);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void FirstDiscussionPost_DependsOnFlag(bool flag, int expectedCount)
    {
        var badge = MakeBadge(BadgeTriggerType.FirstDiscussionPost);
        Assert.Equal(expectedCount, Evaluate(MakeEnrollment(), new() { badge }, firstPost: flag).Count);
    }

    [Fact]
    public void IdentityReached_MatchesOnlyTheCurrentIdentity()
    {
        var scholar = MakeBadge(BadgeTriggerType.IdentityReached, (int)IdentityType.Scholar);
        var challenger = MakeBadge(BadgeTriggerType.IdentityReached, (int)IdentityType.Challenger);

        var result = Evaluate(MakeEnrollment(100, IdentityType.Challenger), new() { scholar, challenger });

        Assert.Single(result);
        Assert.Contains(challenger, result);
    }

    [Fact]
    public void IdentityReached_NoIdentityYet_AwardsNothing()
    {
        var badge = MakeBadge(BadgeTriggerType.IdentityReached, (int)IdentityType.Scholar);
        Assert.Empty(Evaluate(MakeEnrollment(0, null), new() { badge }));
    }

    [Fact]
    public void MultipleTriggers_AwardsEveryMatchingBadge()
    {
        var first = MakeBadge(BadgeTriggerType.XPMilestone, 1);
        var rising = MakeBadge(BadgeTriggerType.XPMilestone, 100);
        var committed = MakeBadge(BadgeTriggerType.XPMilestone, 300);

        var result = Evaluate(MakeEnrollment(125), new() { first, rising, committed });

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(committed, result);
    }
}