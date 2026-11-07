using System.Text.Json;

namespace MeritEd.Core.Services;

public record QuizQuestion(string Question, IReadOnlyList<string> Options, int Answer);

public class QuizGradingService
{
    public const int MaxQuestions = 50;

    public (bool Valid, string Error, List<QuizQuestion> Questions) Parse(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
            return Fail("Questions must be a non-empty JSON array.");

        if (root.GetArrayLength() > MaxQuestions)
            return Fail($"A quiz can have at most {MaxQuestions} questions.");

        var questions = new List<QuizQuestion>();
        var number = 0;

        foreach (var element in root.EnumerateArray())
        {
            number++;

            if (element.ValueKind != JsonValueKind.Object)
                return Fail($"Question {number} must be an object.");

            if (!element.TryGetProperty("question", out var text)
                || text.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(text.GetString()))
                return Fail($"Question {number} needs a non-empty \"question\" text.");

            var options = new List<string>();
            if (element.TryGetProperty("options", out var optionsElement)
                && optionsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var option in optionsElement.EnumerateArray())
                {
                    var value = option.ValueKind == JsonValueKind.String ? option.GetString() : null;
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        options.Clear();
                        break;
                    }
                    options.Add(value);
                }
            }

            if (options.Count < 2 || options.Count > 10)
                return Fail($"Question {number} needs an \"options\" list of 2 to 10 non-empty choices.");

            if (!element.TryGetProperty("answer", out var answerElement)
                || answerElement.ValueKind != JsonValueKind.Number
                || !answerElement.TryGetInt32(out var answer)
                || answer < 0 || answer >= options.Count)
                return Fail($"Question {number} needs an \"answer\" that is the index of a correct option (0 to {options.Count - 1}).");

            questions.Add(new QuizQuestion(text.GetString()!, options, answer));
        }

        return (true, string.Empty, questions);
    }

    public (bool Valid, string Error, int Correct) Grade(
        IReadOnlyList<QuizQuestion> questions, IReadOnlyList<int> answers)
    {
        if (answers.Count != questions.Count)
            return (false, $"Answer all {questions.Count} questions.", 0);

        var correct = 0;
        for (var i = 0; i < questions.Count; i++)
        {
            if (answers[i] < 0 || answers[i] >= questions[i].Options.Count)
                return (false, $"Answer {i + 1} is not a valid option.", 0);

            if (answers[i] == questions[i].Answer) correct++;
        }

        return (true, string.Empty, correct);
    }

    public string StripAnswers(IReadOnlyList<QuizQuestion> questions) =>
        JsonSerializer.Serialize(
            questions.Select(q => new { question = q.Question, options = q.Options }));

    private static (bool Valid, string Error, List<QuizQuestion> Questions) Fail(string error) =>
        (false, error, new List<QuizQuestion>());
}