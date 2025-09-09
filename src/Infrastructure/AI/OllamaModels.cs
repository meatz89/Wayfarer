using System.Text.Json.Serialization;

public class OllamaRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
    
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }
    
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}

public class OllamaResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; }
    
    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

public class OllamaStreamResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; }
    
    [JsonPropertyName("done")]
    public bool Done { get; set; }
}