/// <summary>
/// Repository of all available choices in the game
/// </summary>
public class ChoiceRepository
{
    private readonly List<Choice> _choices = new();

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
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Stand Firm",
            "You plant your feet and refuse to back down.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Display of Force",
            "You demonstrate your physical power and authority, making it clear you won't back down.",
            FocusTags.Physical,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            ApproachTags.Dominance, 1, 0, // Requires Dominance 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Establish Boundaries",
            "You clearly establish what you will and won't tolerate.",
            FocusTags.Relationship,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            ApproachTags.Dominance, 1, 0, // Requires Dominance 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Overwhelming Presence",
            "Your commanding presence fills the room, forcing others to take notice and respect your authority.",
            FocusTags.Relationship,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            ApproachTags.Dominance, 3, 1, // Requires Dominance 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Dominance, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Controlled Aggression",
            "You channel your aggressive energy precisely, defusing a tense situation through focused intimidation.",
            FocusTags.Physical,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            ApproachTags.Dominance, 3, 1, // Requires Dominance 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Dominance, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Territorial Command",
            "You take control of the physical space, demonstrating mastery over the environment.",
            FocusTags.Environment,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Environment, 2, 1, // Requires Environment 2+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Environment, -1) // Negative modification!
        ));

        // Tier 4: Expert
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Masterful Intimidation",
            "Your mere presence silences the room. Your body language and expression demonstrate irresistible authority.",
            FocusTags.Relationship,
            CardTiers.Expert, 4, // Tier 4, +4 momentum
            ApproachTags.Dominance, 5, 2, // Requires Dominance 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Dominance, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Strategic Authority",
            "You apply precise pressure at key moments, completely defusing a tense situation through perfectly timed dominance.",
            FocusTags.Information,
            CardTiers.Expert, 3, // Tier 4, -3 pressure
            ApproachTags.Dominance, 5, 2, // Requires Dominance 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Dominance, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // Tier 5: Master
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Overwhelming Command",
            "You channel absolute authority, bending the situation entirely to your will through perfect dominance.",
            FocusTags.Physical,
            CardTiers.Master, 5, // Tier 5, +5 momentum
            ApproachTags.Dominance, 8, 3, // Requires Dominance 8+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Dominance, -3), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithFocusRequirement(
            "Complete Control",
            "Your mastery of resource management under pressure creates total stability in a chaotic situation.",
            FocusTags.Resource,
            CardTiers.Master, 4, // Tier 5, -4 pressure
            FocusTags.Resource, 5, 3, // Requires Resource 5+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Resource, -3) // Negative modification!
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
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Simple Reassurance",
            "You offer simple comforting words to ease tension.",
            FocusTags.Relationship,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Charming Words",
            "You engage with warmth and genuine interest, making others feel valued.",
            FocusTags.Relationship,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            ApproachTags.Rapport, 1, 0, // Requires Rapport 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Smooth Over",
            "You defuse tension with well-chosen words and genuine empathy.",
            FocusTags.Relationship,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            ApproachTags.Rapport, 1, 0, // Requires Rapport 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Winning Personality",
            "Your natural charisma draws others in, creating an instant connection that advances your goals.",
            FocusTags.Relationship,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            ApproachTags.Rapport, 3, 1, // Requires Rapport 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Rapport, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Diplomatic Solution",
            "You defuse a tense situation with perfectly chosen words that address everyone's concerns.",
            FocusTags.Information,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            ApproachTags.Rapport, 3, 1, // Requires Rapport 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Rapport, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Information Exchange",
            "Your practiced ability to extract information through friendly conversation yields valuable insights.",
            FocusTags.Information,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Information, 2, 1, // Requires Information 2+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Information, -1) // Negative modification!
        ));

        // Tier 4: Expert
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Master Negotiator",
            "Your social brilliance allows you to create perfect win-win scenarios that advance your goals significantly.",
            FocusTags.Resource,
            CardTiers.Expert, 4, // Tier 4, +4 momentum
            ApproachTags.Rapport, 5, 2, // Requires Rapport 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Rapport, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Social Symphony",
            "You orchestrate social dynamics with such finesse that all tension dissolves naturally.",
            FocusTags.Relationship,
            CardTiers.Expert, 3, // Tier 4, -3 pressure
            ApproachTags.Rapport, 5, 2, // Requires Rapport 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Rapport, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Tier 5: Master
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Legendary Charisma",
            "Your presence is magnetic, your words profound. Even strangers would follow you anywhere.",
            FocusTags.Relationship,
            CardTiers.Master, 5, // Tier 5, +5 momentum
            ApproachTags.Rapport, 8, 3, // Requires Rapport 8+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Rapport, -3), // Negative modification!
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithFocusRequirement(
            "Perfect Harmony",
            "Your masterful understanding of relationships creates a uniquely relaxed atmosphere, dissolving all pressure.",
            FocusTags.Relationship,
            CardTiers.Master, 4, // Tier 5, -4 pressure
            FocusTags.Relationship, 5, 3, // Requires Relationship 5+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Relationship, -3) // Negative modification!
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
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Simple Assessment",
            "You take a moment to quickly assess the situation.",
            FocusTags.Information,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Analytical Insight",
            "You observe subtle patterns and connections, gaining crucial insights.",
            FocusTags.Information,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            ApproachTags.Analysis, 1, 0, // Requires Analysis 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Careful Consideration",
            "You methodically rule out incorrect interpretations, preventing wasted effort.",
            FocusTags.Information,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            ApproachTags.Analysis, 1, 0, // Requires Analysis 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Deep Analysis",
            "You perceive connections and patterns that others miss entirely, giving you a significant advantage.",
            FocusTags.Information,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            ApproachTags.Analysis, 3, 1, // Requires Analysis 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Analysis, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Strategic Assessment",
            "Your systematic evaluation of all variables reveals the optimal approach, eliminating potential pitfalls.",
            FocusTags.Environment,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            ApproachTags.Analysis, 3, 1, // Requires Analysis 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Analysis, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Resource Optimization",
            "Your deep understanding of efficient resource allocation creates maximum advantage.",
            FocusTags.Resource,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Resource, 2, 1, // Requires Resource 2+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Resource, -1) // Negative modification!
        ));

        // Tier 4: Expert
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Perfect Deduction",
            "Your analytical brilliance connects seemingly unrelated details into a flawless understanding.",
            FocusTags.Information,
            CardTiers.Expert, 4, // Tier 4, +4 momentum
            ApproachTags.Analysis, 5, 2, // Requires Analysis 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Analysis, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Comprehensive Solution",
            "Your methodical approach addresses all possible risks simultaneously, defusing the situation completely.",
            FocusTags.Physical,
            CardTiers.Expert, 3, // Tier 4, -3 pressure
            ApproachTags.Analysis, 5, 2, // Requires Analysis 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Analysis, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 5: Master
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Unparalleled Insight",
            "Your genius-level analysis provides a complete understanding that transforms the entire situation.",
            FocusTags.Information,
            CardTiers.Master, 5, // Tier 5, +5 momentum
            ApproachTags.Analysis, 8, 3, // Requires Analysis 8+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Analysis, -3), // Negative modification!
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithFocusRequirement(
            "Information Mastery",
            "Your profound understanding of information creates absolute certainty and eliminates all confusion.",
            FocusTags.Information,
            CardTiers.Master, 4, // Tier 5, -4 pressure
            FocusTags.Information, 5, 3, // Requires Information 5+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Information, -3) // Negative modification!
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
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Simple Care",
            "You proceed with basic caution and care.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Precise Strike",
            "You execute a perfectly timed movement with flawless technique.",
            FocusTags.Physical,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            ApproachTags.Precision, 1, 0, // Requires Precision 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Measured Response",
            "You calibrate your response perfectly to minimize risk.",
            FocusTags.Physical,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            ApproachTags.Precision, 1, 0, // Requires Precision 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Flawless Execution",
            "Your movements flow with perfect precision, achieving exactly the result you intended.",
            FocusTags.Physical,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            ApproachTags.Precision, 3, 1, // Requires Precision 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Precision, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Perfect Calibration",
            "Your exacting adjustments to the situation eliminate multiple sources of pressure simultaneously.",
            FocusTags.Environment,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            ApproachTags.Precision, 3, 1, // Requires Precision 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Precision, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Environmental Precision",
            "Your practiced ability to leverage the environment with perfect precision creates significant advantage.",
            FocusTags.Environment,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Environment, 2, 1, // Requires Environment 2+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Environment, -1) // Negative modification!
        ));

        // Tier 4: Expert
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Surgical Precision",
            "Your movements are so perfectly calculated that they seem almost supernatural in their accuracy.",
            FocusTags.Physical,
            CardTiers.Expert, 4, // Tier 4, +4 momentum
            ApproachTags.Precision, 5, 2, // Requires Precision 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Precision, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Perfect Control",
            "Your masterful precision eliminates all instability from the situation, creating perfect equilibrium.",
            FocusTags.Resource,
            CardTiers.Expert, 3, // Tier 4, -3 pressure
            ApproachTags.Precision, 5, 2, // Requires Precision 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Precision, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // Tier 5: Master
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Ultimate Precision",
            "Your movements achieve a level of perfection that seems impossible, creating extraordinary results.",
            FocusTags.Physical,
            CardTiers.Master, 5, // Tier 5, +5 momentum
            ApproachTags.Precision, 8, 3, // Requires Precision 8+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Precision, -3), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithFocusRequirement(
            "Physical Mastery",
            "Your complete physical control creates a state of perfect flow, eliminating all tension and pressure.",
            FocusTags.Physical,
            CardTiers.Master, 4, // Tier 5, -4 pressure
            FocusTags.Physical, 5, 3, // Requires Physical 5+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Physical, -3) // Negative modification!
        ));

        // =============================================
        // CONCEALMENT-BASED CHOICES
        // =============================================

        // Tier 1: Novice
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Basic Stealth",
            "You attempt to stay unnoticed with simple stealth techniques.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, +0 momentum
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Simple Hiding",
            "You try to make yourself less noticeable.",
            FocusTags.Physical,
            CardTiers.Novice, 0, // Tier 1, -0 pressure
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 2: Trained
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Hidden Advantage",
            "You move stealthily, positioning yourself for an advantageous approach.",
            FocusTags.Physical,
            CardTiers.Trained, 2, // Tier 2, +2 momentum
            ApproachTags.Evasion, 1, 0, // Requires Concealment 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Fade Away",
            "You slip into the shadows, removing yourself from immediate danger.",
            FocusTags.Physical,
            CardTiers.Trained, 1, // Tier 2, -1 pressure
            ApproachTags.Evasion, 1, 0, // Requires Concealment 1+, no reduction
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Tier 3: Adept
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Perfect Stealth",
            "You move with such stealth that even those looking for you cannot detect your presence.",
            FocusTags.Physical,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            ApproachTags.Evasion, 3, 1, // Requires Concealment 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Evasion, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Vanishing Act",
            "You disappear from perception entirely, eliminating immediate threats through complete concealment.",
            FocusTags.Environment,
            CardTiers.Adept, 2, // Tier 3, -2 pressure
            ApproachTags.Evasion, 3, 1, // Requires Concealment 3+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Evasion, -1), // Negative modification!
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithFocusRequirement(
            "Hidden Knowledge",
            "Your practiced ability to uncover concealed information gives you a significant tactical advantage.",
            FocusTags.Information,
            CardTiers.Adept, 3, // Tier 3, +3 momentum
            FocusTags.Information, 2, 1, // Requires Information 2+, reduces by 1
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForFocus(FocusTags.Information, -1) // Negative modification!
        ));

        // Tier 4: Expert
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Shadow Master",
            "You move through the world like a ghost, invisible to all but the most perceptive observers.",
            FocusTags.Physical,
            CardTiers.Expert, 4, // Tier 4, +4 momentum
            ApproachTags.Evasion, 5, 2, // Requires Concealment 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Evasion, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithApproachRequirement(
            "Perfect Misdirection",
            "Your masterful manipulation of attention eliminates all awareness of your actual presence and purpose.",
            FocusTags.Relationship,
            CardTiers.Expert, 3, // Tier 4, -3 pressure
            ApproachTags.Evasion, 5, 2, // Requires Concealment 5+, reduces by 2
            TagModification.ForEncounterState(ApproachTags.Evasion, -2), // Negative modification!
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Tier 5: Master
        _choices.Add(ChoiceFactory.CreateMomentumChoiceWithApproachRequirement(
            "Legendary Shadow",
            "You achieve the mythical state of perfect concealment, controlling perception so completely you might as well be invisible.",
            FocusTags.Environment,
            CardTiers.Master, 5, // Tier 5, +5 momentum
            ApproachTags.Evasion, 8, 3, // Requires Concealment 8+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Evasion, -3), // Negative modification!
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _choices.Add(ChoiceFactory.CreatePressureChoiceWithFocusRequirement(
            "Complete Invisibility",
            "Your absolute mastery of stealth and misdirection creates a state where all pressure evaporates as opponents lose track of you entirely.",
            FocusTags.Physical,
            CardTiers.Master, 4, // Tier 5, -4 pressure
            FocusTags.Physical, 5, 3, // Requires Physical 5+, reduces by 3
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForFocus(FocusTags.Physical, -3) // Negative modification!
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