/// <summary>
/// Ollama configuration with compile-time defaults.
/// HIGHLANDER: These values are the SINGLE SOURCE OF TRUTH.
/// No appsettings.json override - compile-time defaults only.
/// Environment variable OLLAMA_BASE_URL allowed for CI/CD machine-specific needs.
/// </summary>
public class OllamaConfiguration
{
    // SINGLE SOURCE OF TRUTH: Default model configuration
    // Use localhost (not 127.0.0.1) to support both IPv4 and IPv6
    // On Windows, Ollama binds to IPv6 (::1) which 127.0.0.1 cannot reach
    public const string DefaultBaseUrl = "http://localhost:11434";
    public const string DefaultModel = "gemma3:12b-it-qat";
    public const string DefaultBackupModel = "gemma3:4b";

    public string BaseUrl { get; }
    public string Model { get; }
    public string BackupModel { get; }

    /// <summary>
    /// Create configuration from compile-time defaults.
    /// Only OLLAMA_BASE_URL environment variable can override (for CI/CD).
    /// </summary>
    public OllamaConfiguration()
    {
        string baseUrlOverride = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL");
        BaseUrl = string.IsNullOrEmpty(baseUrlOverride) ? DefaultBaseUrl : baseUrlOverride;
        Model = DefaultModel;
        BackupModel = DefaultBackupModel;
    }
}
