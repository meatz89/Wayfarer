using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

public static class OpenAiHelpers
{
    private static string NewLine = "\r\n";
    private static string jsonPath = "completions.json";
    private static string completionsUrl = "https://api.openai.com/v1/chat/completions";
    private static string openAiApiKey;
    private static string FileContent;

    public static void Prepare()
    {
        FileHelper fileHelper = new FileHelper();
        FileContent = fileHelper.ReadFile(jsonPath);
    }

    public static HttpRequestMessage PrepareOpenAiRequest(List<CompletionMessage4o> completionMessages, string openAiApiKey)
    {
        // Prompt
        Completion4oModel modelFromFile = GetModelFromFile(FileContent);
        List<CompletionMessage4o> messages = modelFromFile.messages;
        messages.AddRange(completionMessages);

        string protocolLines = NewLine;
        protocolLines += NewLine;
        Console.WriteLine("");
        protocolLines += "----START Messages sent to API START----";
        Console.WriteLine("----START Messages sent to API START----");
        foreach (CompletionMessage4o message in messages)
        {
            foreach (CompletionMessageContent4o content in message.content)
            {
                protocolLines += NewLine;
                Console.WriteLine("");
                protocolLines += "----START Message----";
                Console.WriteLine("----START Message----");
                protocolLines += $"{message.role}:";
                Console.WriteLine($"{message.role}:");
                protocolLines += $"{content.text}";
                Console.WriteLine($"{content.text}");
                protocolLines += "----END Message----";
                Console.WriteLine("----END Message----");
                protocolLines += NewLine;
                Console.WriteLine("");
            }
        }
        protocolLines += "----END Messages sent to API END----";
        Console.WriteLine("----END Messages sent to API END----");
        protocolLines += NewLine;
        Console.WriteLine("");

        //Converting the object to a json string
        string json = JsonConvert.SerializeObject(modelFromFile);
        StringContent stringData = new StringContent(json, Encoding.UTF8, "application/json");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, completionsUrl)
        {
            Content = stringData
        };

        // Add your OpenAI API key here
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

        return request;
    }

    public static string GetOpenAiResponse(HttpRequestMessage request)
    {
        const string openAiCallStarted = $"\r\nCalling OpenAI API\r\n";
        Console.WriteLine(openAiCallStarted);

        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.Timeout = TimeSpan.FromSeconds(200);

            HttpResponseMessage responseFromOpenAiApi = httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead);
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

    public static CompletionMessage4o CreateCompletionMessage(Roles user, string prompt)
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

    private static Completion4oModel GetModelFromFile(string fileContent)
    {
        Completion4oModel completion4Model = null;
        completion4Model = JsonConvert.DeserializeObject<Completion4oModel>(fileContent);
        return completion4Model;
    }

}

