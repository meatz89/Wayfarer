public class Choice
{
    public string Id { get; set; }
    public string Name { get; set; }
    public EffectTypes EffectType { get; set; }
    public ApproachTypes Approach { get; set; }
    public FocusTypes Focus { get; set; }
    public string Description { get; set; }
    public string Narrative { get; set; } = "None";
    public bool IsEncounterWinningChoice { get; internal set; } = false;
    public bool IsEncounterFailingChoice { get; internal set; } = false;

    // The effect value depends on the balance state
    public int GetEffectValue(bool isStable)
    {
        if (EffectType == EffectTypes.Momentum && !isStable)
            return 2; // Unstable momentum gives +2

        return 1; // All other effects give +1
    }

    public override string ToString()
    {
        return $"{Name} ({EffectType}, {Approach}, {Focus})";
    }
}
