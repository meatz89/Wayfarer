
// Parses choice responses into a dictionary
using BlazorRPG.Game.EncounterManager;
using System.Text;

public static class ChoiceResponseParser
{
    public static Dictionary<IChoice, string> ParseResponse(string response, List<IChoice> choices)
    {
        Dictionary<IChoice, string> result = new Dictionary<IChoice, string>();
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
                    result[choices[currentChoice]] = currentDescription.ToString().Trim();
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
            result[choices[currentChoice]] = currentDescription.ToString().Trim();
        }

        // Fill in any missing choices with defaults
        foreach (IChoice choice in choices.Where(c => !result.ContainsKey(c)))
        {
            result[choice] = choice.Description;
        }

        return result;
    }
}
