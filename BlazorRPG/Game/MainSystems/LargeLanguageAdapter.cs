using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

public class LargeLanguageAdapter
{
    private const string completionsUrl = "https://api.openai.com/v1/chat/completions";
    private string openAiApiKey;

    private Completion4oModel modelFromFile;
    private const string jsonPath = "completions.json";

    public SceneNarrative sceneNarrative;
    public List<ChoicesNarrative> choicesNarratives;

    public LargeLanguageAdapter()
    {
        FileHelper fileHelper = new FileHelper();
        string fileContent = fileHelper.ReadFile(jsonPath);
        modelFromFile = GetModelFromFile(fileContent).Result;
    }

    public void Execute(List<CompletionMessage4o> previousPrompts, CompletionMessage4o newPrompt, string apiKey)
    {
        this.openAiApiKey = apiKey;

        Completion4oModel completion4Model = modelFromFile;
        List<CompletionMessage4o> messages = completion4Model.messages;

        messages.AddRange(previousPrompts);
        messages.Add(newPrompt);

        HttpRequestMessage request = PrepareOpenAiRequest(messages);

        string response = GetOpenAiResponse(request);
        ProecessOpenAiResponse(response);
    }

    public void ProecessOpenAiResponse(string openAiResponseString)
    {
        try
        {
            JsonResponse jsonResponse =
                JsonConvert.DeserializeObject<JsonResponse>(openAiResponseString);

            sceneNarrative = new SceneNarrative() { Description = jsonResponse.introductory_narrative };
            choicesNarratives = jsonResponse.choices.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    public SceneNarrative GetSceneNarrative()
    {
        return sceneNarrative;
    }

    public List<ChoicesNarrative> GetChoicesNarrative()
    {
        return choicesNarratives;
    }

    private HttpRequestMessage PrepareOpenAiRequest(List<CompletionMessage4o> completionMessages)
    {
        // Prompt
        List<CompletionMessage4o> messages = modelFromFile.messages;
        messages.AddRange(completionMessages);

        string protocolLines = Environment.NewLine;
        protocolLines += Environment.NewLine;
        Console.WriteLine("");
        protocolLines += "----START Messages sent to API START----";
        Console.WriteLine("----START Messages sent to API START----");
        foreach (CompletionMessage4o message in messages)
        {
            foreach (var content in message.content)
            {
                protocolLines += Environment.NewLine;
                Console.WriteLine("");
                protocolLines += "----START Message----";
                Console.WriteLine("----START Message----");
                protocolLines += $"{message.role}:";
                Console.WriteLine($"{message.role}:");
                protocolLines += $"{content.text}";
                Console.WriteLine($"{content.text}");
                protocolLines += "----END Message----";
                Console.WriteLine("----END Message----");
                protocolLines += Environment.NewLine;
                Console.WriteLine("");
            }
        }
        protocolLines += "----END Messages sent to API END----";
        Console.WriteLine("----END Messages sent to API END----");
        protocolLines += Environment.NewLine;
        Console.WriteLine("");

        //Converting the object to a json string
        string json = JsonConvert.SerializeObject(modelFromFile);
        var stringData = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, completionsUrl)
        {
            Content = stringData
        };

        // Add your OpenAI API key here
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

        return request;
    }

    public string GetOpenAiResponse(HttpRequestMessage request)
    {
        const string openAiCallStarted = $"\r\nCalling OpenAI API\r\n";
        Console.WriteLine(openAiCallStarted);

        using (var httpClient = new HttpClient())
        {
            httpClient.Timeout = TimeSpan.FromSeconds(200);

            var responseFromOpenAiApi = httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead);
            if (!responseFromOpenAiApi.IsSuccessStatusCode)
            {
                Console.WriteLine(responseFromOpenAiApi.Content);
                return string.Empty;
            }

            using (Stream response = responseFromOpenAiApi.Content.ReadAsStream())
            using (StreamReader streamReader = new StreamReader(response))
            {
                string responseJson = streamReader.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(responseJson))
                {
                    dynamic jsonObject = JsonConvert.DeserializeObject(responseJson);
                    string responseMessage = jsonObject?.choices?[0]?.message?.content;

                    const string openAiCallReceived = $"\r\nSuccessfully called OpenAI API:\r\n";
                    Console.WriteLine(openAiCallReceived);
                    Console.WriteLine(responseMessage);
                    return responseMessage;
                }
            }
        }
        return string.Empty;
    }

    public CompletionMessage4o CreateCompletionMessage(Roles user, string prompt)
    {
        CompletionMessageContent4o content = new CompletionMessageContent4o()
        {
            type = "text",
            text = prompt
        };
        CompletionMessage4o message = new CompletionMessage4o()
        {
            role = user.ToString(),
            content = new List<CompletionMessageContent4o> { content }
        };
        return message;
    }

    private async Task<Completion4oModel> GetModelFromFile(string fileContent)
    {
        Completion4oModel completion4Model = null;
        completion4Model = JsonConvert.DeserializeObject<Completion4oModel>(fileContent);
        return completion4Model;
    }
}

