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

        // Analysis approach, Resource optimal - momentum builder; grants Information
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Keen Observation",
            "Your eyes capture minute details in the clarity of a bright setting.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Analysis, FocusTags.Resource, 1, // Optimal position: Resource 1 (requires Resource, grants Information)
            new StrategicEffect(new() { Illumination.Bright }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Analysis approach, Resource optimal - pressure reducer; grants Information
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Methodical Evaluation",
            "You analyze every aspect methodically, relying on a formal atmosphere to reduce pressure.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Analysis, FocusTags.Resource, 1, // Optimal position: Resource 1 (requires Resource, grants Information)
            new StrategicEffect(new() { Atmosphere.Formal }, StrategicTagEffectType.DecreasePressure, ApproachTags.Analysis),
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Precision approach, Environment optimal - momentum builder; grants Physical
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Precise Maneuver",
            "With measured steps and acute awareness of spatial confines, you act with flawless precision.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Precision, FocusTags.Environment, 1, // Optimal position: Environment 1 (requires Environment, grants Physical)
            new StrategicEffect(new() { Physical.Confined }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        // Precision approach, Environment optimal - pressure reducer; grants Physical
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Controlled Execution",
            "Every action is carefully contained and executed in a confined space, ensuring minimal risk.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Precision, FocusTags.Environment, 1, // Optimal position: Environment 1 (requires Environment, grants Physical)
            new StrategicEffect(new() { Physical.Confined }, StrategicTagEffectType.DecreasePressure, ApproachTags.Precision),
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        // Rapport approach, Physical optimal - momentum builder; grants Relationship
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Warm Welcome",
            "A genuine smile and open posture forge friendly bonds amid a crowded gathering.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Rapport, FocusTags.Physical, 1, // Optimal position: Physical 1 (requires Physical, grants Relationship)
            new StrategicEffect(new() { Population.Crowded }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));

        // Rapport approach, Physical optimal - pressure reducer; grants Relationship
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Calming Conversation",
            "Your soothing words ease tensions, inviting tranquility in a serene, quiet space.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Rapport, FocusTags.Physical, 1, // Optimal position: Physical 1 (requires Physical, grants Relationship)
            new StrategicEffect(new() { Population.Quiet }, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));

        // Dominance approach, Relationship optimal - momentum builder; grants Physical
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Commanding Display",
            "A powerful stance and dynamic presence disrupt the norm, setting a chaotic tone.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Dominance, FocusTags.Relationship, 1, // Optimal position: Relationship 1 (requires Relationship, grants Physical)
            new StrategicEffect(new() { Atmosphere.Chaotic }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        // Dominance approach, Physical optimal - pressure reducer; grants Relationship
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Steady Authority",
            "With unwavering calm, you restore order and alleviate tension amid expansive surroundings.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Dominance, FocusTags.Physical, 1, // Optimal position: Physical 1 (requires Physical, grants Relationship)
            new StrategicEffect(new() { Physical.Expansive }, StrategicTagEffectType.DecreasePressure, ApproachTags.Dominance),
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Relationship)
        ));

        // Concealment approach, Information optimal - momentum builder; grants Environment
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Blend with the Crowd",
            "You seamlessly merge into a shadowy backdrop, harnessing low visibility to gather subtle cues.",
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Concealment, FocusTags.Information, 1, // Optimal position: Information 1 (requires Information, grants Environment)
            new StrategicEffect(new() { Illumination.Shadowy }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Concealment),
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // Concealment approach, Information optimal - pressure reducer; grants Physical
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Silent Withdrawal",
            "You discreetly slip away into a quiet corner, easing pressure as you vanish from sight.",
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Concealment, FocusTags.Information, 1, // Optimal position: Information 1 (requires Information, grants Physical)
            new StrategicEffect(new() { Population.Quiet }, StrategicTagEffectType.DecreasePressure, ApproachTags.Concealment),
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Physical)
        ));

        // =============================================
        // TIER 2: TRAINED CARDS
        // =============================================

        // Analysis approach, Resource optimal - momentum builder; grants Environment
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Tactical Survey",
            "You scrutinize every inch of the terrain, reading environmental signals with a clear, bright focus.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Analysis, FocusTags.Resource, 2, // Optimal position: Resource 2 (requires Resource, grants Environment)
            new StrategicEffect(new() { Illumination.Bright }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // Analysis approach, Information optimal - pressure reducer; grants Resource
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Resourceful Strategy",
            "You devise a plan that leverages every asset, drawing on a formal atmosphere to optimize outcomes.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Analysis, FocusTags.Information, 2, // Optimal position: Information 2 (requires Information, grants Resource)
            new StrategicEffect(new() { Atmosphere.Formal }, StrategicTagEffectType.DecreasePressure, ApproachTags.Analysis),
            TagModification.IncreaseApproach(ApproachTags.Analysis),
            TagModification.IncreaseFocus(FocusTags.Resource)
        ));

        // Rapport approach, Physical optimal - momentum builder; grants Information
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Engaging Question",
            "Your sincere inquiry sparks insightful dialogue in a setting enriched by scholarly exchange.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Rapport, FocusTags.Physical, 2, // Optimal position: Physical 2 (requires Physical, grants Information)
            new StrategicEffect(new() { Population.Scholarly }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Rapport approach, Physical optimal - pressure reducer; grants Environment
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Haven of Trust",
            "You create a sanctuary of calm, drawing on the quiet nature of your surroundings to reduce tension.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Rapport, FocusTags.Physical, 2, // Optimal position: Physical 2 (requires Physical, grants Environment)
            new StrategicEffect(new() { Population.Quiet }, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.IncreaseApproach(ApproachTags.Rapport),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // Precision approach, Environment optimal - momentum builder; grants Information
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Pinpoint Query",
            "Your precise question illuminates hidden details, as clarity shines in a bright setting.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Precision, FocusTags.Environment, 2, // Optimal position: Environment 2 (requires Environment, grants Information)
            new StrategicEffect(new() { Illumination.Bright }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Precision approach, Information optimal - pressure reducer; grants Resource
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Optimal Allocation",
            "You streamline every resource with calculated care, channeling confined precision to eliminate waste.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Precision, FocusTags.Information, 2, // Optimal position: Information 2 (requires Information, grants Resource)
            new StrategicEffect(new() { Physical.Confined }, StrategicTagEffectType.DecreasePressure, ApproachTags.Precision),
            TagModification.IncreaseApproach(ApproachTags.Precision),
            TagModification.IncreaseFocus(FocusTags.Resource)
        ));

        // Dominance approach, Resource optimal - momentum builder; grants Environment
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Spatial Domination",
            "You seize control of the area, bending expansive surroundings to your will.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Dominance, FocusTags.Resource, 2, // Optimal position: Resource 2 (requires Resource, grants Environment)
            new StrategicEffect(new() { Physical.Expansive }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Environment)
        ));

        // Dominance approach, Environment optimal - pressure reducer; grants Resource
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Calculated Command",
            "With meticulous control, you neutralize chaos and direct resources with calculated precision.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Dominance, FocusTags.Environment, 2, // Optimal position: Environment 2 (requires Environment, grants Resource)
            new StrategicEffect(new() { Atmosphere.Chaotic }, StrategicTagEffectType.DecreasePressure, ApproachTags.Dominance),
            TagModification.IncreaseApproach(ApproachTags.Dominance),
            TagModification.IncreaseFocus(FocusTags.Resource)
        ));

        // Concealment approach, Relationship optimal - momentum builder; grants Information
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Stealth Reconnaissance",
            "In the midst of social interplay, you quietly gather crucial insights with a discreet touch.",
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Concealment, FocusTags.Relationship, 2, // Optimal position: Relationship 2 (requires Relationship, grants Information)
            new StrategicEffect(new() { Population.Quiet }, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Concealment),
            TagModification.IncreaseApproach(ApproachTags.Concealment),
            TagModification.IncreaseFocus(FocusTags.Information)
        ));

        // Concealment approach, Information optimal - pressure reducer; grants Relationship
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Subtle Dissipation",
            "Gradually, you fade into the background, dispersing tension amidst a rough, edgy setting.",
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Concealment, FocusTags.Information, 2, // Optimal position: Information 2 (requires Information, grants Relationship)
            new StrategicEffect(new() { Atmosphere.Rough }, StrategicTagEffectType.DecreasePressure, ApproachTags.Concealment),
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
