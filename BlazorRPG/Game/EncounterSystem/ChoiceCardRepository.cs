/// <summary>
/// Repository of all available choices in the game
/// </summary>
public class ChoiceCardRepository
{
    private readonly List<ChoiceCard> _choices = new();

    public ChoiceCardRepository()
    {
        InitializeChoices();
    }

    private void InitializeChoices()
    {
        // =============================================
        // TIER 1: NOVICE CARDS — Build Approach Only
        // =============================================

        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Perceptive Glance",
            "You catch a subtle clue in the light, sharpening your analytical senses.",
            CardTiers.Novice, 3,
            ApproachTags.Analysis, FocusTags.Resource, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Organized Thoughts",
            "A formal setting brings clarity and lowers tension as your mind begins to map the situation.",
            CardTiers.Novice, 1,
            ApproachTags.Analysis, FocusTags.Resource, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Careful Steps",
            "You move with deliberate intent, adapting to the confined space around you.",
            CardTiers.Novice, 3,
            ApproachTags.Precision, FocusTags.Environment, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Measured Breath",
            "You focus on the limits of the environment and calm your nerves through physical control.",
            CardTiers.Novice, 1,
            ApproachTags.Precision, FocusTags.Environment, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Inviting Gesture",
            "A warm physical presence opens the door to connection.",
            CardTiers.Novice, 3,
            ApproachTags.Rapport, FocusTags.Physical, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Soft Tones",
            "In a calm, quiet setting, your voice soothes away the pressure.",
            CardTiers.Novice, 1,
            ApproachTags.Rapport, FocusTags.Physical, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Assertive Posture",
            "Your presence alone sends ripples through the environment.",
            CardTiers.Novice, 3,
            ApproachTags.Dominance, FocusTags.Relationship, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Unshaken Presence",
            "You anchor yourself in a broad space, easing tension through unflinching calm.",
            CardTiers.Novice, 1,
            ApproachTags.Dominance, FocusTags.Physical, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Fade Inward",
            "You meld into the shadows, your environment becoming your shield.",
            CardTiers.Novice, 3,
            ApproachTags.Concealment, FocusTags.Information, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Slip Away",
            "Quietly distancing yourself, you reduce pressure by vanishing from view.",
            CardTiers.Novice, 1,
            ApproachTags.Concealment, FocusTags.Information, 1,
            null,
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        // =============================================
        // TIER 2: TRAINED CARDS — Can Use & Build Approach
        // =============================================

        // "Tactical Survey" will USE approach (strategic effect only)
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Tactical Survey",
            "You scrutinize every inch of the terrain, leveraging your acute observation to drive momentum forward.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Analysis, FocusTags.Resource, 3,
            new StrategicEffect(new() { Illumination.Bright }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.None(), // Do not build approach
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // "Resourceful Strategy" will BUILD approach (build only)
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Resourceful Strategy",
            "You methodically marshal your assets, reinforcing your analytical resolve to reduce pressure.",
            CardTiers.Trained, 3, // Base effect: -2 pressure
            ApproachTags.Analysis, FocusTags.Information, 3,
            null, // No strategic effect; instead, build approach
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Resource)
        ));

        // "Engaging Question" will USE approach (strategic effect only)
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Engaging Question",
            "Your incisive inquiry sparks a burst of insight, propelling progress through verbal finesse.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Rapport, FocusTags.Physical, 3,
            new StrategicEffect(new() { Population.Scholarly }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.None(), // Do not build approach
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // "Haven of Trust" will BUILD approach (build only)
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Haven of Trust",
            "You create a safe haven through steady presence, reinforcing your rapport to ease tension.",
            CardTiers.Trained, 3, // Base effect: -2 pressure
            ApproachTags.Rapport, FocusTags.Physical, 3,
            null, // No strategic effect; instead, build approach
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // "Pinpoint Query" will USE approach (strategic effect only)
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Pinpoint Query",
            "Your focused question cuts through confusion, channeling clarity and driving momentum.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Precision, FocusTags.Environment, 3,
            new StrategicEffect(new() { Illumination.Bright }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.None(), // Do not build approach
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // "Optimal Allocation" will BUILD approach (build only)
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Optimal Allocation",
            "You efficiently reassign resources with precision, bolstering your methodical foundation to lower pressure.",
            CardTiers.Trained, 3, // Base effect: -2 pressure
            ApproachTags.Precision, FocusTags.Information, 3,
            null, // No strategic effect; instead, build approach
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Resource)
        ));

        // "Spatial Domination" will USE approach (strategic effect only)
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Spatial Domination",
            "You assert your control over the surroundings, using dominating force to surge momentum.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Dominance, FocusTags.Resource, 3,
            new StrategicEffect(new() { Physical.Expansive }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.None(), // Do not build approach
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // "Calculated Command" will BUILD approach (build only)
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Calculated Command",
            "You calmly orchestrate your resources, reinforcing your dominant presence to quell chaos.",
            CardTiers.Trained, 3, // Base effect: -2 pressure
            ApproachTags.Dominance, FocusTags.Environment, 3,
            null, // No strategic effect; instead, build approach
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Resource)
        ));

        // "Stealth Reconnaissance" will USE approach (strategic effect only)
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Stealth Reconnaissance",
            "In the midst of the fray, you gather critical insights silently, turning subtlety into momentum.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Concealment, FocusTags.Relationship, 3,
            new StrategicEffect(new() { Population.Quiet }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Concealment),
            TagModification.None(), // Do not build approach
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // "Subtle Dissipation" will BUILD approach (build only)
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Subtle Dissipation",
            "You gradually fade from view, reinforcing your covert stance to disperse mounting tension.",
            CardTiers.Trained, 3, // Base effect: -2 pressure
            ApproachTags.Concealment, FocusTags.Information, 3,
            null, // No strategic effect; instead, build approach
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));
    }

    public List<ChoiceCard> GetAvailableChoices(EncounterState state)
    {
        return _choices.ToList();
    }
}
