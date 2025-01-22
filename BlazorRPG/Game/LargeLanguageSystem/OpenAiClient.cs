using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

public class OpenAiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _completionsUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAiClient(string apiKey)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(200);
    }

    public HttpRequestMessage CreateRequest(string json)
    {
        StringContent stringData = new StringContent(json, Encoding.UTF8, "application/json");
        return new HttpRequestMessage(HttpMethod.Post, _completionsUrl)
        {
            Content = stringData
        };
    }

    public string SendRequest(HttpRequestMessage request)
    {
        HttpResponseMessage response = _httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.Content);
            return string.Empty;
        }

        using (Stream responseStream = response.Content.ReadAsStream())
        using (StreamReader streamReader = new StreamReader(responseStream))
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
        return string.Empty;
    }
}