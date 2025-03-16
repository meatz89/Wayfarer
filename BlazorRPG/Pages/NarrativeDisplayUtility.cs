using System.Text.Json;
using System.Text.RegularExpressions;
/// <summary>
/// Utility class for parsing and displaying narrative responses from the AI
/// </summary>
public static class NarrativeDisplayUtility
{
    private static readonly Regex JsonExtractor = new Regex(@"\{(?:[^{}]|(?<Open>\{)|(?<-Open>\}))*(?(Open)(?!))\}", RegexOptions.Compiled);

    /// <summary>
    /// Try to parse a narrative into sections. If JSON parsing fails, returns the original narrative.
    /// </summary>
    public static NarrativeSections GetNarrativeSections(string narrative)
    {
        try
        {
            // Check if this is a JSON response
            string jsonString = ExtractJson(narrative);

            // Try to parse as JSON
            var response = JsonSerializer.Deserialize<NarrativeResponse>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response != null)
            {
                return new NarrativeSections
                {
                    ActionOutcome = response.ActionOutcome,
                    NewSituation = response.NewSituation,
                    KeyPoints = response.KeyPoints,
                    Atmosphere = response.Atmosphere,
                    IsJsonFormat = true
                };
            }
        }
        catch
        {
            // If JSON parsing fails, look for paragraph breaks
            string[] paragraphs = narrative.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (paragraphs.Length >= 2)
            {
                return new NarrativeSections
                {
                    ActionOutcome = paragraphs[0],
                    NewSituation = paragraphs[1],
                    KeyPoints = new List<string>(),
                    Atmosphere = null,
                    IsJsonFormat = false
                };
            }
        }

        // Default: treat the entire narrative as a single section
        return new NarrativeSections
        {
            ActionOutcome = narrative,
            NewSituation = null,
            KeyPoints = new List<string>(),
            Atmosphere = null,
            IsJsonFormat = false
        };
    }

    /// <summary>
    /// Format narrative sections for display
    /// </summary>
    public static string FormatNarrative(NarrativeSections sections)
    {
        if (!sections.IsJsonFormat)
        {
            // If not JSON format, return original text
            if (string.IsNullOrEmpty(sections.NewSituation))
            {
                return sections.ActionOutcome;
            }
            else
            {
                return $"{sections.ActionOutcome}\n\n{sections.NewSituation}";
            }
        }

        // For JSON format, use a more structured display
        var formatted = $"{sections.ActionOutcome}\n\n{sections.NewSituation}";

        if (sections.KeyPoints != null && sections.KeyPoints.Count > 0)
        {
            formatted += "\n\n<div class=\"key-points\">";
            foreach (var point in sections.KeyPoints)
            {
                formatted += $"\n• {point}";
            }
            formatted += "\n</div>";
        }

        if (!string.IsNullOrEmpty(sections.Atmosphere))
        {
            formatted += $"\n\n<div class=\"atmosphere\"><i>{sections.Atmosphere}</i></div>";
        }

        return formatted;
    }

    /// <summary>
    /// Extract JSON from a string that might contain non-JSON content
    /// </summary>
    private static string ExtractJson(string input)
    {
        var match = JsonExtractor.Match(input);
        return match.Success ? match.Value : input;
    }
}
