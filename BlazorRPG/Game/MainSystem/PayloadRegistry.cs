public class PayloadRegistry
{
    private Dictionary<string, IMechanicalEffect> registeredEffects = new Dictionary<string, IMechanicalEffect>();

    public PayloadRegistry(ILogger<PayloadRegistry> logger)
    {
        InitializeEffects();
    }

    public void RegisterEffect(string effectID, IMechanicalEffect effect)
    {
        if (!registeredEffects.ContainsKey(effectID))
        {
            registeredEffects[effectID] = effect;
        }
        else
        {
            throw new Exception("Effect ID {EffectID} already registered");
        }
    }

    public IMechanicalEffect GetEffect(string effectID)
    {
        if (registeredEffects.TryGetValue(effectID, out IMechanicalEffect effect))
        {
            return effect;
        }

        throw new Exception("Effect ID {EffectID} not found");
    }

    public bool HasEffect(string effectID)
    {
        return registeredEffects.ContainsKey(effectID);
    }

    private void InitializeEffects()
    {
        // State flag payloads
        RegisterEffect("SET_FLAG_TRUST_ESTABLISHED",
            new SetFlagEffect(FlagStates.TrustEstablished));
        RegisterEffect("SET_FLAG_INSIGHT_GAINED",
            new SetFlagEffect(FlagStates.InsightGained));
        RegisterEffect("SET_FLAG_PATH_CLEARED",
            new SetFlagEffect(FlagStates.PathCleared));
        RegisterEffect("SET_FLAG_ADVANTAGEOUS_POSITION",
            new SetFlagEffect(FlagStates.AdvantageousPosition));
        RegisterEffect("CLEAR_FLAG_DISTRUST_TRIGGERED",
            new ClearFlagEffect(FlagStates.DistrustTriggered));

        // Focus payloads
        RegisterEffect("GAIN_FOCUS_1",
            new FocusChangeEffect(1));
        RegisterEffect("GAIN_FOCUS_2",
            new FocusChangeEffect(2));
        RegisterEffect("LOSE_FOCUS_1",
            new FocusChangeEffect(-1));

        // Duration payloads
        RegisterEffect("ADVANCE_DURATION_1",
            new DurationAdvanceEffect(1));
        RegisterEffect("ADVANCE_DURATION_2",
            new DurationAdvanceEffect(2));

        // Progress payloads
        RegisterEffect("GAIN_PROGRESS_2",
            new ProgressChangeEffect(2));
        RegisterEffect("GAIN_PROGRESS_4",
            new ProgressChangeEffect(4));
        RegisterEffect("GAIN_PROGRESS_6",
            new ProgressChangeEffect(6));
        RegisterEffect("LOSE_PROGRESS_1",
            new ProgressChangeEffect(-1));

        // Combined effects for common patterns
        RegisterEffect("SUCCESS_WITH_INSIGHT",
            new CompoundEffect(new List<IMechanicalEffect> {
                new SetFlagEffect(FlagStates.InsightGained),
                new ProgressChangeEffect(2)
            }));
    }
}