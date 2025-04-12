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

        // Basic momentum card - Analysis approach
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Basic Assessment",
            "You quickly evaluate the situation, looking for immediate insights.",
            FocusTags.Information,
            CardTiers.Novice, 2, // Base effect: +2 momentum
            ApproachTags.Analysis, 1, 1, // Optimal position: Analysis 1, Information 1
            new StrategicEffect(Population.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // Basic pressure card - Precision approach
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Careful Approach",
            "You proceed with caution, deliberately controlling your actions.",
            FocusTags.Physical,
            CardTiers.Novice, 1, // Base effect: -1 pressure
            ApproachTags.Precision, 1, 1, // Optimal position: Precision 1, Physical 1
            new StrategicEffect(Atmosphere.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // =============================================
        // TIER 2: TRAINED CARDS
        // =============================================

        // Momentum card - Rapport approach
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Engaging Presence",
            "You establish a connection that creates immediate progress.",
            FocusTags.Relationship,
            CardTiers.Trained, 3, // Base effect: +3 momentum
            ApproachTags.Rapport, 3, 2, // Optimal position: Rapport 3, Relationship 2
            new StrategicEffect(Population.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Pressure card - Concealment approach
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Strategic Withdrawal",
            "You take a step back to regroup and reduce immediate tension.",
            FocusTags.Environment,
            CardTiers.Trained, 2, // Base effect: -2 pressure
            ApproachTags.Concealment, 3, 2, // Optimal position: Concealment 3, Environment 2
            new StrategicEffect(Illumination.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Concealment),
            TagModification.ForEncounterState(ApproachTags.Concealment, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        // =============================================
        // TIER 3: ADEPT CARDS
        // =============================================

        // Position modification card - Changes player position dramatically
        _choices.Add(new PositionModificationCard(
            "Strategic Pivot",
            "You dramatically change your approach to gain a tactical advantage.",
            FocusTags.Information,
            EffectTypes.Momentum,
            CardTiers.Adept,
            3, // Base effect: +3 momentum
            new RequirementInfo(),
            new StrategicEffect(Atmosphere.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            new List<TagModification> {
                TagModification.ForEncounterState(ApproachTags.Analysis, 1),
                TagModification.ForFocus(FocusTags.Information, 1)
            },
            ApproachTags.Analysis, 4, 3, // Optimal position: Analysis 4, Information 3
            PositionModificationType.IncreaseLowestDecreaseHighest
        ));

        // Position-scaled card - Momentum scales with approach value
        _choices.Add(new PositionScaledChoiceCard(
            "Dominant Display",
            "You leverage your commanding presence for maximum effect.",
            FocusTags.Physical,
            EffectTypes.Momentum,
            CardTiers.Adept,
            2, // Base effect: +2 momentum (plus scaling)
            new RequirementInfo(),
            new StrategicEffect(Population.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance),
            new List<TagModification> {
                TagModification.ForEncounterState(ApproachTags.Dominance, 1),
                TagModification.ForFocus(FocusTags.Physical, 1)
            },
            ApproachTags.Dominance, 4, 2, // Optimal position: Dominance 4, Physical 2
            ScalingType.DirectApproachValue,
            ApproachTags.Dominance
        ));

        // =============================================
        // TIER 4: EXPERT CARDS
        // =============================================

        // Approach differential card - Exploits position differences
        _choices.Add(new PositionScaledChoiceCard(
            "Position Leverage",
            "You exploit the stark contrast between your strongest and weakest approaches.",
            FocusTags.Environment,
            EffectTypes.Momentum,
            CardTiers.Expert,
            3, // Base effect: +3 momentum (plus differential)
            new RequirementInfo(),
            new StrategicEffect(Physical.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            new List<TagModification> {
                TagModification.ForEncounterState(ApproachTags.Analysis, 1),
                TagModification.ForFocus(FocusTags.Environment, 1)
            },
            ApproachTags.Analysis, 5, 2, // Optimal position: Analysis 5, Environment 2
            ScalingType.HighestToLowestDifference
        ));

        // Focus-based card - Works with player's focus investments
        _choices.Add(new PositionScaledChoiceCard(
            "Specialized Focus",
            "Your concentrated attention on a single aspect creates exceptional results.",
            FocusTags.Resource,
            EffectTypes.Momentum,
            CardTiers.Expert,
            3, // Base effect: +3 momentum (plus focus scaling)
            new RequirementInfo(),
            new StrategicEffect(Economic.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            new List<TagModification> {
                TagModification.ForEncounterState(ApproachTags.Precision, 1),
                TagModification.ForFocus(FocusTags.Resource, 1)
            },
            ApproachTags.Precision, 5, 3, // Optimal position: Precision 5, Resource 3
            ScalingType.FocusDifference
        ));

        // =============================================
        // TIER 5: MASTER CARDS
        // =============================================

        // Maximum momentum card - Pure power
        _choices.Add(ChoiceFactory.CreateMomentumChoice(
            "Perfect Execution",
            "You perform with absolute mastery, achieving exceptional results.",
            FocusTags.Physical,
            CardTiers.Master, 6, // Base effect: +6 momentum
            ApproachTags.Precision, 7, 4, // Optimal position: Precision 7, Physical 4
            new StrategicEffect(Physical.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Precision),
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // Maximum pressure reduction card
        _choices.Add(ChoiceFactory.CreatePressureChoice(
            "Complete Control",
            "Your mastery of the situation eliminates complications and pressure.",
            FocusTags.Relationship,
            CardTiers.Master, 5, // Base effect: -5 pressure
            ApproachTags.Rapport, 7, 4, // Optimal position: Rapport 7, Relationship 4
            new StrategicEffect(Atmosphere.Any, StrategicTagEffectType.DecreasePressure, ApproachTags.Rapport),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // Complex position effect card
        _choices.Add(new PositionScaledChoiceCard(
            "Balanced Mastery",
            "Your exceptional balance across all approaches creates unique advantages.",
            FocusTags.Information,
            EffectTypes.Momentum,
            CardTiers.Master,
            4, // Base effect: +4 momentum (plus balance scaling)
            new RequirementInfo(),
            new StrategicEffect(Atmosphere.Any, StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis),
            new List<TagModification> {
                TagModification.ForEncounterState(ApproachTags.Analysis, 1),
                TagModification.ForFocus(FocusTags.Information, 1)
            },
            ApproachTags.Analysis, 6, 4, // Optimal position: Analysis 6, Information 4
            ScalingType.ApproachesWithinTwoPoints
        ));
    }

    public List<ChoiceCard> GetAllChoices()
    {
        return new List<ChoiceCard>(_choices);
    }

    public List<ChoiceCard> GetAvailableChoices(EncounterState state)
    {
        return _choices.Where(c => c.Requirement.IsMet(state.TagSystem))
            .Select(choice => choice)
            .ToList();
    }
}