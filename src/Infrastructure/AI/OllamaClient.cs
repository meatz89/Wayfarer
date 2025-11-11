using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

public class OllamaClient
{
    private readonly HttpClient httpClient;
    private readonly OllamaConfiguration configuration;

    public OllamaClient(HttpClient httpClient, OllamaConfiguration configuration)
    {
        this.httpClient = httpClient;
        this.configuration = configuration;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        OllamaRequest request = new OllamaRequest
        {
            Model = configuration.Model,
            Prompt = prompt,
            Stream = true
        };

        string requestJson = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{configuration.BaseUrl}/api/generate")
        {
            Content = content
        };

        HttpResponseMessage response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        Stream responseStream = await response.Content.ReadAsStreamAsync();
        StreamReader reader = new StreamReader(responseStream);

        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            OllamaStreamResponse streamResponse = JsonSerializer.Deserialize<OllamaStreamResponse>(line, new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true });

            if (streamResponse.Done)
                break;

            if (!string.IsNullOrEmpty(streamResponse.Response))
                yield return streamResponse.Response;
        }
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        // Use a reasonable timeout for health checks
        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(2)); // 2 seconds for health check

        string healthUrl = $"{configuration.BaseUrl}/api/tags"; HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, healthUrl);
        HttpResponseMessage response = await httpClient.SendAsync(request, cts.Token);

        bool isHealthy = response.IsSuccessStatusCode; return isHealthy;
    }
}