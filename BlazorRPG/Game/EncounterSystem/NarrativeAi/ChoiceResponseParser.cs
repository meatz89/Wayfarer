using System.Text.RegularExpressions;

public static class ChoiceResponseParser
{
    // Regex pattern to match choices in the expected format
    private static readonly Regex ChoicePattern = new Regex(
        @"Choice\s+(\d+)\s*:\s*(I[^-]+)\s+-\s*(I.+?)(?=\s*Choice\s+\d+\s*:|$)",
        RegexOptions.Singleline | RegexOptions.IgnoreCase);

    public static Dictionary<IChoice, ChoiceNarrative> ParseChoiceNarratives(string response, List<IChoice> choices)
    {
        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();

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

        // Handle any missing choices with generated narratives based on approach and focus
        for (int i = 0; i < choices.Count; i++)
        {
            if (!result.ContainsKey(choices[i]))
            {
                // Generate a reasonable fallback based on the approach and focus tags
                result[choices[i]] = GenerateFallbackNarrative(choices[i]);
            }
        }

        return result;
    }

    private static ChoiceNarrative GenerateFallbackNarrative(IChoice choice)
    {
        string focus = choice.Focus.ToString();
        string effectType = choice.EffectType.ToString();

        // Create a basic name and description based on tags
        string description = $"I focus on {focus} to make progress.";

        // Add effect type information
        if (effectType == "Momentum")
        {
            description += " This should help me advance toward my goal.";
        }
        else if (effectType == "Pressure")
        {
            description += " This might be risky but could yield valuable results.";
        }

        return new ChoiceNarrative(choice.Name, description);
    }
}