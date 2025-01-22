using Newtonsoft.Json;

public static class OpenAiHelper
{
    private static string openAiApiKey;
    private static OpenAiClient openAiClient;

    public static void SetClient(OpenAiClient client)
    {
        if (openAiClient == null)
        {
            openAiClient = client;
        }
    }

    public static HttpRequestMessage CreateOpenAiRequest(string fileContent, List<CompletionMessage4o> completionMessages)
    {
        Completion4oModel modelFromFile = GetModelFromFile(fileContent);
        List<CompletionMessage4o> messages = modelFromFile.messages;
        return CreateRequest(completionMessages, openAiApiKey, modelFromFile, messages);
    }

    private static HttpRequestMessage CreateRequest(List<CompletionMessage4o> completionMessages, string openAiApiKey, Completion4oModel modelFromFile, List<CompletionMessage4o> messages)
    {
        messages.AddRange(completionMessages);

        Console.WriteLine("");
        Console.WriteLine("----START Messages sent to API START----");
        foreach (CompletionMessage4o message in messages)
        {
            foreach (CompletionMessageContent4o content in message.content)
            {
                Console.WriteLine("");
                Console.WriteLine("----New Prompt----");
                Console.WriteLine($"{message.role}:");
                Console.WriteLine($"{content.text}");
                Console.WriteLine("");
            }
        }
        Console.WriteLine("----END Messages sent to API END----");
        Console.WriteLine("");

        //Converting the object to a json string
        string json = JsonConvert.SerializeObject(modelFromFile);
        return openAiClient.CreateRequest(json);
    }

    public static string SendRequest(HttpRequestMessage request)
    {
        const string openAiCallStarted = $"\r\nCalling OpenAI API\r\n";
        Console.WriteLine(openAiCallStarted);

        return openAiClient.SendRequest(request);
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

