
using System.Text;

public static class ChoiceResponseParser
{
    public static Dictionary<IChoice, ChoiceNarrative> ParseChoiceNarratives(string response, List<IChoice> choices)
    {
        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();

        // Split the response by lines to process one line at a time
        string[] lines = response.Split('\n');
        int currentChoice = -1;
        StringBuilder currentContent = new StringBuilder();

        // Process each line
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // Check if this line starts a new choice
            if (trimmedLine.StartsWith("Choice ") && trimmedLine.Contains(":"))
            {
                // Process the previous choice if we were building one
                if (currentChoice >= 0 && currentChoice < choices.Count && currentContent.Length > 0)
                {
                    ProcessChoiceContent(result, choices, currentChoice, currentContent.ToString().Trim());
                    currentContent.Clear();
                }

                // Extract the choice number
                string choicePrefix = "Choice ";
                int colonIndex = trimmedLine.IndexOf(':');
                if (colonIndex > choicePrefix.Length)
                {
                    string numberPart = trimmedLine.Substring(choicePrefix.Length, colonIndex - choicePrefix.Length).Trim();
                    if (int.TryParse(numberPart, out int choiceNum) && choiceNum >= 1 && choiceNum <= choices.Count)
                    {
                        currentChoice = choiceNum - 1;

                        // Add content after the colon to our buffer
                        if (colonIndex < trimmedLine.Length - 1)
                        {
                            currentContent.Append(trimmedLine.Substring(colonIndex + 1).Trim());
                            currentContent.Append(' ');
                        }
                    }
                }
            }
            else if (currentChoice >= 0 && currentChoice < choices.Count && !string.IsNullOrWhiteSpace(trimmedLine))
            {
                // Add content to the current choice being built
                currentContent.Append(trimmedLine);
                currentContent.Append(' ');
            }
        }

        // Process the last choice if we were building one
        if (currentChoice >= 0 && currentChoice < choices.Count && currentContent.Length > 0)
        {
            ProcessChoiceContent(result, choices, currentChoice, currentContent.ToString().Trim());
        }

        // Fill in any missing choices with defaults based on approach and focus tags
        for (int i = 0; i < choices.Count; i++)
        {
            if (!result.ContainsKey(choices[i]))
            {
                result[choices[i]] = new ChoiceNarrative("defaultName", "defaultDescription");
            }
        }

        return result;
    }

    private static void ProcessChoiceContent(Dictionary<IChoice, ChoiceNarrative> result, List<IChoice> choices, int choiceIndex, string content)
    {
        IChoice choice = choices[choiceIndex];

        // Split by the dash separator
        int dashPosition = content.IndexOf(" - ");
        if (dashPosition > 0 && dashPosition < content.Length - 3) // Ensure there's content after the dash
        {
            string name = content.Substring(0, dashPosition).Trim();
            string description = content.Substring(dashPosition + 3).Trim(); // +3 to skip " - "

            // Ensure both parts start with "I"
            if (name.StartsWith("I ", StringComparison.OrdinalIgnoreCase) &&
                description.StartsWith("I ", StringComparison.OrdinalIgnoreCase))
            {
                result[choice] = new ChoiceNarrative(name, description);
                return;
            }
        }

        result[choice] = new ChoiceNarrative("fallbackName", "fallbackDescription");
    }
}