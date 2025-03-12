// Updated parser to handle first-person action summaries
using BlazorRPG.Game.EncounterManager;
using System.Text;

public static class ChoiceResponseParser
{
    public static Dictionary<IChoice, ChoiceNarrative> ParseChoiceNarratives(string response, List<IChoice> choices)
    {
        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();
        string[] lines = response.Split('\n');

        int currentChoice = -1;
        string currentSummary = string.Empty;
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
                    result[choices[currentChoice]] = new ChoiceNarrative(currentSummary, currentDescription.ToString().Trim());
                    currentDescription.Clear();
                    currentSummary = string.Empty;
                }

                // Parse new choice number
                string[] parts = trimmedLine.Split(':', 2);
                string choiceNumStr = parts[0].Substring("Choice ".Length).Trim();

                if (int.TryParse(choiceNumStr, out int choiceNum) && choiceNum > 0 && choiceNum <= choices.Count)
                {
                    currentChoice = choiceNum - 1;

                    if (parts.Length > 1)
                    {
                        string content = parts[1].Trim();

                        // Check if there's a summary: full format with " - " separator
                        if (content.Contains(" - "))
                        {
                            string[] contentParts = content.Split(new[] { " - " }, 2, StringSplitOptions.None);
                            currentSummary = contentParts[0].Trim();

                            if (contentParts.Length > 1)
                            {
                                currentDescription.AppendLine(contentParts[1].Trim());
                            }
                        }
                        else
                        {
                            // If no separator, the whole line is the summary and description starts with next line
                            currentSummary = content;
                        }
                    }
                }
            }
            else if (currentChoice >= 0 && currentChoice < choices.Count && !string.IsNullOrWhiteSpace(trimmedLine))
            {
                // If we have a summary but no description yet, and this is the first content line,
                // check if it might be a continuation of the summary (no " - " separator)
                if (currentDescription.Length == 0 && !string.IsNullOrEmpty(currentSummary) && !currentSummary.StartsWith("I "))
                {
                    // If current summary doesn't start with "I" but this line does, it might be the real summary
                    if (trimmedLine.StartsWith("I "))
                    {
                        // Move the previous "summary" to the description
                        currentDescription.AppendLine(currentSummary);
                        // Set the new summary
                        currentSummary = trimmedLine;
                    }
                    else
                    {
                        // Just add to description
                        currentDescription.AppendLine(trimmedLine);
                    }
                }
                else
                {
                    // Add to current description
                    currentDescription.AppendLine(trimmedLine);
                }
            }
        }

        // Add the last choice if not added
        if (currentChoice >= 0 && currentChoice < choices.Count && currentDescription.Length > 0)
        {
            result[choices[currentChoice]] = new ChoiceNarrative(currentSummary, currentDescription.ToString().Trim());
        }

        // Fill in any missing choices with defaults
        foreach (IChoice choice in choices.Where(c => !result.ContainsKey(c)))
        {
            // Create a default first-person summary from the choice name if missing
            string summary = $"I {choice.Name.ToLower()}";
            result[choice] = new ChoiceNarrative(summary, choice.Description);
        }

        // Ensure all summaries start with "I" and are within length guidelines
        foreach (var key in result.Keys.ToList())
        {
            var narrative = result[key];

            // Ensure summary starts with "I"
            if (!narrative.ShorthandName.StartsWith("I ", StringComparison.OrdinalIgnoreCase))
            {
                narrative.ShorthandName = $"I {narrative.ShorthandName.ToLower()}";
            }

            // Trim summary to reasonable length if too long
            if (narrative.ShorthandName.Split(' ').Length > 10)
            {
                var words = narrative.ShorthandName.Split(' ').Take(10).ToArray();
                narrative.ShorthandName = string.Join(" ", words);
            }

            result[key] = narrative;
        }

        return result;
    }
}