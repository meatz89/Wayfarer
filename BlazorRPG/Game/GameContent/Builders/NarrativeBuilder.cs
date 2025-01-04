public class NarrativeBuilder
{
    private BasicActionTypes actionType;
    private LocationTypes locationType;
    private LocationNames locationName;
    private LocationSpotNames locationSpotName;
    private CharacterNames narrativeCharacter;
    private NarrativeCharacterRoles narrativeCharacterRole;

    private TimeSlots timeSlot;
    private string situation;

    private NarrativeState initialState = NarrativeState.InitialState;
    private List<NarrativeStage> stages = new();


    public NarrativeBuilder ForAction(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public NarrativeBuilder ForLocationType(LocationTypes locationType)
    {
        this.locationType = locationType;
        return this;
    }

    public NarrativeBuilder ForLocation(LocationNames locationName)
    {
        this.locationName = locationName;
        return this;
    }

    public NarrativeBuilder WithCharacter(CharacterNames characterName)
    {
        this.narrativeCharacter = characterName;
        return this;
    }

    public NarrativeBuilder InCharacterRole(NarrativeCharacterRoles narrativeCharacterRole)
    {
        this.narrativeCharacterRole = narrativeCharacterRole;
        return this;
    }

    public NarrativeBuilder ForLocationSpot(LocationSpotNames locationSpotName)
    {
        this.locationSpotName = locationSpotName;
        return this;
    }

    public NarrativeBuilder WithSituation(string situation)
    {
        this.situation = situation;
        return this;
    }

    public NarrativeBuilder WithTimeSlot(TimeSlots timeSlot)
    {
        this.timeSlot = timeSlot;
        return this;
    }

    public NarrativeBuilder AddStage(Action<NarrativeStageBuilder> buildStage)
    {
        NarrativeStageBuilder builder = new NarrativeStageBuilder();
        buildStage(builder);
        stages.Add(builder.Build());
        return this;
    }

    public NarrativeBuilder WithMomentum(int momentum)
    {
        this.initialState.Momentum = momentum;
        return this;
    }

    public NarrativeBuilder WithAdvantage(int advantage)
    {
        this.initialState.Advantage = advantage;
        return this;
    }

    public NarrativeBuilder WithUnderstanding(int understanding)
    {
        this.initialState.Understanding = understanding;
        return this;
    }

    public NarrativeBuilder WithConnection(int connection)
    {
        this.initialState.Connection = connection;
        return this;
    }

    public NarrativeBuilder WithTension(int tension)
    {
        this.initialState.Tension = tension;
        return this;
    }

    public Narrative Build()
    {
        return new Narrative
        {
            ActionType = actionType,
            LocationType = locationType,
            LocationName = locationName,
            LocationSpot = locationSpotName,
            NarrativeCharacter = narrativeCharacter,
            NarrativeCharacterRole = narrativeCharacterRole,
            TimeSlot = timeSlot,
            Situation = situation,
            InitialState = initialState,
            Stages = stages
        };
    }
}
