public class OllamaConfiguration
{
public string BaseUrl { get; set; }
public string Model { get; set; }
public string BackupModel { get; set; }

public OllamaConfiguration(IConfiguration config)
{
    // Check environment variable first, then fall back to config
    string baseUrl = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL");

    if (string.IsNullOrEmpty(baseUrl))
    {
        // Use default from config - Development.json will override for WSL
        baseUrl = config["Ollama:BaseUrl"];
    }

    BaseUrl = baseUrl;
    Model = config["Ollama:Model"];
    BackupModel = config["Ollama:BackupModel"];
}
}