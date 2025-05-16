public class EncounterState
{
    public static EncounterState PreviousEncounterState { get; set; }
    public NarrativeChoice PreviousChoice { get; set; }
    public int PreviousMomentum { get; set; }
    public int PreviousPressure { get; set; }
    public int Momentum { get; private set; }
    public int Pressure { get; private set; }
    public int CurrentTurn { get; private set; }
    public Encounter EncounterInfo { get; }
    public LocationSpot LocationSpot { get; set; }

    private readonly TagManager _tagManager;
    private readonly ProjectionService _projectionService;

    public List<IEncounterTag> ActiveTags
    {
        get
        {
            return _tagManager.EncounterTags;
        }
    }

    public EncounterState(
        Encounter encounterInfo,
        PlayerState playerState)
    {
        Momentum = 5;
        Pressure = encounterInfo.EncounterDifficulty + 3;
        CurrentTurn = 0;
        EncounterInfo = encounterInfo;

        _projectionService = new ProjectionService(encounterInfo, playerState);
        _tagManager = new TagManager();

        PreviousEncounterState = this;
    }

    public static EncounterState CreateDeepCopy(
        EncounterState originalState,
        PlayerState playerState)
    {
        // Create a fresh EncounterState with the same encounter info
        EncounterState copy = new EncounterState(
            originalState.EncounterInfo,
            playerState.Clone());

        // Copy the current state values
        copy.Momentum = originalState.Momentum;
        copy.Pressure = originalState.Pressure;
        copy.CurrentTurn = originalState.CurrentTurn;
        copy.LocationSpot = originalState.LocationSpot;

        return copy;
    }

    public ChoiceProjection ApplyChoice(PlayerState playerState, Encounter encounterInfo, NarrativeChoice choice)
    {
        // Store the current state before making changes
        this.UpdateStateHistory(choice);

        // Then apply the choice as normal
        ChoiceProjection projection = CreateChoiceProjection(choice);
        ApplyChoiceProjection(playerState, encounterInfo, projection);

        return projection;
    }

    private void ApplyChoiceProjection(PlayerState playerState, Encounter encounterInfo, ChoiceProjection projection)
    {
        Momentum = projection.FinalMomentum;
        Pressure = projection.FinalPressure;

        CurrentTurn++;
    }

    public void UpdateStateHistory(NarrativeChoice selectedChoice)
    {
        // Store previous choice
        PreviousChoice = selectedChoice;

        // Store previous values
        PreviousMomentum = Momentum;
        PreviousPressure = Pressure;
    }

    public void BuildMomentum(int amount)
    {
        Momentum += amount;
    }

    public void BuildPressure(int amount)
    {
        Pressure += amount;
    }

    public void ReducePressure(int amount)
    {
        Pressure = Math.Max(0, Pressure - amount);
    }

    public ChoiceProjection CreateChoiceProjection(NarrativeChoice choice)
    {
        return _projectionService.CreateChoiceProjection(
            choice,
            Momentum,
            Pressure,
            CurrentTurn);
    }

    public List<string> GetActiveTagsNames()
    {
        if (ActiveTags == null) return new List<string>();

        List<string> list = ActiveTags.Select(t =>
        {
            return t.NarrativeName;
        }).ToList();

        if (list == null) return new List<string>();
        return list;
    }

}
