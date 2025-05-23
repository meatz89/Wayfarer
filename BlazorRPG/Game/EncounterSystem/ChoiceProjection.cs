public class ChoiceProjection
{
    public EncounterOption Choice { get; }
    public string NarrativeDescription { get; set; }
    public string FormattedOutcomeSummary { get; set; }

    // --- Cost & Affordability ---
    public int FocusCost { get; set; }
    public bool IsAffordableFocus { get; set; }
    public Dictionary<AspectTokenTypes, int> AspectTokenCosts { get; private set; }
    public bool IsAffordableAspectTokens { get; set; }
    public bool IsDisabled => !IsAffordableFocus || (Choice.RequiresTokens() && !IsAffordableAspectTokens);

    // --- Projected Positive Effects (if affordable and chosen) ---
    public Dictionary<AspectTokenTypes, int> AspectTokensGained { get; private set; }
    public int ProgressGained { get; set; }
    public int FocusPointsGained { get; set; }

    // --- Skill Check Details ---
    public bool HasSkillCheck { get; set;  }
    public SkillTypes SkillUsed { get; }
    public int BaseSkillLevel { get; set; }
    public int LocationModifierValue { get; set; }
    public int EffectiveSkillLevel { get; set; }
    public int SkillCheckDifficulty { get; }
    public bool SkillCheckSuccess { get; set; }

    // --- Projected Negative Consequence (if skill check fails) ---
    public NegativeConsequenceTypes NegativeConsequenceType { get; }
    public string MechanicalDescription { get; set; } 

    // --- Encounter End & Outcome Projection ---
    public bool WillEncounterEnd { get; set; }
    public EncounterOutcomes ProjectedOutcome { get; set; }

    public ChoiceProjection(EncounterOption choice)
    {
        Choice = choice;
        NarrativeDescription = choice.Description;

        FocusCost = choice.FocusCost;

        AspectTokensGained = new Dictionary<AspectTokenTypes, int>(choice.TokenGeneration ?? new Dictionary<AspectTokenTypes, int>());
        AspectTokenCosts = new Dictionary<AspectTokenTypes, int>(choice.TokenCosts ?? new Dictionary<AspectTokenTypes, int>());
        ProgressGained = choice.SuccessProgress; 

        SkillUsed = choice.Skill;
        HasSkillCheck = choice.Skill != SkillTypes.None;
        SkillCheckDifficulty = choice.Difficulty;
        NegativeConsequenceType = choice.NegativeConsequenceType;
    }

    public int GetTokenGain(AspectTokenTypes tokenType) => AspectTokensGained.TryGetValue(tokenType, out int value) ? value : 0;
    public int GetTokenCost(AspectTokenTypes tokenType) => AspectTokenCosts.TryGetValue(tokenType, out int value) ? value : 0;
    public Dictionary<AspectTokenTypes, int> GetAllTokenGains() => new Dictionary<AspectTokenTypes, int>(AspectTokensGained);
    public Dictionary<AspectTokenTypes, int> GetAllTokenCosts() => new Dictionary<AspectTokenTypes, int>(AspectTokenCosts);
}