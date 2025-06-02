public class ChoiceProjection
{
    // Core Choice Data
    public string ChoiceID { get; }
    public string NarrativeText { get; }
    public int FocusCost { get; }
    public EncounterChoice Choice { get; }

    // Narrative & Descriptions
    public string NarrativeDescription { get; set; }
    public string FormattedOutcomeSummary { get; set; }
    public string MechanicalDescription { get; set; }

    // Affordability & Availability
    public bool IsAffordable { get; set; }
    public bool IsAffordableFocus { get; set; }
    public bool IsAffordableAspectTokens { get; set; }
    public bool WillEncounterEnd { get; set; }

    // Progress & Rewards
    public int ProgressGained { get; set; }
    public int FocusPointsGained { get; set; }

    // Skill Check Data
    public bool HasSkillCheck { get; set; }
    public SkillTypes SkillUsed { get; }
    public int BaseSkillLevel { get; set; }
    public int LocationModifierValue { get; set; }
    public int EffectiveSkillLevel { get; set; }
    public int SkillCheckDifficulty { get; }
    public bool SkillCheckSuccess { get; set; }
    public SkillOptionProjection SkillOption { get; set; }

    // Consequences & Outcomes
    public NegativeConsequenceTypes NegativeConsequenceType { get; }
    public EncounterStageOutcomes ProjectedOutcome { get; set; }

    public ChoiceProjection(EncounterChoice choice)
    {
        ChoiceID = choice.ChoiceID;
        NarrativeText = choice.NarrativeText;
        FocusCost = choice.FocusCost;
        Choice = choice;
    }
}

public class EffectProjection
{
    public string NarrativeEffect { get; set; }
    public string MechanicalDescription { get; set; }
}