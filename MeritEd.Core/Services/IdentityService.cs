using MeritEd.Core.Enums;

namespace MeritEd.Core.Services;

public class IdentityService
{
    public IdentityType? Calculate(IEnumerable<(XPCategory Category, int Amount)> transactions)
    {
        var list = transactions.ToList();
        var total = list.Sum(t => t.Amount);

        if (total == 0) return null;

        var assessmentXP = list
            .Where(t => t.Category == XPCategory.Assessment)
            .Sum(t => t.Amount);

        var recoveryXP = list
            .Where(t => t.Category == XPCategory.Recovery)
            .Sum(t => t.Amount);

        var assessmentRatio = (double)assessmentXP / total;
        var recoveryRatio = (double)recoveryXP / total;

        // Priority order matters — first match wins
        if (recoveryRatio > 0.30)
            return IdentityType.Challenger;

        if (assessmentRatio > 0.60)
            return IdentityType.Scholar;

        return IdentityType.Contributor;
    }
}