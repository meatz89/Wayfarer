public class EncounterBuilder
{
    private BasicActionTypes actionType;
    private LocationTypes locationType;
    private LocationNames locationName;
    private CharacterNames encounterCharacter;
    private CharacterRoleTypes encounterCharacterRole;

    private TimeSlots timeSlot;
    private string situation;

    private EncounterStateValues initialState = EncounterStateValues.InitialState;
    private List<EncounterStage> stages = new();


    public EncounterBuilder ForAction(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public EncounterBuilder ForLocationType(LocationTypes locationType)
    {
        this.locationType = locationType;
        return this;
    }

    public EncounterBuilder ForLocation(LocationNames locationName)
    {
        this.locationName = locationName;
        return this;
    }

    public EncounterBuilder WithCharacter(CharacterNames characterName)
    {
        this.encounterCharacter = characterName;
        return this;
    }

    public EncounterBuilder InCharacterRole(CharacterRoleTypes encounterCharacterRole)
    {
        this.encounterCharacterRole = encounterCharacterRole;
        return this;
    }

    public EncounterBuilder WithSituation(string situation)
    {
        this.situation = situation;
        return this;
    }

    public EncounterBuilder WithTimeSlot(TimeSlots timeSlot)
    {
        this.timeSlot = timeSlot;
        return this;
    }

    public EncounterBuilder AddStage(Action<EncounterStageBuilder> buildStage)
    {
        EncounterStageBuilder builder = new EncounterStageBuilder();
        buildStage(builder);
        stages.Add(builder.Build());
        return this;
    }

    public EncounterBuilder WithMomentum(int momentum)
    {
        this.initialState.Momentum = momentum;
        return this;
    }

    public EncounterBuilder WithAdvantage(int advantage)
    {
        this.initialState.Advantage = advantage;
        return this;
    }

    public EncounterBuilder WithUnderstanding(int understanding)
    {
        this.initialState.Understanding = understanding;
        return this;
    }

    public EncounterBuilder WithConnection(int connection)
    {
        this.initialState.Connection = connection;
        return this;
    }

    public EncounterBuilder WithTension(int tension)
    {
        this.initialState.Tension = tension;
        return this;
    }

    public Encounter Build()
    {
        return new Encounter
        {
            ActionType = actionType,
            LocationType = locationType,
            LocationName = locationName,
            EncounterCharacter = encounterCharacter,
            EncounterCharacterRole = encounterCharacterRole,
            TimeSlot = timeSlot,
            Situation = situation,
            InitialState = initialState,
            Stages = stages
        };
    }
}
