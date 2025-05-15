/// <summary>
/// Represents the outcome of the player character (PC)'s choice
/// </summary>
public class ChoiceOutcome
{
    public string Description { get; }
    public bool IsEncounterOver { get; }
    public EncounterOutcomes Outcome { get; }

    public int MomentumGain { get; }
    public int PressureGain { get; }

    public int HealthChange { get; }
    public int ConcentrationChange { get; }

    public ChoiceOutcome(
        int momentumGained,
        int pressureBuilt,
        string description,
        bool isEncounterOver,
        EncounterOutcomes outcome,
        int healthChange = 0,
        int concentrationChange = 0)
    {
        MomentumGain = momentumGained;
        PressureGain = pressureBuilt;
        Description = description;
        IsEncounterOver = isEncounterOver;
        Outcome = outcome;
        HealthChange = healthChange;
        ConcentrationChange = concentrationChange;
    }
}
