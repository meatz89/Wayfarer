/// <summary>
/// Represents the current status of an encounter
/// </summary>
public class EncounterStatusModel
{
    public int MaxMomentum { get; set; }
    public int MaxPressure { get; set; }
    public int SuccessThreshold { get; set; }
    public int MaxTurns { get; set; }

    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Concentration { get; set; }
    public int MaxConcentration { get; set; }

    public int CurrentTurn { get; }
    public int Momentum { get; }
    public int Pressure { get; }

    public List<string> ActiveTagNames { get; }
    public WorldState WorldState { get; }
    public PlayerState PlayerState { get; }

    // Added properties
    public List<IEncounterTag> ActiveTags { get; }
    public Encounter EncounterInfo { get; }
    public EncounterCategories EncounterType
    {
        get
        {
            return EncounterInfo?.EncounterType ?? EncounterCategories.Persuasion;
        }
    }

    public EncounterStatusModel(
        int currentTurn,
        int maxMomentum,
        int maxPressure,
        int successThreshold,
        int maxTurns,
        int momentum,
        int pressure,
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

        ActiveTagNames = activeTagNames;
        PlayerState = playerState;

        // Initialize empty collections for the added properties
        ActiveTags = new List<IEncounterTag>();
        WorldState = worldState;
    }

}
