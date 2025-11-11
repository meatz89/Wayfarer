/// <summary>
/// Service for managing development mode features and debugging tools
/// </summary>
public class DevModeService
{
    private readonly IConfiguration _configuration;
    private readonly bool _isDevMode;

    public DevModeService(IConfiguration configuration)
    {
        _configuration = configuration;

        // Check if we're in Development environment
        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        bool isDevelopment = environment == "Development";

        // Dev mode is enabled if explicitly set in config OR if in Development environment
        _isDevMode = configuration.GetValue<bool>("DevMode:Enabled", isDevelopment);
    }

    /// <summary>
    /// Whether dev mode is enabled
    /// </summary>
    public bool IsEnabled => _isDevMode;

    /// <summary>
    /// Whether to show the deck viewer button for NPCs
    /// </summary>
    public bool ShowDeckViewer => _isDevMode && _configuration.GetValue<bool>("DevMode:ShowDeckViewer", true);

    /// <summary>
    /// Whether to show debug information in the UI
    /// </summary>
    public bool ShowDebugInfo => _isDevMode && _configuration.GetValue<bool>("DevMode:ShowDebugInfo", true);

    /// <summary>
    /// Whether to show card IDs in card displays
    /// </summary>
    public bool ShowCardIds => _isDevMode && _configuration.GetValue<bool>("DevMode:ShowCardIds", true);

    /// <summary>
    /// Whether to show mechanical state information
    /// </summary>
    public bool ShowMechanicalState => _isDevMode && _configuration.GetValue<bool>("DevMode:ShowMechanicalState", true);
}