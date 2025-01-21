using Newtonsoft.Json;

public class LargeLanguageAdapter
{ 
    public void Reset()
    {
        OpenAiHelpers.Prepare();
    }

    public ChoicesNarrativeResponse NextEncounterChoices(List<CompletionMessage4o> previousPrompts, CompletionMessage4o newPrompt, string openAiApiKey)
    {
        List<CompletionMessage4o> messages = new();
        messages.AddRange(previousPrompts);
        messages.Add(newPrompt);

        HttpRequestMessage request = OpenAiHelpers.PrepareOpenAiRequestChoices(messages, openAiApiKey);

        string response = OpenAiHelpers.GetOpenAiResponse(request);
        var result = ProcessOpenAiResponseChoices(response);

        return result;
    }


    public string EncounterEndNarrative(List<CompletionMessage4o> previousPrompts, CompletionMessage4o newPrompt, string openAiApiKey)
    {

        List<CompletionMessage4o> messages = new();
        messages.AddRange(previousPrompts);
        messages.Add(newPrompt);

        HttpRequestMessage request = OpenAiHelpers.PrepareOpenAiRequestEncounterEnd(messages, openAiApiKey);

        string response = OpenAiHelpers.GetOpenAiResponse(request);
        var result = ProcessOpenAiResponseEncounterEnd(response);
        
        return result;
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

