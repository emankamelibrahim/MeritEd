using System.Text.Json;
using MeritEd.Core.Services;
using Xunit;

namespace MeritEd.Tests.Services;

public class QuizGradingServiceTests
{
    private readonly QuizGradingService _sut = new();

    private const string Valid = """
        [{"question":"2+2?","options":["3","4"],"answer":1},
         {"question":"Sky?","options":["Blue","Green","Red"],"answer":0}]
        """;

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement;

    private List<QuizQuestion> ValidQuestions() => _sut.Parse(Json(Valid)).Questions;

    [Fact]
    public void Parse_ValidQuiz_ReturnsQuestions()
    {
        var (valid, _, questions) = _sut.Parse(Json(Valid));

        Assert.True(valid);
        Assert.Equal(2, questions.Count);
        Assert.Equal(1, questions[0].Answer);
        Assert.Equal(3, questions[1].Options.Count);
    }

    [Theory]
    [InlineData("""{}""")]
    [InlineData("""[]""")]
    [InlineData(""" "text" """)]
    [InlineData("""[{"options":["a","b"],"answer":0}]""")]
    [InlineData("""[{"question":"q","options":["a"],"answer":0}]""")]
    [InlineData("""[{"question":"q","options":["a","b"],"answer":2}]""")]
    [InlineData("""[{"question":"q","options":["a","b"],"answer":-1}]""")]
    [InlineData("""[{"question":"q","options":["a","b"]}]""")]
    [InlineData("""[{"question":"q","options":["a",""],"answer":0}]""")]
    public void Parse_InvalidShapes_AreRejected(string json)
        => Assert.False(_sut.Parse(Json(json)).Valid);

    [Fact]
    public void Parse_MoreThanMaxQuestions_IsRejected()
    {
        var items = Enumerable.Repeat(
            """{"question":"q","options":["a","b"],"answer":0}""",
            QuizGradingService.MaxQuestions + 1);

        Assert.False(_sut.Parse(Json($"[{string.Join(",", items)}]")).Valid);
    }

    [Fact]
    public void Grade_AllCorrect_ReturnsFullCount()
    {
        var (valid, _, correct) = _sut.Grade(ValidQuestions(), new[] { 1, 0 });
        Assert.True(valid);
        Assert.Equal(2, correct);
    }

    [Fact]
    public void Grade_PartiallyCorrect_CountsOnlyCorrectOnes()
        => Assert.Equal(1, _sut.Grade(ValidQuestions(), new[] { 1, 1 }).Correct);

    [Fact]
    public void Grade_AllWrong_ReturnsZero()
        => Assert.Equal(0, _sut.Grade(ValidQuestions(), new[] { 0, 2 }).Correct);

    [Fact]
    public void Grade_WrongNumberOfAnswers_IsInvalid()
        => Assert.False(_sut.Grade(ValidQuestions(), new[] { 1 }).Valid);

    [Fact]
    public void Grade_OptionOutOfRange_IsInvalid()
        => Assert.False(_sut.Grade(ValidQuestions(), new[] { 1, 3 }).Valid);

    [Fact]
    public void Grade_NegativeIndex_IsInvalid()
        => Assert.False(_sut.Grade(ValidQuestions(), new[] { -1, 0 }).Valid);

    [Fact]
    public void StripAnswers_RemovesAnswerFieldAndKeepsTheRest()
    {
        var stripped = _sut.StripAnswers(ValidQuestions());

        Assert.DoesNotContain("\"answer\"", stripped);

        var root = Json(stripped);
        Assert.Equal(2, root.GetArrayLength());
        Assert.Equal("2+2?", root[0].GetProperty("question").GetString());
        Assert.Equal(2, root[0].GetProperty("options").GetArrayLength());
    }
}