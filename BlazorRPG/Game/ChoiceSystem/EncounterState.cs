/// <summary>
/// Tracks the state of an Encounter
/// </summary>
public class EncounterState
{
    // Core resources
    public int Momentum { get; private set; }
    public int Pressure { get; private set; }

    // All approach tag values
    public Dictionary<ApproachTypes, int> ApproachTypesDic { get; set; }

    // All focus tag values
    public Dictionary<FocusTypes, int> FocusTypesDic { get; set; }

    // Balance state (Stable or Unstable)
    public bool IsStable => Pressure <= Momentum;

    /// <summary>
    /// Creates a new Encounter state with optional initial values
    /// </summary>
    public EncounterState(int momentum = 0, int pressure = 0)
    {
        Momentum = momentum;
        Pressure = pressure;

        // Initialize all approach tags to 0
        ApproachTypesDic = new Dictionary<ApproachTypes, int>();
        foreach (ApproachTypes tag in Enum.GetValues(typeof(ApproachTypes)))
        {
            ApproachTypesDic[tag] = 0;
        }

        // Initialize all focus tags to 0
        FocusTypesDic = new Dictionary<FocusTypes, int>();
        foreach (FocusTypes tag in Enum.GetValues(typeof(FocusTypes)))
        {
            FocusTypesDic[tag] = 0;
        }
    }

    /// <summary>
    /// Sets initial tag values for specific Encounter types
    /// </summary>
    public void SetInitialTags(Dictionary<ApproachTypes, int> ApproachTypess, Dictionary<FocusTypes, int> FocusTypess)
    {
        foreach (KeyValuePair<ApproachTypes, int> pair in ApproachTypess)
        {
            ApproachTypess[pair.Key] = pair.Value;
        }

        foreach (KeyValuePair<FocusTypes, int> pair in FocusTypess)
        {
            FocusTypess[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Apply a choice's effects to the Encounter state
    /// </summary>
    public EncounterState ApplyChoice(Choice choice)
    {
        EncounterState oldState = this;
        EncounterState newState = new EncounterState(Momentum, Pressure);
        newState.ApproachTypesDic = new Dictionary<ApproachTypes, int>(ApproachTypesDic);
        newState.FocusTypesDic = new Dictionary<FocusTypes, int>(FocusTypesDic);

        // Apply effect (momentum or pressure)
        if (choice.EffectType == EffectTypes.Momentum)
        {
            // Base: +1 momentum
            // Unstable State: +2 momentum instead
            newState.Momentum += newState.IsStable ? 1 : 2;
        }
        else
        {
            // Always +1 pressure
            newState.Pressure += 1;
        }

        // Increase approach tag
        newState.ApproachTypesDic[choice.ApproachType] += 1;

        // Increase focus tag
        newState.FocusTypesDic[choice.FocusType] += 1;

        return newState;
    }

    /// <summary>
    /// Momentum to Pressure ratio used for choice distribution
    /// </summary>
    public double MomentumToPressureRatio
    {
        get
        {
            if (Pressure == 0)
                return Momentum > 0 ? double.PositiveInfinity : 1.0;

            double value = 5;
            
            if(Pressure > 0)
                Math.Clamp((double)Momentum / Pressure, 0, 5);

            return value;
        }
    }

    /// <summary>
    /// Determine whether the Encounter has succeeded or failed
    /// </summary>
    public (bool Succeeded, string Result) GetOutcome(Location location)
    {
        // Check for failure condition
        if (Pressure >= 10)
            return (false, "Failure");

        // Compare momentum to thresholds
        int momentum = Momentum;

        if (momentum < location.FailThreshold)
            return (false, "Failure");
        else if (momentum < location.PartialSuccessThreshold)
            return (true, "Partial Success");
        else if (momentum < location.StandardSuccessThreshold)
            return (true, "Standard Success");
        else
            return (true, "Exceptional Success");
    }
}
