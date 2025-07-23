using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Interface for narrative effects that can be applied when narrative steps complete
/// </summary>
public interface INarrativeEffect
{
    /// <summary>
    /// Apply the effect to the game world
    /// </summary>
    /// <param name="world">The game world to modify</param>
    /// <param name="parameters">Effect-specific parameters from JSON configuration</param>
    /// <returns>Result of applying the effect</returns>
    Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters);
    
    /// <summary>
    /// Validate that the effect can be applied with the given parameters
    /// </summary>
    /// <param name="parameters">Effect parameters to validate</param>
    /// <returns>True if parameters are valid</returns>
    bool ValidateParameters(Dictionary<string, object> parameters);
    
    /// <summary>
    /// Get the effect type identifier
    /// </summary>
    string EffectType { get; }
}

/// <summary>
/// Result of applying a narrative effect
/// </summary>
public class NarrativeEffectResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new Dictionary<string, object>();
    
    public static NarrativeEffectResult Succeeded(string message = null)
    {
        return new NarrativeEffectResult { Success = true, Message = message };
    }
    
    public static NarrativeEffectResult Failed(string message)
    {
        return new NarrativeEffectResult { Success = false, Message = message };
    }
}