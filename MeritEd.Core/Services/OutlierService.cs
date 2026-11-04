using MeritEd.Core.Enums;

namespace MeritEd.Core.Services;

public class OutlierService
{
    public decimal CalculateMedian(IEnumerable<int> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        if (sorted.Count == 0) return 0;

        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2m
            : sorted[mid];
    }

    public OutlierStatus Classify(int xp, decimal median, decimal threshold = 0.5m)
    {
        if (median <= 0) return OutlierStatus.Normal;
        if (xp > median * (1 + threshold)) return OutlierStatus.High;
        if (xp < median * (1 - threshold)) return OutlierStatus.Low;
        return OutlierStatus.Normal;
    }
}