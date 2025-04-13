/// <summary>
/// Repository of all available choices in the game
/// </summary>
public class ChoiceRepository
{
    private readonly List<ChoiceCard> _choices = new();

    public ChoiceRepository()
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
            ApproachTags.Analysis, 1, FocusTags.Information, 1, // Optimal position: Analysis 1, Information 1
            new StrategicEffect(Population.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // Analysis approach, Information focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Careful Assessment",
            "You take a moment to consider the situation methodically.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Analysis, 1, FocusTags.Information, 1, // Optimal position: Analysis 1, Information 1
            new StrategicEffect(Atmosphere.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // Precision approach, Physical focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Deliberate Action",
            "You move with careful precision, maximizing efficiency.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Precision, 1, FocusTags.Physical, 1, // Optimal position: Precision 1, Physical 1
            new StrategicEffect(Physical.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Precision approach, Physical focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Measured Technique",
            "You execute your actions with careful control to minimize risk.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Precision, 1, FocusTags.Physical, 1, // Optimal position: Precision 1, Physical 1
            new StrategicEffect(Physical.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Rapport approach, Relationship focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Friendly Greeting",
            "You establish a positive connection with warm approachability.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Rapport, 1, FocusTags.Relationship, 1, // Optimal position: Rapport 1, Relationship 1
            new StrategicEffect(Population.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Rapport approach, Relationship focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Soothing Words",
            "You speak calmly, easing tension in the situation.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Rapport, 1, FocusTags.Relationship, 1, // Optimal position: Rapport 1, Relationship 1
            new StrategicEffect(Atmosphere.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Dominance approach, Physical focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Show of Strength",
            "You demonstrate your physical power and confidence.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Dominance, 1, FocusTags.Physical, 1, // Optimal position: Dominance 1, Physical 1
            new StrategicEffect(Population.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Dominance approach, Relationship focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Establish Presence",
            "You take command of the situation, creating a sense of order.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Dominance, 1, FocusTags.Relationship, 1, // Optimal position: Dominance 1, Relationship 1
            new StrategicEffect(Atmosphere.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Concealment approach, Environment focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Use Surroundings",
            "You leverage the environment to remain unnoticed.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Concealment, 1, FocusTags.Environment, 1, // Optimal position: Concealment 1, Environment 1
            new StrategicEffect(Illumination.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Concealment),
            TagModification.ForEncounterState(ApproachTags.Concealment, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        // Concealment approach, Physical focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Keep Low Profile",
            "You minimize your presence to avoid drawing attention.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Concealment, 1, FocusTags.Physical, 1, // Optimal position: Concealment 1, Physical 1
            new StrategicEffect(Illumination.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Concealment),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // =============================================
        // TIER 2: TRAINED CARDS
        // =============================================

        // Analysis approach, Environment focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Environmental Assessment",
            "You analyze the surroundings to identify advantageous elements.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Analysis, 3, FocusTags.Environment, 2, // Optimal position: Analysis 3, Environment 2
            new StrategicEffect(Physical.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Environment, 2)
        ));

        // Analysis approach, Resource focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Efficient Planning",
            "You identify the optimal use of available resources.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Analysis, 3, FocusTags.Resource, 2, // Optimal position: Analysis 3, Resource 2
            new StrategicEffect(Economic.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Resource, 2)
        ));

        // Rapport approach, Information focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Friendly Inquiry",
            "You ask questions in a way that encourages open sharing.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Rapport, 3, FocusTags.Information, 2, // Optimal position: Rapport 3, Information 2
            new StrategicEffect(Population.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Information, 2)
        ));

        // Rapport approach, Environment focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Create Safe Space",
            "You cultivate an atmosphere where everyone feels secure.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Rapport, 3, FocusTags.Environment, 2, // Optimal position: Rapport 3, Environment 2
            new StrategicEffect(Atmosphere.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Environment, 2)
        ));

        // Precision approach, Information focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Targeted Question",
            "You ask the exact question needed to get essential information.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Precision, 3, FocusTags.Information, 2, // Optimal position: Precision 3, Information 2
            new StrategicEffect(Atmosphere.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Information, 2)
        ));

        // Precision approach, Resource focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Resource Optimization",
            "You use exactly what's needed with no waste or excess.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Precision, 3, FocusTags.Resource, 2, // Optimal position: Precision 3, Resource 2
            new StrategicEffect(Economic.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Resource, 2)
        ));

        // Dominance approach, Environment focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Command the Space",
            "You take control of the physical environment.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Dominance, 3, FocusTags.Environment, 2, // Optimal position: Dominance 3, Environment 2
            new StrategicEffect(Physical.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Environment, 2)
        ));

        // Dominance approach, Resource focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Resource Control",
            "You take charge of resource allocation, creating order.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Dominance, 3, FocusTags.Resource, 2, // Optimal position: Dominance 3, Resource 2
            new StrategicEffect(Economic.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Resource, 2)
        ));

        // Concealment approach, Information focus - momentum builder
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Gather Intelligence",
            "You collect information while remaining unnoticed.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Concealment, 3, FocusTags.Information, 2, // Optimal position: Concealment 3, Information 2
            new StrategicEffect(Population.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Concealment),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1),
            TagModification.ForFocus(FocusTags.Information, 2)
        ));

        // Concealment approach, Relationship focus - pressure reducer
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Fade from Attention",
            "You divert attention away from yourself, reducing social pressure.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Concealment, 3, FocusTags.Relationship, 2, // Optimal position: Concealment 3, Relationship 2
            new StrategicEffect(Population.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Concealment),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1),
            TagModification.ForFocus(FocusTags.Relationship, 2)
        ));
    }

    public List<ChoiceCard> GetAllChoices()
    {
        return new List<ChoiceCard>(_choices);
    }

    public List<ChoiceCard> GetAvailableChoices(EncounterState state)
    {
        return _choices
            .Select(choice => choice)
            .ToList();
    }
}