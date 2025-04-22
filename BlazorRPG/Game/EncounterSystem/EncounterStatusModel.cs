/// <summary>
/// Represents the current status of an encounter
/// </summary>
public class EncounterStatusModel
{
    public int MaxMomentum { get; set; } = 20;
    public int MaxPressure { get; set; } = 15;
    public int SuccessThreshold { get; set; } = 15;
    public int MaxTurns { get; set; } = 6;

    public int Health { get; set; } = 10;
    public int MaxHealth { get; set; } = 10;
    public int Confidence { get; set; } = 10;
    public int MaxConfidence { get; set; } = 10;
    public int Concentration { get; set; } = 10;
    public int MaxConcentration { get; set; } = 10;

    public int CurrentTurn { get; }
    public int Momentum { get; }
    public int Pressure { get; }

    public Dictionary<ApproachTags, int> ApproachTags { get; }
    public Dictionary<FocusTags, int> FocusTags { get; }
    public List<string> ActiveTagNames { get; }
    public WorldState WorldState { get; }
    public PlayerState PlayerState { get; }

    // Added properties
    public List<IEncounterTag> ActiveTags { get; }
    public Encounter EncounterInfo { get; }
    public EncounterTypes EncounterType => EncounterInfo?.EncounterType ?? EncounterTypes.Physical;


    public EncounterStatusModel(
        int currentTurn,
        int maxMomentum,
        int maxPressure,
        int successThreshold,
        int maxTurns,
        int momentum,
        int pressure,
        Dictionary<ApproachTags, int> approachTags,
        Dictionary<FocusTags, int> focusTags,
        List<string> activeTagNames,
        PlayerState playerState,
        WorldState worldState)
    {
        CurrentTurn = currentTurn;

        MaxMomentum = maxMomentum;
        MaxPressure = maxPressure;
        SuccessThreshold = successThreshold;
        MaxTurns = maxTurns;
        Momentum = momentum;
        Pressure = pressure;

        Health = playerState.Health;
        MaxHealth = playerState.MaxHealth;
        Concentration = playerState.Concentration;
        MaxConcentration = playerState.MaxConcentration;
        Confidence = playerState.Confidence;
        MaxConfidence = playerState.MaxConfidence;

        ApproachTags = approachTags;
        FocusTags = focusTags;
        ActiveTagNames = activeTagNames;
        PlayerState = playerState;

        // Initialize empty collections for the added properties
        ActiveTags = new List<IEncounterTag>();
        WorldState = worldState;
    }

}
