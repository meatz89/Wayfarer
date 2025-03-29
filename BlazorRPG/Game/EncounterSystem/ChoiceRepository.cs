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
        // DOMINANCE-BASED CHOICES
        // =============================================

        // Tier 1: Novice
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Basic Force",
            "You attempt to assert dominance through simple physical intimidation.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, +0 momentum
            new StrategicEffect(Population.Crowded, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Stand Firm",
            "You plant your feet and refuse to back down.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            new StrategicEffect(Atmosphere.Tense, StrategicTagEffectType.DecreasePressure, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Display of Force",
            "You demonstrate your physical power and authority, making it clear you won't back down.",
            FocusTags.Physical,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            FocusTags.Resource, 1, 0, // Requires Dominance 1+, no reduction
            new StrategicEffect(Physical.Hazardous, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Establish Boundaries",
            "You clearly establish what you will and won't tolerate.",
            FocusTags.Relationship,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            FocusTags.Resource, 1, 0, // Requires Dominance 1+, no reduction
            new StrategicEffect(Atmosphere.Tense, StrategicTagEffectType.DecreasePressure, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Overwhelming Presence",
            "Your commanding presence fills the room, forcing others to take notice and respect your authority.",
            FocusTags.Relationship,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Resource, 2, 1, // Requires Dominance 4+, reduces by 2
            new StrategicEffect(Population.Crowded, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Controlled Aggression",
            "You channel your aggressive energy precisely, defusing a tense situation through focused intimidation.",
            FocusTags.Physical,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            FocusTags.Resource, 2, 1, // Requires Dominance 4+, reduces by 2
            new StrategicEffect(Atmosphere.Tense, StrategicTagEffectType.DecreasePressure, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Territorial Command",
            "You take control of the physical space, demonstrating mastery over the environment.",
            FocusTags.Environment,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Resource, 3, 1, // Requires Environment 3+, reduces by 1
            new StrategicEffect(Physical.Hazardous, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Environment, -1) // Negative modification!
        ));

        // =============================================
        // RAPPORT-BASED CHOICES
        // =============================================

        // Tier 1: Novice
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Basic Charm",
            "You attempt to make a friendly connection with basic pleasantries.",
            FocusTags.Relationship,
            CardTiers.Novice, 0, // Tier 1, +0 momentum
            new StrategicEffect(Population.Crowded, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Simple Reassurance",
            "You offer simple comforting words to ease tension.",
            FocusTags.Relationship,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            new StrategicEffect(Atmosphere.Tense, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Charming Words",
            "You engage with warmth and genuine interest, making others feel valued.",
            FocusTags.Relationship,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            FocusTags.Relationship, 1, 0, // Requires Rapport 2+, no reduction
            new StrategicEffect(Population.Crowded, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Smooth Over",
            "You defuse tension with well-chosen words and genuine empathy.",
            FocusTags.Relationship,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            FocusTags.Relationship, 1, 0, // Requires Rapport 2+, no reduction
            new StrategicEffect(Atmosphere.Tense, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Winning Personality",
            "Your natural charisma draws others in, creating an instant connection that advances your goals.",
            FocusTags.Relationship,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Relationship, 2, 1, // Requires Rapport 4+, reduces by 2
            new StrategicEffect(Population.Crowded, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Diplomatic Solution",
            "You defuse a tense situation with perfectly chosen words that address everyone's concerns.",
            FocusTags.Information,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            FocusTags.Relationship, 2, 1, // Requires Rapport 4+, reduces by 2
            new StrategicEffect(Atmosphere.Tense, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Information Exchange",
            "Your practiced ability to extract information through friendly conversation yields valuable insights.",
            FocusTags.Information,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Relationship, 3, 1, // Requires Information 3+, reduces by 1
            new StrategicEffect(Economic.Commercial, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Information, -1) // Negative modification!
        ));

        // =============================================
        // ANALYSIS-BASED CHOICES
        // =============================================

        // Tier 1: Novice
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Basic Observation",
            "You make a simple observation about the situation.",
            FocusTags.Information,
            CardTiers.Novice, 0, // Tier 1, +0 momentum
            new StrategicEffect(Economic.Commercial, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Simple Assessment",
            "You take a moment to quickly assess the situation.",
            FocusTags.Information,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            new StrategicEffect(Atmosphere.Formal, StrategicTagEffectType.DecreasePressure, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Analytical Insight",
            "You observe subtle patterns and connections, gaining crucial insights.",
            FocusTags.Information,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            FocusTags.Information, 1, 0, // Requires Analysis 2+, no reduction
            new StrategicEffect(Economic.Commercial, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Careful Consideration",
            "You methodically rule out incorrect interpretations, preventing wasted effort.",
            FocusTags.Information,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            FocusTags.Information, 1, 0, // Requires Analysis 2+, no reduction
            new StrategicEffect(Atmosphere.Formal, StrategicTagEffectType.DecreasePressure, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Deep Analysis",
            "You perceive connections and patterns that others miss entirely, giving you a significant advantage.",
            FocusTags.Information,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Information, 2, 1, // Requires Analysis 4+, reduces by 2
            new StrategicEffect(Economic.Commercial, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Strategic Assessment",
            "Your systematic evaluation of all variables reveals the optimal approach, eliminating potential pitfalls.",
            FocusTags.Environment,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            FocusTags.Information, 2, 1, // Requires Analysis 4+, reduces by 2
            new StrategicEffect(Atmosphere.Formal, StrategicTagEffectType.DecreasePressure, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Resource Optimization",
            "Your deep understanding of efficient resource allocation creates maximum advantage.",
            FocusTags.Resource,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Information, 3, 1, // Requires Resource 3+, reduces by 1
            new StrategicEffect(Economic.Wealthy, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Resource, -1) // Negative modification!
        ));

        // =============================================
        // PRECISION-BASED CHOICES
        // =============================================

        // Tier 1: Novice
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Basic Accuracy",
            "You attempt a simple, careful movement.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, +0 momentum
            new StrategicEffect(Physical.Confined, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Simple Care",
            "You proceed with basic caution and care.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            new StrategicEffect(Physical.Hazardous, StrategicTagEffectType.DecreasePressure, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Precise Strike",
            "You execute a perfectly timed movement with flawless technique.",
            FocusTags.Physical,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            FocusTags.Physical, 1, 0, // Requires Precision 2+, no reduction
            new StrategicEffect(Physical.Confined, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Measured Response",
            "You calibrate your response perfectly to minimize risk.",
            FocusTags.Physical,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            FocusTags.Physical, 1, 0, // Requires Precision 2+, no reduction
            new StrategicEffect(Physical.Hazardous, StrategicTagEffectType.DecreasePressure, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Flawless Execution",
            "Your movements flow with perfect precision, achieving exactly the result you intended.",
            FocusTags.Physical,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Physical, 2, 1, // Requires Precision 4+, reduces by 2
            new StrategicEffect(Physical.Confined, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Perfect Calibration",
            "Your exacting adjustments to the situation eliminate multiple sources of pressure simultaneously.",
            FocusTags.Environment,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            FocusTags.Physical, 2, 1, // Requires Precision 4+, reduces by 2
            new StrategicEffect(Physical.Hazardous, StrategicTagEffectType.DecreasePressure, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Environmental Precision",
            "Your practiced ability to leverage the environment with perfect precision creates significant advantage.",
            FocusTags.Environment,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Physical, 3, 1, // Requires Environment 3+, reduces by 1
            new StrategicEffect(Physical.Confined, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Environment, -1) // Negative modification!
        ));

        // =============================================
        // Evasion-BASED CHOICES
        // =============================================

        // Tier 1: Novice
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Basic Stealth",
            "You attempt to stay unnoticed with simple stealth techniques.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, +0 momentum
            new StrategicEffect(Illumination.Shadowy, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Evasion),
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Simple Hiding",
            "You try to make yourself less noticeable.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            new StrategicEffect(Illumination.Shadowy, StrategicTagEffectType.DecreasePressure, ApproachTags.Evasion),
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Hidden Advantage",
            "You move stealthily, positioning yourself for an advantageous approach.",
            FocusTags.Physical,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            FocusTags.Environment, 1, 0, // Requires Evasion 2+, no reduction
            new StrategicEffect(Illumination.Shadowy, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Evasion),
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Fade Away",
            "You slip into the shadows, removing yourself from immediate danger.",
            FocusTags.Physical,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            FocusTags.Environment, 1, 0, // Requires Evasion 2+, no reduction
            new StrategicEffect(Illumination.Shadowy, StrategicTagEffectType.DecreasePressure, ApproachTags.Evasion),
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Perfect Stealth",
            "You move with such stealth that even those looking for you cannot detect your presence.",
            FocusTags.Physical,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Environment, 2, 1, // Requires Evasion 4+, reduces by 2
            new StrategicEffect(Illumination.Shadowy, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Evasion),
            TagModification.ForEncounterState(ApproachTags.Evasion, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Vanishing Act",
            "You disappear from perception entirely, eliminating immediate threats through complete evasion.",
            FocusTags.Environment,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            FocusTags.Environment, 2, 1, // Requires Evasion 4+, reduces by 2
            new StrategicEffect(Illumination.Shadowy, StrategicTagEffectType.DecreasePressure, ApproachTags.Evasion),
            TagModification.ForEncounterState(ApproachTags.Evasion, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Hidden Knowledge",
            "Your practiced ability to uncover concealed information gives you a significant tactical advantage.",
            FocusTags.Information,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Environment, 3, 1, // Requires Information 3+, reduces by 1
            new StrategicEffect(Economic.Commercial, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Evasion),
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForFocus(FocusTags.Information, -1) // Negative modification!
        ));
    }

    public List<IChoice> GetAllChoices()
    {
        return new List<IChoice>(_choices);
    }

    public List<IChoice> GetAvailableChoices(EncounterState state)
    {
        return _choices.Where(c => c.Requirement.IsMet(state.TagSystem))
            .Select(choice => (IChoice)choice)
            .ToList();
    }
}