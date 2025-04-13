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
        // TIER 1: NOVICE CARDS
        // =============================================

        // Analysis approach, Information focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Basic Observation",
            "You quickly scan the situation for relevant details.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Analysis, FocusTags.Information, 1, // Optimal position: Information 1
            StrategicEffectsContent.Insightful,
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Analysis approach, Information focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Careful Assessment",
            "You take a moment to consider the situation methodically.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Analysis, FocusTags.Information, 1, // Optimal position: Information 1
            new StrategicEffect(new() { Atmosphere.Any }, StrategicTagEffectType.DecreasePressure, ApproachTags.Analysis),
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Precision approach, Physical focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Deliberate Action",
            "You move with careful precision, maximizing efficiency.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Precision, FocusTags.Physical, 1, // Optimal position: Physical 1
            new StrategicEffect(new() { Physical.Any }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        // Precision approach, Physical focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Measured Technique",
            "You execute your actions with careful control to minimize risk.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Precision, FocusTags.Physical, 1, // Optimal position: Physical 1
            StrategicEffectsContent.Calculated,
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        // Rapport approach, Relationship focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Friendly Greeting",
            "You establish a positive connection with warm approachability.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Rapport, FocusTags.Relationship, 1, // Optimal position: Relationship 1
            new StrategicEffect(new() { Population.Any }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));

        // Rapport approach, Relationship focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Soothing Words",
            "You speak calmly, easing tension in the situation.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Rapport, FocusTags.Relationship, 1, // Optimal position: Relationship 1
            new StrategicEffect(new() { Atmosphere.Any }, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));

        // Dominance approach, Physical focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Show of Strength",
            "You demonstrate your physical power and confidence.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Dominance, FocusTags.Physical, 1, // Optimal position: Physical 1
            new StrategicEffect(new() { Population.Any }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        // Dominance approach, Relationship focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Establish Presence",
            "You take command of the situation, creating a sense of order.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Dominance, FocusTags.Relationship, 1, // Optimal position: Relationship 1
            new StrategicEffect(new() { Atmosphere.Any }, StrategicTagEffectType.DecreasePressure, ApproachTags.Dominance),
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));

        // Concealment approach, Environment focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Use Surroundings",
            "You leverage the environment to remain unnoticed.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Concealment, FocusTags.Environment, 1, // Optimal position: Environment 1
            new StrategicEffect(new() { Illumination.Any }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Concealment),
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // Concealment approach, Physical focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Keep Low Profile",
            "You minimize your presence to avoid drawing attention.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Concealment, FocusTags.Physical, 1, // Optimal position: Physical 1
            new StrategicEffect(new() { Illumination.Any }, StrategicTagEffectType.DecreasePressure, ApproachTags.Concealment),
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        // =============================================
        // TIER 2: TRAINED CARDS
        // =============================================

        // Analysis approach, Environment focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Environmental Assessment",
            "You analyze the surroundings to identify advantageous elements.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Analysis, FocusTags.Environment, 2, // Optimal position: Environment 2
            new StrategicEffect(new() { Physical.Any }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // Analysis approach, Resource focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Efficient Planning",
            "You identify the optimal use of available resources.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Analysis, FocusTags.Resource, 2, // Optimal position: Resource 2
            new StrategicEffect(new() { Atmosphere.Any }, StrategicTagEffectType.DecreasePressure, ApproachTags.Analysis),
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Resource)
        ));

        // Rapport approach, Information focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Friendly Inquiry",
            "You ask questions in a way that encourages open sharing.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Rapport, FocusTags.Information, 2, // Optimal position: Information 2
            new StrategicEffect(new() { Population.Any }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Rapport approach, Environment focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Create Safe Space",
            "You cultivate an atmosphere where everyone feels secure.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Rapport, FocusTags.Environment, 2, // Optimal position: Environment 2
            new StrategicEffect(new() { Atmosphere.Any }, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // Precision approach, Information focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Targeted Question",
            "You ask the exact question needed to get essential information.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Precision, FocusTags.Information, 2, // Optimal position: Information 2
            new StrategicEffect(new() { Atmosphere.Any }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Precision approach, Resource focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Resource Optimization",
            "You use exactly what's needed with no waste or excess.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Precision, FocusTags.Resource, 2, // Optimal position: Resource 2
            StrategicEffectsContent.Calculated,
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Resource)
        ));

        // Dominance approach, Environment focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Command the Space",
            "You take control of the physical environment.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Dominance, FocusTags.Environment, 2, // Optimal position: Environment 2
            new StrategicEffect(new() { Physical.Any }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // Dominance approach, Resource focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Resource Control",
            "You take charge of resource allocation, creating order.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Dominance, FocusTags.Resource, 2, // Optimal position: Resource 2
            new StrategicEffect(new() { Atmosphere.Any }, StrategicTagEffectType.DecreasePressure, ApproachTags.Dominance),
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Resource)
        ));

        // Concealment approach, Information focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Gather Intelligence",
            "You collect information while remaining unnoticed.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Concealment, FocusTags.Information, 2, // Optimal position: Information 2
            new StrategicEffect(new() { Population.Any }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Concealment),
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Concealment approach, Relationship focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Fade from Attention",
            "You divert attention away from yourself, reducing social pressure.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Concealment, FocusTags.Relationship, 2, // Optimal position: Relationship 2
            new StrategicEffect(new() { Population.Any }, StrategicTagEffectType.DecreasePressure, ApproachTags.Concealment),
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));
    }
    
    public List<ChoiceCard> GetAvailableChoices(EncounterState state)
    {
        return _choices
            .Select(choice => choice)
            .ToList();
    }
}