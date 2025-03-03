/// <summary>
/// Represents a complete projection of what would happen if a choice is made
/// </summary>
public class ChoiceProjection
{
    // Current values
    public int CurrentMomentum { get; set; }
    public int CurrentPressure { get; set; }

    // Projected values
    public int ProjectedMomentum { get; set; }
    public int ProjectedPressure { get; set; }

    // Changes
    public int MomentumChange { get; set; }
    public int PressureChange { get; set; }

    // Component breakdown of changes
    public int BaseMomentumChange { get; set; }
    public int BasePressureChange { get; set; }
    public Dictionary<string, int> TagMomentumEffects { get; set; }
    public Dictionary<string, int> TagPressureEffects { get; set; }

    // Tag changes
    public List<EncounterTag> TagsActivated { get; set; }
    public List<EncounterTag> TagsDeactivated { get; set; }

    // Signature changes
    public StrategicSignature CurrentSignature { get; set; }
    public StrategicSignature ProjectedSignature { get; set; }

    public ChoiceProjection()
    {
        TagMomentumEffects = new Dictionary<string, int>();
        TagPressureEffects = new Dictionary<string, int>();
        TagsActivated = new List<EncounterTag>();
        TagsDeactivated = new List<EncounterTag>();
    }
}
