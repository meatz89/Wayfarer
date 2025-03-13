using BlazorRPG.Game.EncounterManager;
using System.Text;

public static class ChoiceResponseParser
{
    public static Dictionary<IChoice, ChoiceNarrative> ParseChoiceNarratives(string response, List<IChoice> choices)
    {
        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();

        // Split the response by choice identifiers
        string[] choiceSections = SplitIntoChoiceSections(response);

        // Process each choice section
        for (int i = 0; i < choiceSections.Length && i < choices.Count; i++)
        {
            string section = choiceSections[i];
            int choiceNumber = i + 1;

            // Try to extract name and description
            if (TryExtractNameAndDescription(section, out string name, out string description))
            {
                result[choices[i]] = new ChoiceNarrative(name, description);
            }
            else
            {
                // Fallback with default formatting
                string fallbackName = $"I {choices[i].Approach.ToString().ToLower()} with {choices[i].Focus.ToString().ToLower()}";
                string fallbackDescription = section.Trim();

                // If we couldn't extract a description either, provide a generic one
                if (string.IsNullOrWhiteSpace(fallbackDescription))
                {
                    fallbackDescription = $"I use my {choices[i].Approach} approach focused on {choices[i].Focus} to address the situation.";
                }

                result[choices[i]] = new ChoiceNarrative(fallbackName, fallbackDescription);
            }
        }

        // Fill in any missing choices with defaults
        for (int i = 0; i < choices.Count; i++)
        {
            if (!result.ContainsKey(choices[i]))
            {
                string defaultName = $"I {choices[i].Approach.ToString().ToLower()} with {choices[i].Focus.ToString().ToLower()}";
                string defaultDescription = $"I use my {choices[i].Approach} approach focused on {choices[i].Focus} to address the situation.";
                result[choices[i]] = new ChoiceNarrative(defaultName, defaultDescription);
            }
        }

        return result;
    }

    private static string[] SplitIntoChoiceSections(string response)
    {
        List<string> sections = new List<string>();

        // Find starting positions of each "Choice #:" section
        List<int> startPositions = new List<int>();

        for (int i = 1; i <= 4; i++)
        {
            string marker = $"Choice {i}:";
            int pos = response.IndexOf(marker);
            if (pos >= 0)
            {
                startPositions.Add(pos);
            }
        }

        // Sort by position in the text
        startPositions.Sort();

        // Extract each section
        for (int i = 0; i < startPositions.Count; i++)
        {
            int startPos = startPositions[i];
            int endPos = (i < startPositions.Count - 1) ? startPositions[i + 1] : response.Length;

            sections.Add(response.Substring(startPos, endPos - startPos));
        }

        return sections.ToArray();
    }

    private static bool TryExtractNameAndDescription(string section, out string name, out string description)
    {
        name = string.Empty;
        description = string.Empty;

        // Skip the "Choice #:" prefix
        int colonPos = section.IndexOf(':');
        if (colonPos < 0) return false;

        string content = section.Substring(colonPos + 1).Trim();

        // Find the separator dash
        int dashPos = content.IndexOf(" - ");
        if (dashPos < 0) return false;

        // Extract name and description
        name = content.Substring(0, dashPos).Trim();
        description = content.Substring(dashPos + 3).Trim();

        // Verify both start with "I"
        return name.StartsWith("I ") && description.StartsWith("I ");
    }
}