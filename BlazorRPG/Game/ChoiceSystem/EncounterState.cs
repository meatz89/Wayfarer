/// <summary>
/// Represents the complete state of an encounter
/// </summary>
public class EncounterState
{
    // Core resources
    public int Momentum { get; set; }
    public int Pressure { get; set; }

    // Turn tracking
    public int CurrentTurn { get; set; }
    public int MaxTurns { get; set; }
    public int AdditionalTurnsRemaining { get; set; }

    // Encounter status
    public EncounterStatus EncounterStatus { get; set; }

    // Narrative tracking
    public string NarrativePhase { get; set; }
    public List<Choice> CurrentChoices { get; set; }

    // Approach and Focus tracking dictionaries
    public Dictionary<ApproachTypes, int> ApproachTypesDic { get; private set; }
    public Dictionary<FocusTypes, int> FocusTypesDic { get; private set; }

    public EncounterState()
    {
        // Initialize defaults
        Momentum = 0;
        Pressure = 1;
        CurrentTurn = 0;
        MaxTurns = 15;
        AdditionalTurnsRemaining = 0;
        EncounterStatus = EncounterStatus.InProgress;
        CurrentChoices = new List<Choice>();

        // Initialize tracking dictionaries
        InitializeTrackingDictionaries();
    }

    private void InitializeTrackingDictionaries()
    {
        // Initialize approach tracking
        ApproachTypesDic = new Dictionary<ApproachTypes, int>();
        foreach (ApproachTypes approach in Enum.GetValues(typeof(ApproachTypes)))
        {
            ApproachTypesDic[approach] = 0;
        }

        // Initialize focus tracking
        FocusTypesDic = new Dictionary<FocusTypes, int>();
        foreach (FocusTypes focus in Enum.GetValues(typeof(FocusTypes)))
        {
            FocusTypesDic[focus] = 0;
        }
    }

    /// <summary>
    /// Get the most used approach type
    /// </summary>
    public ApproachTypes GetMostUsedApproach()
    {
        return ApproachTypesDic.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    /// <summary>
    /// Get the most used focus type
    /// </summary>
    public FocusTypes GetMostUsedFocus()
    {
        return FocusTypesDic.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    /// <summary>
    /// Create a deep copy of this encounter state
    /// </summary>
    public EncounterState Clone()
    {
        EncounterState clone = new EncounterState
        {
            Momentum = this.Momentum,
            Pressure = this.Pressure,
            CurrentTurn = this.CurrentTurn,
            MaxTurns = this.MaxTurns,
            AdditionalTurnsRemaining = this.AdditionalTurnsRemaining,
            EncounterStatus = this.EncounterStatus,
            NarrativePhase = this.NarrativePhase
        };

        // Clone choices
        clone.CurrentChoices = new List<Choice>(this.CurrentChoices);

        // Clone tracking dictionaries
        foreach (var kvp in this.ApproachTypesDic)
        {
            clone.ApproachTypesDic[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in this.FocusTypesDic)
        {
            clone.FocusTypesDic[kvp.Key] = kvp.Value;
        }

        return clone;
    }
}