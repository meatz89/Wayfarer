public class PayloadRegistry
{
    private List<PayloadEntry> registeredEffects;

    public PayloadRegistry()
    {
        registeredEffects = new List<PayloadEntry>();
        RegisterAllStandardEffects();
    }

    public void RegisterEffect(string id, IMechanicalEffect effect)
    {
        // Remove existing if present
        for (int i = registeredEffects.Count - 1; i >= 0; i--)
        {
            if (registeredEffects[i].ID == id)
            {
                registeredEffects.RemoveAt(i);
            }
        }

        registeredEffects.Add(new PayloadEntry { ID = id, Effect = effect });
    }

    public IMechanicalEffect GetEffect(string id)
    {
        for (int i = 0; i < registeredEffects.Count; i++)
        {
            if (registeredEffects[i].ID == id)
            {
                return registeredEffects[i].Effect;
            }
        }
        return new NoEffect(); // Safe fallback
    }

    private void RegisterAllStandardEffects()
    {
        // Flag effects
        RegisterEffect("SET_FLAG_TRUST_ESTABLISHED", new SetFlagEffect(FlagStates.TrustEstablished));
        RegisterEffect("SET_FLAG_INSIGHT_GAINED", new SetFlagEffect(FlagStates.InsightGained));
        RegisterEffect("SET_FLAG_PATH_CLEARED", new SetFlagEffect(FlagStates.PathCleared));
        RegisterEffect("SET_FLAG_ADVANTAGEOUS_POSITION", new SetFlagEffect(FlagStates.AdvantageousPosition));
        RegisterEffect("SET_FLAG_HIDDEN_POSITION", new SetFlagEffect(FlagStates.HiddenPosition));

        // Focus effects
        RegisterEffect("GAIN_FOCUS_1", new ModifyFocusEffect(1));
        RegisterEffect("GAIN_FOCUS_2", new ModifyFocusEffect(2));
        RegisterEffect("LOSE_FOCUS_1", new ModifyFocusEffect(-1));
        RegisterEffect("LOSE_FOCUS_2", new ModifyFocusEffect(-2));

        // Duration effects
        RegisterEffect("ADVANCE_DURATION_1", new AdvanceDurationEffect(1));
        RegisterEffect("ADVANCE_DURATION_2", new AdvanceDurationEffect(2));

        // No effect fallback
        RegisterEffect("NO_EFFECT", new NoEffect());
    }
}

public class NoEffect : IMechanicalEffect
{
    public void Apply(EncounterState state) { }
    public string GetDescriptionForPlayer() { return "No effect"; }
}

public class AdvanceDurationEffect : IMechanicalEffect
{
    private int amount;

    public AdvanceDurationEffect(int amount)
    {
        this.amount = amount;
    }

    public void Apply(EncounterState state)
    {
        state.DurationCounter += amount;
    }

    public string GetDescriptionForPlayer()
    {
        return "Time advances";
    }
}