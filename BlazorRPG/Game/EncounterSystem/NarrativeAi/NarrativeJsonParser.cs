using System.Text.Json;
using System.Text.RegularExpressions;

public static class NarrativeJsonParser
{
    private static readonly Regex JsonExtractor = new Regex(@"\{(?:[^{}]|(?<Open>\{)|(?<-Open>\}))*(?(Open)(?!))\}", RegexOptions.Compiled);

    public static Dictionary<IChoice, ChoiceNarrative> ParseChoiceResponse(string jsonResponse, List<IChoice> choices)
    {
        // Extract the JSON if it's embedded in other text
        Match match = JsonExtractor.Match(jsonResponse);
        string jsonToDeserialize = match.Success ? match.Value : jsonResponse;

        // Parse as a dynamic object
        using JsonDocument doc = JsonDocument.Parse(jsonToDeserialize);
        JsonElement root = doc.RootElement;

        // Get the choices array
        JsonElement choicesArray = root.GetProperty("choices");

        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();

        for (int i = 0; i < Math.Min(choices.Count, choicesArray.GetArrayLength()); i++)
        {
            JsonElement choiceObj = choicesArray[i];

            string name = choiceObj.GetProperty("name").GetString();
            string description = choiceObj.GetProperty("description").GetString();

            result[choices[i]] = new ChoiceNarrative(name, description);
        }

        return result;
    }
}