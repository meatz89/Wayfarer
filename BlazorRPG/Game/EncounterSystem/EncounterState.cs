public class EncounterState
{
    // Added properties for history tracking
    public IChoice PreviousChoice { get; set; }
    public int PreviousMomentum { get; set; }
    public int PreviousPressure { get; set; }
    public Dictionary<ApproachTags, int> PreviousApproachValues { get; set; } = new Dictionary<ApproachTags, int>();
    public Dictionary<FocusTags, int> PreviousFocusValues { get; set; } = new Dictionary<FocusTags, int>();

    public int Momentum { get; private set; }
    public int Pressure { get; private set; }
    public int CurrentTurn { get; private set; }
    public EncounterInfo Location { get; }
    public LocationSpot LocationSpot { get; internal set; }
    public PlayerState PlayerState { get; }

    // Expose tag system through the TagManager
    public BaseTagSystem TagSystem => _tagManager.TagSystem;
    public List<IEncounterTag> ActiveTags => _tagManager.ActiveTags;

    private readonly TagManager _tagManager;
    private readonly ResourceManager _resourceManager;
    private readonly ProjectionService _projectionService;
    private IChoice _lastChoice;

    public EncounterState(EncounterInfo encounterInfo, PlayerState playerState)
    {
        Momentum = 0;
        Pressure = 0;
        CurrentTurn = 0;
        Location = encounterInfo;
        PlayerState = playerState;

        // Initialize managers
        _tagManager = new TagManager();
        _resourceManager = new ResourceManager(playerState, encounterInfo);
        _projectionService = new ProjectionService(_tagManager, _resourceManager, encounterInfo);
    }

    // Forward methods used by IEncounterTag.ApplyEffect and Choice.ApplyChoice
    public void AddFocusMomentumBonus(FocusTags focus, int bonus) =>
        _tagManager.AddFocusMomentumBonus(focus, bonus);

    public void AddFocusPressureModifier(FocusTags focus, int modifier) =>
        _tagManager.AddFocusPressureModifier(focus, modifier);

    public void AddEndOfTurnPressureReduction(int reduction) =>
        _tagManager.AddEndOfTurnPressureReduction(reduction);

    public int GetTotalMomentum(IChoice choice, int baseMomentum) =>
        _tagManager.GetTotalMomentum(choice, baseMomentum);

    public int GetTotalPressure(IChoice choice, int basePressure) =>
        _tagManager.GetTotalPressure(choice, basePressure);

    public ChoiceProjection ApplyChoice(IChoice choice)
    {
        // Store the current state before making changes
        this.UpdateStateHistory(choice);

        // Then apply the choice as normal
        ChoiceProjection projection = CreateChoiceProjection(choice);
        ApplyChoiceProjection(projection);

        return projection;
    }

    private void ApplyChoiceProjection(ChoiceProjection projection)
    {
        _lastChoice = projection.Choice;

        // Apply resource changes from pressure at start of turn (based on current pressure)
        _resourceManager.ApplyPressureResourceDamage(Pressure);

        // 1. Apply tag changes
        foreach (KeyValuePair<ApproachTags, int> pair in projection.EncounterStateTagChanges)
            TagSystem.ModifyEncounterStateTag(pair.Key, pair.Value);

        foreach (KeyValuePair<FocusTags, int> pair in projection.FocusTagChanges)
            TagSystem.ModifyFocusTag(pair.Key, pair.Value);

        // 2. Apply exactly the values from the projection
        Momentum = projection.FinalMomentum;
        Pressure = projection.FinalPressure;

        // 3. Apply resource changes directly from the projection
        _resourceManager.ApplyResourceChanges(
            projection.HealthChange,
            projection.ConcentrationChange,
            projection.ConfidenceChange);

        // 4. Update active tags based on new tag values
        _tagManager.ResetTagEffects();
        _tagManager.UpdateActiveTags(Location.AvailableTags);

        // 5. Increment turn counter
        CurrentTurn++;
    }

    public void UpdateStateHistory(IChoice selectedChoice)
    {
        // Store previous choice
        PreviousChoice = selectedChoice;

        // Store previous values
        PreviousMomentum = Momentum;
        PreviousPressure = Pressure;

        // Store previous approach tag values
        PreviousApproachValues = new Dictionary<ApproachTags, int>();
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (approach == ApproachTags.Dominance ||
                approach == ApproachTags.Rapport ||
                approach == ApproachTags.Analysis ||
                approach == ApproachTags.Precision ||
                approach == ApproachTags.Concealment)
            {
                PreviousApproachValues[approach] = TagSystem.GetEncounterStateTagValue(approach);
            }
        }

        // Store previous focus tag values
        PreviousFocusValues = new Dictionary<FocusTags, int>();
        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
        {
            PreviousFocusValues[focus] = TagSystem.GetFocusTagValue(focus);
        }
    }

    public void UpdateActiveTags(IEnumerable<IEncounterTag> locationTags)
    {
        _tagManager.UpdateActiveTags(locationTags);
    }

    public void BuildMomentum(int amount) => Momentum += amount;

    public void BuildPressure(int amount) => Pressure += amount;

    public void ReducePressure(int amount) => Pressure = Math.Max(0, Pressure - amount);

    public ChoiceProjection CreateChoiceProjection(IChoice choice)
    {
        return _projectionService.CreateChoiceProjection(
            choice,
            Momentum,
            Pressure,
            CurrentTurn);
    }

    public List<string> GetActiveTagsNames()
    {
        return ActiveTags.Select(t => t.Name).ToList();
    }
}
