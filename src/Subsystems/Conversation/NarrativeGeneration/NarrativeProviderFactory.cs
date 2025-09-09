using System;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Factory for selecting appropriate narrative provider based on configuration and availability.
/// Handles selection logic between AI providers and JSON fallback provider.
/// </summary>
public class NarrativeProviderFactory
{
    private readonly AIConversationNarrativeProvider _aiProvider;
    private readonly JsonNarrativeProvider _jsonProvider;
    private readonly IConfiguration _configuration;

    public NarrativeProviderFactory(
        AIConversationNarrativeProvider aiProvider,
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
    /// </summary>
    /// <returns>Available narrative provider instance</returns>
    public INarrativeProvider GetProvider()
    {
        // Check if AI narrative is enabled in configuration
        bool useAiNarrative = _configuration.GetValue<bool>("useAiNarrative", false);
        Console.WriteLine($"[NarrativeProviderFactory] useAiNarrative config: {useAiNarrative}");

        if (useAiNarrative)
        {
            // Check availability directly every time - no caching
            try
            {
                Console.WriteLine("[NarrativeProviderFactory] Checking AI availability...");
                bool isAvailable = _aiProvider.IsAvailable();
                Console.WriteLine($"[NarrativeProviderFactory] AI availability: {isAvailable}");
                
                if (isAvailable)
                {
                    Console.WriteLine("[NarrativeProviderFactory] Using AI provider");
                    return _aiProvider;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NarrativeProviderFactory] AI availability check failed: {ex.Message}");
            }
        }

        // Fall back to JSON provider (always available)
        Console.WriteLine("[NarrativeProviderFactory] Using JSON fallback provider");
        return _jsonProvider;
    }
}