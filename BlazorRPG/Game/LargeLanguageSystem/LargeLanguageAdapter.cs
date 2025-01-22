using Newtonsoft.Json;

public class LargeLanguageAdapter
{
    private readonly string _jsonPathChoices = "choices.json";
    private readonly string _jsonPathEncounterEnd = "endEncounter.json";
    private readonly OpenAiClient _openAiClient;
    private string _fileContentChoices;
    private string _fileContentEncounterEnd;

    public LargeLanguageAdapter(string apiKey)
    {
        _openAiClient = new OpenAiClient(apiKey);
    }

    public void Reset()
    {
        FileHelper fileHelper = new FileHelper();
        _fileContentChoices = fileHelper.ReadFile(_jsonPathChoices);
        _fileContentEncounterEnd = fileHelper.ReadFile(_jsonPathEncounterEnd);
    }

    public ChoicesNarrativeResponse NextEncounterChoices(List<CompletionMessage4o> previousPrompts, CompletionMessage4o newPrompt)
    {
        List<CompletionMessage4o> messages = new();
        messages.AddRange(previousPrompts);
        messages.Add(newPrompt);

        Completion4oModel modelFromFile = JsonConvert.DeserializeObject<Completion4oModel>(_fileContentChoices);
        modelFromFile.messages.AddRange(messages);
        string json = JsonConvert.SerializeObject(modelFromFile);

        HttpRequestMessage request = _openAiClient.CreateRequest(json);
        string response = _openAiClient.SendRequest(request);
        return ProcessOpenAiResponseChoices(response);
    }

    public string EncounterEndNarrative(List<CompletionMessage4o> previousPrompts, CompletionMessage4o newPrompt)
    {
        List<CompletionMessage4o> messages = new();
        messages.AddRange(previousPrompts);
        messages.Add(newPrompt);

        Completion4oModel modelFromFile = JsonConvert.DeserializeObject<Completion4oModel>(_fileContentEncounterEnd);
        modelFromFile.messages.AddRange(messages);
        string json = JsonConvert.SerializeObject(modelFromFile);

        HttpRequestMessage request = _openAiClient.CreateRequest(json);
        string response = _openAiClient.SendRequest(request);
        return ProcessOpenAiResponseEncounterEnd(response);
    }

    private ChoicesNarrativeResponse ProcessOpenAiResponseChoices(string openAiResponseString)
    {
        try
        {
            return JsonConvert.DeserializeObject<ChoicesNarrativeResponse>(openAiResponseString);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    private string ProcessOpenAiResponseEncounterEnd(string openAiResponseString)
    {
        try
        {
            return JsonConvert.DeserializeObject<string>(openAiResponseString);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
}