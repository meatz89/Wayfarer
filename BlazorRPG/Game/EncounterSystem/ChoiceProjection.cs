public class ChoiceProjection
{
    public EncounterOption Choice { get; }
    public string Description { get; set; }
    public int ProgressGained { get; set; }
    public bool EncounterWillEnd { get; set; }
    public EncounterOutcomes ProjectedOutcome { get; set; } = EncounterOutcomes.Partial;
    public int HealthChange { get; set; }
    public int ConcentrationChange { get; set; }

    // Universal Encounter System additions
    public int FocusCost { get; set; }
    public bool SkillCheckSuccess { get; set; }
    public bool IsConversionChoice { get; set; }
    public NegativeConsequenceTypes NegativeConsequenceType { get; set; }

    private Dictionary<AspectTokenTypes, int> tokenGains;
    private Dictionary<AspectTokenTypes, int> tokenCosts;

    public ChoiceProjection(EncounterOption choice)
    {
        Choice = choice;
        tokenGains = new Dictionary<AspectTokenTypes, int>();
        tokenCosts = new Dictionary<AspectTokenTypes, int>();
    }

    public void SetTokenGain(AspectTokenTypes tokenType, int amount)
    {
        tokenGains[tokenType] = amount;
    }

    public void SetTokenCost(AspectTokenTypes tokenType, int amount)
    {
        tokenCosts[tokenType] = amount;
    }

    public int GetTokenGain(AspectTokenTypes tokenType)
    {
        return tokenGains.TryGetValue(tokenType, out int value) ? value : 0;
    }

    public int GetTokenCost(AspectTokenTypes tokenType)
    {
        return tokenCosts.TryGetValue(tokenType, out int value) ? value : 0;
    }

    public Dictionary<AspectTokenTypes, int> GetAllTokenGains()
    {
        return new Dictionary<AspectTokenTypes, int>(tokenGains);
    }

    public Dictionary<AspectTokenTypes, int> GetAllTokenCosts()
    {
        return new Dictionary<AspectTokenTypes, int>(tokenCosts);
    }
}