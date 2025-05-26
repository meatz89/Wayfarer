public class ChoiceProjection
{
    public EncounterOption Choice { get; }
    public string NarrativeDescription { get; set; }
    public string FormattedOutcomeSummary { get; set; }

    public int FocusCost { get; set; }
    public bool IsAffordableFocus { get; set; }
    public bool IsAffordableAspectTokens { get; set; }
    public int ProgressGained { get; set; }
    public int FocusPointsGained { get; set; }

    public bool HasSkillCheck { get; set;  }
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

    public ChoiceProjection(EncounterOption choice)
    {
        Choice = choice;
        NarrativeDescription = choice.Description;

        FocusCost = choice.FocusCost;

        SkillUsed = choice.Skill;
        HasSkillCheck = choice.Skill != SkillTypes.None;
        SkillCheckDifficulty = choice.Difficulty;
        NegativeConsequenceType = choice.NegativeConsequenceType;
    }

}