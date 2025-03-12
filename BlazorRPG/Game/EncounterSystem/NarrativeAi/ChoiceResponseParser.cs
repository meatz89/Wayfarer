using BlazorRPG.Game.EncounterManager;
using System.Text;

public static class ChoiceResponseParser
{
    public static Dictionary<IChoice, ChoiceNarrative> ParseChoiceNarratives(string response, List<IChoice> choices)
    {
        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();
        string[] lines = response.Split('\n');

        int currentChoice = -1;
        StringBuilder currentDescription = new StringBuilder();

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // Check if this is a new choice marker
            if (trimmedLine.StartsWith("Choice ") && trimmedLine.Contains(":"))
            {
                // Save previous choice if exists
                if (currentChoice >= 0 && currentChoice < choices.Count && currentDescription.Length > 0)
                {
                    string fullDescription = currentDescription.ToString().Trim();
                    // Generate a meaningful action summary from the first portion of description
                    string actionSummary = CreateActionSummaryFromDescription(fullDescription);

                    result[choices[currentChoice]] = new ChoiceNarrative(actionSummary, fullDescription);
                    currentDescription.Clear();
                }

                // Parse new choice number
                string[] parts = trimmedLine.Split(':', 2);
                string choiceNumStr = parts[0].Substring("Choice ".Length).Trim();

                if (int.TryParse(choiceNumStr, out int choiceNum) && choiceNum > 0 && choiceNum <= choices.Count)
                {
                    currentChoice = choiceNum - 1;
                    if (parts.Length > 1)
                    {
                        currentDescription.AppendLine(parts[1].Trim());
                    }
                }
            }
            else if (currentChoice >= 0 && currentChoice < choices.Count && !string.IsNullOrWhiteSpace(trimmedLine))
            {
                // Add to current description
                currentDescription.AppendLine(trimmedLine);
            }
        }

        // Add the last choice if not added
        if (currentChoice >= 0 && currentChoice < choices.Count && currentDescription.Length > 0)
        {
            string fullDescription = currentDescription.ToString().Trim();
            string actionSummary = CreateActionSummaryFromDescription(fullDescription);

            result[choices[currentChoice]] = new ChoiceNarrative(actionSummary, fullDescription);
        }

        // Fill in any missing choices with defaults
        foreach (IChoice choice in choices.Where(c => !result.ContainsKey(c)))
        {
            string actionSummary = $"I {choice.Approach.ToString().ToLower()} with {choice.Focus.ToString().ToLower()}";
            string description = $"I use my {choice.Approach} approach focused on {choice.Focus} to address the situation.";

            result[choice] = new ChoiceNarrative(actionSummary, description);
        }

        return result;
    }

    private static string CreateActionSummaryFromDescription(string description)
    {
        // Remove any text that appears after punctuation marks
        int firstPunctuation = FindFirstMajorPunctuation(description);
        string firstSentenceOrClause = firstPunctuation > 0
            ? description.Substring(0, firstPunctuation).Trim()
            : description;

        // Take first 6-10 words as summary
        string[] words = firstSentenceOrClause.Split(' ');
        int wordCount = Math.Min(words.Length, words.Length < 8 ? words.Length : 8);

        string summary = string.Join(" ", words.Take(wordCount));

        // Ensure it starts with "I"
        if (!summary.StartsWith("I ", StringComparison.OrdinalIgnoreCase))
        {
            summary = "I " + summary;
        }

        // Add ellipsis if we truncated
        if (words.Length > wordCount)
        {
            summary += "...";
        }

        return summary;
    }

    private static int FindFirstMajorPunctuation(string text)
    {
        int commaPos = text.IndexOf(',');
        int periodPos = text.IndexOf('.');
        int semicolonPos = text.IndexOf(';');
        int dashPos = text.IndexOf(" - ");

        List<int> positions = new List<int> { commaPos, periodPos, semicolonPos, dashPos };
        positions = positions.Where(p => p > 0).ToList();

        return positions.Any() ? positions.Min() : text.Length;
    }
}