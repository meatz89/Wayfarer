
/// <summary>
/// Represents a projection of a choice's effects
/// </summary>
public class ChoiceProjection
{
    // Current state
    public int CurrentMomentum { get; set; }
    public int CurrentPressure { get; set; }
    public StrategicSignature CurrentSignature { get; set; }

    // Base changes
    public int BaseMomentumChange { get; set; }
    public int BasePressureChange { get; set; }

    // Tag effect modifiers
    public Dictionary<string, int> TagMomentumEffects { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> TagPressureEffects { get; set; } = new Dictionary<string, int>();

    // Special effects that were applied
    public List<string> SpecialEffects { get; set; } = new List<string>();

    // Special game mechanics flags
    public bool GrantsAdditionalTurn { get; set; } = false;

    // Projected signature
    public StrategicSignature ProjectedSignature { get; set; }

    // Tag activations/deactivations
    public List<EncounterTag> TagsActivated { get; set; } = new List<EncounterTag>();
    public List<EncounterTag> TagsDeactivated { get; set; } = new List<EncounterTag>();

    // Cumulative trigger updates
    public Dictionary<string, int> CumulativeTriggerChanges { get; set; } = new Dictionary<string, int>();

    // Total changes
    public int MomentumChange { get; set; }
    public int PressureChange { get; set; }

    // Projected values
    public int ProjectedMomentum { get; set; }
    public int ProjectedPressure { get; set; }

    /// <summary>
    /// Generate a detailed explanation of this projection for debugging or UI
    /// </summary>
    public Dictionary<string, string> GenerateExplanation()
    {
        Dictionary<string, string> explanation = new Dictionary<string, string>();

        // Base momentum change
        explanation["Base Momentum"] = $"{BaseMomentumChange}";

        // Tag momentum effects
        if (TagMomentumEffects.Count > 0)
        {
            foreach (var effect in TagMomentumEffects)
            {
                explanation[$"Tag: {effect.Key}"] = $"Momentum {(effect.Value >= 0 ? "+" : "")}{effect.Value}";
            }
        }

        // Base pressure change
        explanation["Base Pressure"] = $"{BasePressureChange}";

        // Tag pressure effects
        if (TagPressureEffects.Count > 0)
        {
            foreach (var effect in TagPressureEffects)
            {
                explanation[$"Tag: {effect.Key}"] = $"Pressure {(effect.Value >= 0 ? "+" : "")}{effect.Value}";
            }
        }

        // End-of-turn pressure
        explanation["End-of-Turn Pressure"] = "+1";

        // Special effects
        if (SpecialEffects.Count > 0)
        {
            explanation["Special Effects"] = string.Join(", ", SpecialEffects);
        }

        // Additional turn info
        if (GrantsAdditionalTurn)
        {
            explanation["Additional Turn"] = "Yes";
        }

        // Total changes
        explanation["Total Momentum Change"] = $"{MomentumChange}";
        explanation["Total Pressure Change"] = $"{PressureChange}";

        // Added tags
        if (TagsActivated.Count > 0)
        {
            explanation["Tags Activated"] = string.Join(", ", TagsActivated.Select(t => t.Name));
        }

        // Removed tags
        if (TagsDeactivated.Count > 0)
        {
            explanation["Tags Deactivated"] = string.Join(", ", TagsDeactivated.Select(t => t.Name));
        }

        return explanation;
    }

    /// <summary>
    /// Apply this projection to an encounter state
    /// </summary>
    public void ApplyToState(EncounterState state)
    {
        // Update state values
        state.Momentum = ProjectedMomentum;
        state.Pressure = ProjectedPressure;

        // Handle additional turn if granted
        if (GrantsAdditionalTurn)
        {
            state.AdditionalTurnsRemaining++;
        }
    }
}