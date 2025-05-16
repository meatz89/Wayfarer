using System.Text.RegularExpressions;

public static class ChoiceResponseParser
{
    // Regex pattern to match choices in the expected format
    private static readonly Regex ChoicePattern = new Regex(
        @"Choice\s+(\d+)\s*:\s*(I[^-]+)\s+-\s*(I.+?)(?=\s*Choice\s+\d+\s*:|$)",
        RegexOptions.Singleline | RegexOptions.IgnoreCase);

    public static Dictionary<ActionCardDefinition, ChoiceNarrative> ParseChoiceNarratives(string response, List<ActionCardDefinition> choices)
    {
        Dictionary<ActionCardDefinition, ChoiceNarrative> result = new Dictionary<ActionCardDefinition, ChoiceNarrative>();

        // Use regex to find all choices in the response
        MatchCollection matches = ChoicePattern.Matches(response);

        // Process each match
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 4)
            {
                // Extract choice number (1-based), name, and description
                if (int.TryParse(match.Groups[1].Value, out int choiceNum) &&
                    choiceNum >= 1 && choiceNum <= choices.Count)
                {
                    int index = choiceNum - 1;
                    string name = match.Groups[2].Value.Trim();
                    string description = match.Groups[3].Value.Trim();

                    // Store the narrative for this choice
                    result[choices[index]] = new ChoiceNarrative(name, description);
                }
            }
        }

        return result;
    }
}