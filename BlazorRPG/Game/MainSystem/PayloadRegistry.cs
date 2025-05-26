public class PayloadRegistry
{
    private readonly Dictionary<string, IMechanicalEffect> registeredEffects;

    public PayloadRegistry()
    {
        registeredEffects = new Dictionary<string, IMechanicalEffect>();
        InitializePayloadRegistry();
    }

    public void RegisterEffect(string effectID, IMechanicalEffect effect)
    {
        registeredEffects[effectID] = effect;
    }

    public IMechanicalEffect GetEffect(string effectID)
    {
        return registeredEffects.TryGetValue(effectID, out IMechanicalEffect effect) ? effect : null;
    }

    public bool HasEffect(string effectID)
    {
        return registeredEffects.ContainsKey(effectID);
    }

    private void InitializePayloadRegistry()
    {
        registeredEffects["SET_FLAG_TRUST_ESTABLISHED"] = new SetFlagEffect(FlagStates.TrustEstablished);
        registeredEffects["SET_FLAG_INSIGHT_GAINED"] = new SetFlagEffect(FlagStates.InsightGained);
        registeredEffects["SET_FLAG_PATH_CLEARED"] = new SetFlagEffect(FlagStates.PathCleared);
        registeredEffects["SET_FLAG_ADVANTAGEOUS_POSITION"] = new SetFlagEffect(FlagStates.AdvantageousPosition);
        registeredEffects["SET_FLAG_SECRET_REVEALED"] = new SetFlagEffect(FlagStates.SecretRevealed);
        registeredEffects["SET_FLAG_RESPECT_EARNED"] = new SetFlagEffect(FlagStates.RespectEarned);
        registeredEffects["SET_FLAG_SURPRISE_ACHIEVED"] = new SetFlagEffect(FlagStates.SurpriseAchieved);
        registeredEffects["SET_FLAG_PREPARATION_COMPLETED"] = new SetFlagEffect(FlagStates.PreparationCompleted);
        registeredEffects["SET_FLAG_RESOURCE_SECURED"] = new SetFlagEffect(FlagStates.ResourceSecured);
        registeredEffects["SET_FLAG_AREA_SECURED"] = new SetFlagEffect(FlagStates.AreaSecured);
        registeredEffects["SET_FLAG_DISTRACTION_CREATED"] = new SetFlagEffect(FlagStates.DistractionCreated);
        registeredEffects["SET_FLAG_HAZARD_NEUTRALIZED"] = new SetFlagEffect(FlagStates.HazardNeutralized);
        registeredEffects["SET_FLAG_CONFIDENCE_BUILT"] = new SetFlagEffect(FlagStates.ConfidenceBuilt);
        registeredEffects["SET_FLAG_TENSION_INCREASED"] = new SetFlagEffect(FlagStates.TensionIncreased);
        registeredEffects["SET_FLAG_URGENCY_CREATED"] = new SetFlagEffect(FlagStates.UrgencyCreated);

        registeredEffects["CLEAR_FLAG_DISTRUST_TRIGGERED"] = new ClearFlagEffect(FlagStates.DistrustTriggered);
        registeredEffects["CLEAR_FLAG_HOSTILITY_PROVOKED"] = new ClearFlagEffect(FlagStates.HostilityProvoked);
        registeredEffects["CLEAR_FLAG_CONFUSION_CREATED"] = new ClearFlagEffect(FlagStates.ConfusionCreated);
        registeredEffects["CLEAR_FLAG_PATH_BLOCKED"] = new ClearFlagEffect(FlagStates.PathBlocked);
        registeredEffects["CLEAR_FLAG_OBSTACLE_PRESENT"] = new ClearFlagEffect(FlagStates.ObstaclePresent);
        registeredEffects["CLEAR_FLAG_FEAR_INSTILLED"] = new ClearFlagEffect(FlagStates.FearInstilled);

        registeredEffects["GAIN_FOCUS_1"] = new FocusChangeEffect(1);
        registeredEffects["GAIN_FOCUS_2"] = new FocusChangeEffect(2);
        registeredEffects["LOSE_FOCUS_1"] = new FocusChangeEffect(-1);
        registeredEffects["LOSE_FOCUS_2"] = new FocusChangeEffect(-2);

        registeredEffects["ADVANCE_DURATION_1"] = new DurationAdvanceEffect(1);
        registeredEffects["ADVANCE_DURATION_2"] = new DurationAdvanceEffect(2);

        registeredEffects["GAIN_CURRENCY_MINOR"] = new CurrencyChangeEffect(1, 3);
        registeredEffects["GAIN_CURRENCY_MODERATE"] = new CurrencyChangeEffect(4, 7);
        registeredEffects["GAIN_CURRENCY_MAJOR"] = new CurrencyChangeEffect(8, 10);
        registeredEffects["LOSE_CURRENCY_MINOR"] = new CurrencyChangeEffect(-3, -1);
        registeredEffects["LOSE_CURRENCY_MODERATE"] = new CurrencyChangeEffect(-7, -4);
        registeredEffects["LOSE_CURRENCY_MAJOR"] = new CurrencyChangeEffect(-10, -8);

        registeredEffects["BUFF_NEXT_CHECK_1"] = new NextCheckModifierEffect(1);
        registeredEffects["BUFF_NEXT_CHECK_2"] = new NextCheckModifierEffect(2);
        registeredEffects["DEBUFF_NEXT_CHECK_1"] = new NextCheckModifierEffect(-1);
        registeredEffects["DEBUFF_NEXT_CHECK_2"] = new NextCheckModifierEffect(-2);

        registeredEffects["MODIFY_RELATIONSHIP_POSITIVE_MINOR"] = new RelationshipModifierEffect(5, "Recent positive interaction");
        registeredEffects["MODIFY_RELATIONSHIP_POSITIVE_MODERATE"] = new RelationshipModifierEffect(10, "Significant positive interaction");
        registeredEffects["MODIFY_RELATIONSHIP_POSITIVE_MAJOR"] = new RelationshipModifierEffect(15, "Major positive development");
        registeredEffects["MODIFY_RELATIONSHIP_NEGATIVE_MINOR"] = new RelationshipModifierEffect(-5, "Recent negative interaction");
        registeredEffects["MODIFY_RELATIONSHIP_NEGATIVE_MODERATE"] = new RelationshipModifierEffect(-10, "Significant negative interaction");
        registeredEffects["MODIFY_RELATIONSHIP_NEGATIVE_MAJOR"] = new RelationshipModifierEffect(-15, "Major negative development");

        registeredEffects["RECOVERY_SUCCESS"] = new RecoveryEffect(true);
        registeredEffects["RECOVERY_FAILURE"] = new RecoveryEffect(false);

        registeredEffects["SUCCESS_WITH_INSIGHT"] = new CompoundEffect(new List<IMechanicalEffect> {
            new SetFlagEffect(FlagStates.InsightGained),
            new FocusChangeEffect(1)
        });
    }
}

