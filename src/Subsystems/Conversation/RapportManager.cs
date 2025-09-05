using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages rapport system which modifies success rates for all cards.
/// Rapport starts based on connection tokens and is modified by card effects.
/// Each rapport point provides +1% success chance on all cards.
/// </summary>
public class RapportManager
{
    private int currentRapport = 0;
    private const int TOKEN_TO_RAPPORT_MULTIPLIER = 3;

    public int CurrentRapport => currentRapport;

    /// <summary>
    /// Initialize rapport manager with starting tokens
    /// </summary>
    /// <param name="tokens">Dictionary of connection types and their token counts</param>
    public RapportManager(Dictionary<ConnectionType, int> tokens)
    {
        currentRapport = tokens.Values.Sum() * TOKEN_TO_RAPPORT_MULTIPLIER;
    }

    /// <summary>
    /// Apply rapport changes from cards
    /// </summary>
    /// <param name="change">The base rapport change</param>
    /// <param name="atmosphere">Current conversation atmosphere for modifiers</param>
    public void ApplyRapportChange(int change, AtmosphereType atmosphere = AtmosphereType.Neutral)
    {
        // Apply atmosphere modifiers if needed
        int modified = ModifyByAtmosphere(change, atmosphere);
        currentRapport += modified;
    }
    /// <summary>
    /// Scale rapport change by current flow level
    /// </summary>
    /// <param name="currentFlow">Current flow value</param>
    /// <returns>Scaled rapport value (better rapport when flow is negative)</returns>
    public int ScaleRapportByFlow(int currentFlow)
    {
        return 4 - currentFlow; // Example: Better rapport when flow is negative
    }

    /// <summary>
    /// Scale rapport change by patience level
    /// </summary>
    /// <param name="patience">Current patience value</param>
    /// <returns>Scaled rapport value</returns>
    public int ScaleRapportByPatience(int patience)
    {
        return patience / 3; // Example scaling
    }

    /// <summary>
    /// Scale rapport change by remaining focus
    /// </summary>
    /// <param name="remainingFocus">Remaining focus points</param>
    /// <returns>Scaled rapport value</returns>
    public int ScaleRapportByFocus(int remainingFocus)
    {
        return remainingFocus;
    }

    /// <summary>
    /// Get current success modifier for all cards
    /// </summary>
    /// <returns>Success modifier percentage (each point = 1%)</returns>
    public int GetSuccessModifier()
    {
        return currentRapport; // Each point = 1% success modifier
    }

    /// <summary>
    /// Reset rapport to starting value based on tokens
    /// </summary>
    /// <param name="tokens">Dictionary of connection types and their token counts</param>
    public void ResetRapport(Dictionary<ConnectionType, int> tokens)
    {
        currentRapport = tokens.Values.Sum() * TOKEN_TO_RAPPORT_MULTIPLIER;
    }

    /// <summary>
    /// Apply atmosphere modifiers to rapport changes
    /// </summary>
    /// <param name="baseChange">Base rapport change</param>
    /// <param name="atmosphere">Current atmosphere</param>
    /// <returns>Modified rapport change</returns>
    private int ModifyByAtmosphere(int baseChange, AtmosphereType atmosphere)
    {
        return atmosphere switch
        {
            AtmosphereType.Volatile => baseChange > 0 ? baseChange + 1 : (baseChange < 0 ? baseChange - 1 : 0),
            AtmosphereType.Exposed => baseChange * 2,
            AtmosphereType.Synchronized => baseChange * 2, // Handled by effect doubling
            _ => baseChange
        };
    }

    /// <summary>
    /// Get a visual representation of current rapport
    /// </summary>
    /// <returns>String representation for UI display</returns>
    public string GetRapportDisplay()
    {
        string sign = currentRapport > 0 ? "+" : "";
        return $"{sign}{currentRapport}";
    }

    /// <summary>
    /// Get rapport effect description for UI
    /// </summary>
    /// <returns>Description of rapport effect on success rates</returns>
    public string GetRapportEffectDescription()
    {
        if (currentRapport == 0)
            return "No effect on success rates";

        string sign = currentRapport > 0 ? "+" : "";
        return $"{sign}{currentRapport}% to all cards";
    }
}