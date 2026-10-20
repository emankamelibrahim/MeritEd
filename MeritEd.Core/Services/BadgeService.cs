using MeritEd.Core.Entities;
using MeritEd.Core.Enums;

namespace MeritEd.Core.Services;

public class BadgeService
{
    public IEnumerable<Badge> EvaluateTriggers(
        Enrollment enrollment,
        IEnumerable<Badge> allBadges,
        IEnumerable<Guid> alreadyEarnedBadgeIds,
        bool isFirstSubmission,
        bool isFirstDiscussionPost)
    {
        var earned = new List<Badge>();
        var earnedIds = alreadyEarnedBadgeIds.ToHashSet();

        foreach (var badge in allBadges)
        {
            if (earnedIds.Contains(badge.Id))
                continue;

            var shouldAward = badge.TriggerType switch
            {
                BadgeTriggerType.XPMilestone =>
                    enrollment.TotalXP >= badge.TriggerValue,

                BadgeTriggerType.FirstSubmission =>
                    isFirstSubmission,

                BadgeTriggerType.FirstDiscussionPost =>
                    isFirstDiscussionPost,

                BadgeTriggerType.IdentityReached =>
                    enrollment.IdentityType.HasValue &&
                    (int)enrollment.IdentityType.Value == badge.TriggerValue,

                _ => false
            };

            if (shouldAward)
                earned.Add(badge);
        }

        return earned;
    }
}