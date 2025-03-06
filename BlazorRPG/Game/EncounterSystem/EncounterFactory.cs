
/// <summary>
/// Factory to create complete encounter systems for different locations
/// </summary>
public class EncounterFactory
{
    private readonly ChoiceRepository _choiceRepository;
    private readonly EncounterTagRepository _tagRepository;

    public EncounterFactory()
    {
        _choiceRepository = new ChoiceRepository();
        _tagRepository = new EncounterTagRepository();
    }

    /// <summary>
    /// Create an encounter for the Merchant Guild location
    /// </summary>
    public EncounterProcessor CreateMerchantGuildEncounter()
    {
        EncounterState state = new EncounterState
        {
            MaxTurns = 6,
        };

        LocationStrategicProperties locationProperties = LocationContent.CreateMerchantGuild();

        return new EncounterProcessor(state, locationProperties, _tagRepository);
    }

    /// <summary>
    /// Create an encounter for the Bandit Camp location
    /// </summary>
    public EncounterProcessor CreateBanditCampEncounter()
    {
        EncounterState state = new EncounterState
        {
            MaxTurns = 4,
        };

        LocationStrategicProperties locationProperties = LocationContent.CreateBanditCamp();

        return new EncounterProcessor(state, locationProperties, _tagRepository);
    }

    /// <summary>
    /// Create a custom encounter
    /// </summary>
    public EncounterProcessor CreateCustomEncounter(LocationStrategicProperties locationProperties, int maxTurns, int maxMomentum)
    {
        EncounterState state = new EncounterState
        {
            MaxTurns = maxTurns,
        };

        return new EncounterProcessor(state, locationProperties, _tagRepository);
    }
}
