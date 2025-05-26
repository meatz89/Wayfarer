public class ChoiceProjection
{
    public string ChoiceID { get; }
    public string NarrativeText { get; }
    public int FocusCost { get; }
    public bool IsAffordable { get; set; }
    public List<SkillOptionProjection> SkillOptions { get; set; }
    public AiChoice Choice { get; }
    public string NarrativeDescription { get; set; }
    public string FormattedOutcomeSummary { get; set; }
    public bool IsAffordableFocus { get; set; }
    public bool IsAffordableAspectTokens { get; set; }
    public int ProgressGained { get; set; }
    public int FocusPointsGained { get; set; }
    public bool HasSkillCheck { get; set; }
    public SkillTypes SkillUsed { get; }
    public int BaseSkillLevel { get; set; }
    public int LocationModifierValue { get; set; }
    public int EffectiveSkillLevel { get; set; }
    public int SkillCheckDifficulty { get; }
    public bool SkillCheckSuccess { get; set; }
    public NegativeConsequenceTypes NegativeConsequenceType { get; }
    public string MechanicalDescription { get; set; }
    public bool WillEncounterEnd { get; set; }
    public EncounterOutcomes ProjectedOutcome { get; set; }

    public ChoiceProjection(AiChoice choice)
    {
        ChoiceID = choice.ChoiceID;
        NarrativeText = choice.NarrativeText;
        FocusCost = choice.FocusCost;
        SkillOptions = new List<SkillOptionProjection>();
    }
}

public class PayloadProjection
{
    public string NarrativeEffect { get; set; }
    public string MechanicalDescription { get; set; }
}