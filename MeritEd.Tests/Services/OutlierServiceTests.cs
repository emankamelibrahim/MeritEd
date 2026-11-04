using MeritEd.Core.Enums;
using MeritEd.Core.Services;
using Xunit;

namespace MeritEd.Tests.Services;

public class OutlierServiceTests
{
    private readonly OutlierService _sut = new();

    [Fact]
    public void Median_Empty_ReturnsZero()
        => Assert.Equal(0m, _sut.CalculateMedian(Array.Empty<int>()));

    [Fact]
    public void Median_OddCount_ReturnsMiddleValue()
        => Assert.Equal(50m, _sut.CalculateMedian(new[] { 90, 10, 50 }));

    [Fact]
    public void Median_EvenCount_ReturnsMidpoint()
        => Assert.Equal(90m, _sut.CalculateMedian(new[] { 0, 180 }));

    [Theory]
    [InlineData(180, 90, OutlierStatus.High)]
    [InlineData(0, 90, OutlierStatus.Low)]
    [InlineData(90, 90, OutlierStatus.Normal)]
    [InlineData(135, 90, OutlierStatus.Normal)]   // exactly +50%
    [InlineData(136, 90, OutlierStatus.High)]
    [InlineData(45, 90, OutlierStatus.Normal)]    // exactly -50%
    [InlineData(44, 90, OutlierStatus.Low)]
    public void Classify_UsesFiftyPercentThreshold(int xp, int median, OutlierStatus expected)
        => Assert.Equal(expected, _sut.Classify(xp, median));

    [Fact]
    public void Classify_ZeroMedian_NeverFlags()
        => Assert.Equal(OutlierStatus.Normal, _sut.Classify(500, 0m));

    [Fact]
    public void Classify_CustomThreshold_IsRespected()
        => Assert.Equal(OutlierStatus.High, _sut.Classify(130, 100m, 0.25m));
}