using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

/// <summary>
/// Factory for selecting appropriate narrative provider based on configuration and availability.
/// Handles selection logic between AI providers and JSON fallback provider.
/// </summary>
public class NarrativeProviderFactory
{
    private readonly AINarrativeProvider _aiProvider;
    private readonly JsonNarrativeProvider _jsonProvider;
    private readonly IConfiguration _configuration;

    public NarrativeProviderFactory(
        AINarrativeProvider aiProvider,
        JsonNarrativeProvider jsonProvider,
        IConfiguration configuration)
    {
        _aiProvider = aiProvider;
        _jsonProvider = jsonProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the appropriate narrative provider based on configuration and availability.
    /// First checks if AI narrative is enabled and available, then falls back to JSON provider.
    /// PRINCIPLE: Always use async/await for I/O operations to prevent deadlocks.
    /// </summary>
    /// <returns>Available narrative provider instance</returns>
    public async Task<INarrativeProvider> GetProviderAsync()
    {
        // Check if AI narrative is enabled in configuration
        bool useAiNarrative = _configuration.GetValue<bool>("useAiNarrative", false);if (useAiNarrative)
        {
            // Check availability directly every time - no caching
            bool isAvailable = await _aiProvider.IsAvailableAsync();if (isAvailable)
            {return _aiProvider;
            }
        }

        // Fall back to JSON provider (always available)return _jsonProvider;
    }
}