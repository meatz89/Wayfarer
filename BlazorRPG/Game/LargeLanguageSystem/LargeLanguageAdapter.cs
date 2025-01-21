using Newtonsoft.Json;

public class LargeLanguageAdapter
{
    private static string jsonPathChoices = "choices.json";
    private static string jsonPathEncounterEnd = "endEncounter.json";

    private static string FileContentChoices;
    private static string FileContentEncounterEnd;

    public void Reset()
    {
        FileHelper fileHelper = new FileHelper();
        FileContentChoices = fileHelper.ReadFile(jsonPathChoices);
        FileContentEncounterEnd = fileHelper.ReadFile(jsonPathEncounterEnd);
    }

    public ChoicesNarrativeResponse NextEncounterChoices(List<CompletionMessage4o> previousPrompts, CompletionMessage4o newPrompt, string openAiApiKey)
    {
        List<CompletionMessage4o> messages = new();
        messages.AddRange(previousPrompts);
        messages.Add(newPrompt);

        HttpRequestMessage request = OpenAiHelpers.PrepareOpenAiRequest(FileContentChoices, messages, openAiApiKey);

        string response = OpenAiHelpers.GetOpenAiResponse(request);
        ChoicesNarrativeResponse result = ProcessOpenAiResponseChoices(response);

        return result;
    }


    public string EncounterEndNarrative(List<CompletionMessage4o> previousPrompts, CompletionMessage4o newPrompt, string openAiApiKey)
    {
        List<CompletionMessage4o> messages = new();
        messages.AddRange(previousPrompts);
        messages.Add(newPrompt);

        HttpRequestMessage request = OpenAiHelpers.PrepareOpenAiRequest(FileContentEncounterEnd, messages, openAiApiKey);

        string response = OpenAiHelpers.GetOpenAiResponse(request);
        return response;
    }

    public ChoicesNarrativeResponse ProcessOpenAiResponseChoices(string openAiResponseString)
    {
        try
        {
            return JsonConvert.DeserializeObject<ChoicesNarrativeResponse>(openAiResponseString);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }

    public string ProcessOpenAiResponseEncounterEnd(string openAiResponseString)
    {
        try
        {
            return JsonConvert.DeserializeObject<string>(openAiResponseString);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }
}

