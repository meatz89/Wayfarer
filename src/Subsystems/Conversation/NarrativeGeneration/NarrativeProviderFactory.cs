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
    
    // Cache the availability check result with timestamp
    private bool? _cachedAvailability;
    private DateTime _lastAvailabilityCheck = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromSeconds(30);

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
            // Check cached availability first
            bool isAvailable = CheckAIAvailabilityWithCache();
            if (isAvailable)
            {
                return _aiProvider;
            }
        }

        // Fall back to JSON provider (always available)
        return _jsonProvider;
    }
    
    private bool CheckAIAvailabilityWithCache()
    {
        DateTime now = DateTime.UtcNow;
        
        // If we have a recent cached result, use it
        if (_cachedAvailability.HasValue && 
            (now - _lastAvailabilityCheck) < _cacheTimeout)
        {
            return _cachedAvailability.Value;
        }
        
        // Otherwise, check availability and cache the result
        try
        {
            bool isAvailable = _aiProvider.IsAvailable();
            _cachedAvailability = isAvailable;
            _lastAvailabilityCheck = now;
            return isAvailable;
        }
        catch
        {
            // If check fails, cache negative result
            _cachedAvailability = false;
            _lastAvailabilityCheck = now;
            return false;
        }
    }
}