using System;
using System.Collections.Generic;
using System.Linq;

public class EncounterState
{
    public int Momentum { get; private set; }
    public int Pressure { get; private set; }
    public int CurrentTurn { get; private set; }
    public LocationInfo Location { get; }
    public LocationSpot LocationSpot { get; internal set; }
    public PlayerState PlayerState { get; }

    // Expose tag system through the TagManager
    public BaseTagSystem TagSystem => _tagManager.TagSystem;
    public List<IEncounterTag> ActiveTags => _tagManager.ActiveTags;

    public const int MaxPressure = 30;

    private readonly TagManager _tagManager;
    private readonly ResourceManager _resourceManager;
    private readonly ProjectionService _projectionService;
    private int _escalationLevel = 0;
    private IChoice _lastChoice;

    public EncounterState(LocationInfo location, PlayerState playerState)
    {
        Momentum = 0;
        Pressure = 0;
        CurrentTurn = 0;
        Location = location;
        PlayerState = playerState;

        // Initialize managers
        _tagManager = new TagManager();
        _resourceManager = new ResourceManager(playerState, _tagManager, location);
        _projectionService = new ProjectionService(_tagManager, _resourceManager, location);
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

    public void ApplyChoiceProjection(ChoiceProjection projection)
    {
        _lastChoice = projection.Choice;

        // Apply resource changes from pressure at start of turn (based on current pressure)
        _resourceManager.ApplyPressureResourceDamage(Pressure);

        // 1. Apply tag changes
        foreach (KeyValuePair<ApproachTags, int> pair in projection.EncounterStateTagChanges)
            TagSystem.ModifyEncounterStateTag(pair.Key, pair.Value);

        foreach (KeyValuePair<ApproachTags, int> pair in projection.ApproachTagChanges)
            TagSystem.ModifyApproachTag(pair.Key, pair.Value);

        foreach (KeyValuePair<FocusTags, int> pair in projection.FocusTagChanges)
            TagSystem.ModifyFocusTag(pair.Key, pair.Value);

        // 2. Update active tags based on new tag values
        _tagManager.ResetTagEffects();
        _tagManager.UpdateActiveTags(Location.AvailableTags);

        // 3. Apply exactly the values from the projection
        Momentum = projection.FinalMomentum;
        Pressure = projection.FinalPressure;

        // 4. Apply resource changes directly from the projection
        _resourceManager.ApplyResourceChanges(
            projection.HealthChange,
            projection.FocusChange,
            projection.ConfidenceChange);

        // 5. Increment turn counter
        CurrentTurn++;
        _escalationLevel = Math.Min(3, (CurrentTurn - 1) / 2);
    }

    public void UpdateActiveTags(IEnumerable<IEncounterTag> locationTags)
    {
        _tagManager.UpdateActiveTags(locationTags);
    }

    public void BuildMomentum(int amount) => Momentum += amount;

    public void BuildPressure(int amount) => Pressure += amount;

    public void ReducePressure(int amount) => Pressure = Math.Max(0, Pressure - amount);

    public void EndTurn()
    {
        CurrentTurn++;
        _escalationLevel = Math.Min(3, (CurrentTurn - 1) / 2);

        int endOfTurnPressureReduction = _tagManager.GetEndOfTurnPressureReduction();
        if (endOfTurnPressureReduction > 0)
            ReducePressure(endOfTurnPressureReduction);
    }

    public bool IsEncounterOver() => Pressure >= MaxPressure || CurrentTurn >= Location.TurnDuration;

    public EncounterOutcomes GetOutcome()
    {
        if (Pressure >= MaxPressure || Momentum < Location.PartialThreshold)
            return EncounterOutcomes.Failure;
        if (Momentum < Location.StandardThreshold)
            return EncounterOutcomes.Partial;
        if (Momentum < Location.ExceptionalThreshold)
            return EncounterOutcomes.Standard;

        return EncounterOutcomes.Exceptional;
    }

    public ChoiceProjection CreateChoiceProjection(IChoice choice)
    {
        return _projectionService.CreateChoiceProjection(
            choice,
            Momentum,
            Pressure,
            CurrentTurn,
            _escalationLevel);
    }
}
