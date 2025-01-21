using Newtonsoft.Json;

public class LargeLanguageAdapter
{
    public ChoicesNarrativeResponse ChoicesNarrativeResponse;

    public void Reset()
    {
        OpenAiHelpers.Prepare();
    }

    public ChoicesNarrativeResponse Execute(List<CompletionMessage4o> previousPrompts, CompletionMessage4o newPrompt, string apiKey)
    {
        List<CompletionMessage4o> messages = new();
        messages.AddRange(previousPrompts);
        messages.Add(newPrompt);

        HttpRequestMessage request = OpenAiHelpers.PrepareOpenAiRequest(messages, apiKey);

        string response = OpenAiHelpers.GetOpenAiResponse(request);
        ProecessOpenAiResponse(response);

        return ChoicesNarrativeResponse;
    }

    public void ProecessOpenAiResponse(string openAiResponseString)
    {
        try
        {
            ChoicesNarrativeResponse =
                JsonConvert.DeserializeObject<ChoicesNarrativeResponse>(openAiResponseString);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

}

