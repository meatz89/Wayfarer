
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
    /// Create an encounter for the Harbor Warehouse location
    /// </summary>
    public EncounterProcessor CreateHarborWarehouseEncounter()
    {
        EncounterState state = new EncounterState
        {
            MaxTurns = 5,
            MaxMomentum = 15
        };

        LocationStrategicProperties locationProperties = LocationContent.CreateHarborWarehouse();

        return new EncounterProcessor(state, locationProperties, _tagRepository);
    }

    /// <summary>
    /// Create an encounter for the Merchant Guild location
    /// </summary>
    public EncounterProcessor CreateMerchantGuildEncounter()
    {
        EncounterState state = new EncounterState
        {
            MaxTurns = 6,
            MaxMomentum = 18
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
            MaxMomentum = 12
        };

        LocationStrategicProperties locationProperties = LocationContent.CreateBanditCamp();

        return new EncounterProcessor(state, locationProperties, _tagRepository);
    }

    /// <summary>
    /// Create an encounter for the Royal Court location
    /// </summary>
    public EncounterProcessor CreateRoyalCourtEncounter()
    {
        EncounterState state = new EncounterState
        {
            MaxTurns = 7,
            MaxMomentum = 20
        };

        LocationStrategicProperties locationProperties = LocationContent.CreateRoyalCourt();

        return new EncounterProcessor(state, locationProperties, _tagRepository);
    }

    /// <summary>
    /// Create an encounter for the Wilderness location
    /// </summary>
    public EncounterProcessor CreateWildernessEncounter()
    {
        EncounterState state = new EncounterState
        {
            MaxTurns = 5,
            MaxMomentum = 15
        };

        LocationStrategicProperties locationProperties = LocationContent.CreateWilderness();

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
            MaxMomentum = maxMomentum
        };

        return new EncounterProcessor(state, locationProperties, _tagRepository);
    }
}
