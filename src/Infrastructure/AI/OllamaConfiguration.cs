/// <summary>
/// Ollama configuration with compile-time defaults.
/// ARCHITECTURAL GUARANTEE: Default values are defined HERE in code,
/// ensuring tests and production use the same model configuration.
/// appsettings.json can OVERRIDE but the defaults are the source of truth.
/// </summary>
public class OllamaConfiguration
{
    // SINGLE SOURCE OF TRUTH: Default model configuration
    // Tests and production both use these defaults if not overridden
    // Use localhost (not 127.0.0.1) to support both IPv4 and IPv6
    // On Windows, Ollama may bind to IPv6 (::1) which 127.0.0.1 won't reach
    public const string DefaultBaseUrl = "http://localhost:11434";
    public const string DefaultModel = "gemma3:12b-it-qat";
    public const string DefaultBackupModel = "gemma3:4b";

    public string BaseUrl { get; set; }
    public string Model { get; set; }
    public string BackupModel { get; set; }

    /// <summary>
    /// Create configuration with defaults (for tests - production parity guaranteed)
    /// Respects OLLAMA_BASE_URL environment variable for machine-specific URL override.
    /// </summary>
    public OllamaConfiguration()
    {
        string baseUrl = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL");
        BaseUrl = string.IsNullOrEmpty(baseUrl) ? DefaultBaseUrl : baseUrl;
        Model = DefaultModel;
        BackupModel = DefaultBackupModel;
    }

    /// <summary>
    /// Create configuration from IConfiguration (for production DI)
    /// Falls back to compile-time defaults if config values are missing
    /// </summary>
    public OllamaConfiguration(IConfiguration config)
    {
        // Check environment variable first, then config, then compile-time default
        string baseUrl = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL");

        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = config["Ollama:BaseUrl"];
        }

        BaseUrl = string.IsNullOrEmpty(baseUrl) ? DefaultBaseUrl : baseUrl;
        Model = config["Ollama:Model"] ?? DefaultModel;
        BackupModel = config["Ollama:BackupModel"] ?? DefaultBackupModel;
    }
}