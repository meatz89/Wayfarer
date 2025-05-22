public class EncounterOption
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public SkillTypes Skill { get; set; }
    public int Difficulty { get; set; }
    public int SuccessProgress { get; set; }
    public int FailureProgress { get; set; }
    public int LocationModifier { get; set; }
    public bool IsDisabled { get; set; }

    // Universal Encounter System additions
    public int FocusCost { get; set; }
    public UniversalActionType ActionType { get; set; }
    public Dictionary<AspectTokenTypes, int> TokenGeneration { get; set; }
    public Dictionary<AspectTokenTypes, int> TokenCosts { get; set; }
    public NegativeConsequenceTypes NegativeConsequenceType { get; set; }
    public List<string> Tags { get; set; }

    public EncounterOption()
    {
        TokenGeneration = new Dictionary<AspectTokenTypes, int>();
        TokenCosts = new Dictionary<AspectTokenTypes, int>();
        Tags = new List<string>();
        FocusCost = 1;
        ActionType = UniversalActionType.GenerateForce;
    }

    public EncounterOption(string id, string name) : this()
    {
        Id = id;
        Name = name;
    }

    public bool RequiresTokens()
    {
        return TokenCosts.Values.Any(cost => cost > 0);
    }

    public bool GeneratesTokens()
    {
        return TokenGeneration.Values.Any(generation => generation > 0);
    }

    public int GetTokenGeneration(AspectTokenTypes tokenType)
    {
        return TokenGeneration.TryGetValue(tokenType, out int value) ? value : 0;
    }

    public int GetTokenCost(AspectTokenTypes tokenType)
    {
        return TokenCosts.TryGetValue(tokenType, out int value) ? value : 0;
    }
}