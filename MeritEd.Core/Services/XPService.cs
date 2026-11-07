using MeritEd.Core.Enums;

namespace MeritEd.Core.Services;

public class XPService
{
    public int CalculateXP(int baseXP, bool isRecovery, decimal recoveryMultiplier)
    {
        if (!isRecovery)
            return baseXP;

        return (int)Math.Floor(baseXP * recoveryMultiplier);
    }

    public bool IsRecoveryEligible(DateTime? dueDate, bool allowsRecovery)
    {
        if (!allowsRecovery) return false;
        if (dueDate == null) return false;
        return DateTime.UtcNow > dueDate;
    }
    public int CalculateScoredXP(int baseXP, int correct, int total,
    bool isRecovery, decimal recoveryMultiplier)
{
    if (total <= 0) return 0;

    var multiplier = isRecovery ? recoveryMultiplier : 1m;
    return (int)Math.Floor((decimal)baseXP * correct * multiplier / total);
}
}