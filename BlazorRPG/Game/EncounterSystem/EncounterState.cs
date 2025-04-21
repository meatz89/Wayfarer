public class EncounterState
{
    public static EncounterState Last { get; set; }
    public CardDefinition PreviousChoice { get; set; }
    public int PreviousMomentum { get; set; }
    public int PreviousPressure { get; set; }
    public Dictionary<ApproachTags, int> PreviousApproachValues { get; set; } = new Dictionary<ApproachTags, int>();
    public Dictionary<FocusTags, int> PreviousFocusValues { get; set; } = new Dictionary<FocusTags, int>();

    public int Momentum { get; private set; }
    public int Pressure { get; private set; }
    public int CurrentTurn { get; private set; }
    public Encounter EncounterInfo { get; }
    public LocationSpot LocationSpot { get; set; }

    // Expose tag system through the TagManager
    public EncounterTagSystem EncounterTagSystem => _tagManager.EncounterTagSystem;
    public List<IEncounterTag> ActiveTags => _tagManager.EncounterTags;

    private readonly TagManager _tagManager;
    private readonly ResourceManager _resourceManager;
    private readonly ProjectionService _projectionService;

    public EncounterState(
        Encounter encounterInfo,
        PlayerState playerState,
        ResourceManager resourceManager)
    {
        Momentum = 5;
        Pressure = encounterInfo.EncounterDifficulty + 3;
        CurrentTurn = 0;
        EncounterInfo = encounterInfo;

        _tagManager = new TagManager();

        foreach (ApproachTags approach in Enum.GetValues<ApproachTags>())
        {
            int bonus = CalculateStartingApproachValue(approach, playerState.PlayerSkills);
            EncounterTagSystem.ModifyApproachPosition(approach, bonus);
        }

        foreach (FocusTags focus in Enum.GetValues<FocusTags>())
        {
            int bonus = CalculateStartingFocusValue(focus, playerState.PlayerSkills);
            EncounterTagSystem.ModifyFocusPosition(focus, bonus);
        }

        _resourceManager = new ResourceManager();
        _projectionService = new ProjectionService(_tagManager, _resourceManager, encounterInfo, playerState);

        Last = this;
    }

    private int CalculateStartingApproachValue(ApproachTags approachTag, PlayerSkills playerSkills)
    {
        float baseValue = 1.0f;
        float skillBonus = 0.0f;

        List<SkillApproachMapping> relevantMappings = SkillTagMappings.ApproachMappings
            .FindAll(mapping => mapping.ApproachTag == approachTag);

        foreach (SkillApproachMapping mapping in relevantMappings)
        {
            int playerSkillLevel = playerSkills.GetLevelForSkill(mapping.SkillType);
            skillBonus += playerSkillLevel * mapping.Multiplier;
        }

        return (int)baseValue + (int)skillBonus;
    }

    private int CalculateStartingFocusValue(FocusTags focusTag, PlayerSkills playerSkills)
    {
        float baseValue = 1.0f;
        float skillBonus = 0.0f;

        List<SkillFocusMapping> relevantMappings = SkillTagMappings.FocusMappings
            .FindAll(mapping => mapping.FocusTag == focusTag);

        foreach (SkillFocusMapping mapping in relevantMappings)
        {
            int playerSkillLevel = playerSkills.GetLevelForSkill(mapping.SkillType);
            skillBonus += playerSkillLevel * mapping.Multiplier;
        }

        return (int)baseValue + (int)skillBonus;
    }

    // Forward methods used by IEncounterTag.ApplyEffect and Choice.ApplyChoice
    public void AddFocusMomentumBonus(FocusTags focus, int bonus) =>
        _tagManager.AddFocusMomentumBonus(focus, bonus);

    public void AddFocusPressureModifier(FocusTags focus, int modifier) =>
        _tagManager.AddFocusPressureModifier(focus, modifier);

    public int GetTotalMomentum(CardDefinition choice, int baseMomentum) =>
        _tagManager.GetTotalMomentum(choice, baseMomentum);

    public int GetTotalPressure(CardDefinition choice, int basePressure) =>
        _tagManager.GetTotalPressure(choice, basePressure);

    public ChoiceProjection ApplyChoice(PlayerState playerState, Encounter encounterInfo, CardDefinition choice)
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
        // 1. Apply tag changes
        foreach (KeyValuePair<ApproachTags, int> pair in projection.ApproachTagChanges)
            EncounterTagSystem.ModifyApproachPosition(pair.Key, pair.Value);

        foreach (KeyValuePair<FocusTags, int> pair in projection.FocusTagChanges)
            EncounterTagSystem.ModifyFocusPosition(pair.Key, pair.Value);

        // 2. Apply exactly the values from the projection
        Momentum = projection.FinalMomentum;
        Pressure = projection.FinalPressure;

        // 3. Apply resource changes directly from the projection
        _resourceManager.ApplyResourceChanges(
            playerState,
            projection.HealthChange,
            projection.ConcentrationChange,
            projection.ConfidenceChange);

        // 4. Update active tags based on new tag values
        _tagManager.ResetTagEffects();
        _tagManager.CreateEncounterTags(EncounterInfo.AllEncounterTags);

        // 5. Increment turn counter
        CurrentTurn++;
    }

    public void UpdateStateHistory(CardDefinition selectedChoice)
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
                PreviousApproachValues[approach] = EncounterTagSystem.GetApproachTagValue(approach);
            }
        }

        // Store previous focus tag values
        PreviousFocusValues = new Dictionary<FocusTags, int>();
        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
        {
            PreviousFocusValues[focus] = EncounterTagSystem.GetFocusTagValue(focus);
        }
    }

    public void BuildMomentum(int amount) => Momentum += amount;

    public void BuildPressure(int amount) => Pressure += amount;

    public void ReducePressure(int amount) => Pressure = Math.Max(0, Pressure - amount);

    public ChoiceProjection CreateChoiceProjection(CardDefinition choice)
    {
        return _projectionService.CreateChoiceProjection(
            choice,
            Momentum,
            Pressure,
            CurrentTurn);
    }

    public List<string> GetActiveTagsNames()
    {
        return ActiveTags.Select(t => t.NarrativeName).ToList();
    }



}
