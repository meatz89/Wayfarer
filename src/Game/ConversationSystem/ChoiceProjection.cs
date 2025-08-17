public class ChoiceProjection
{
    // Core Choice Data
    public string ChoiceID { get; }
    public string NarrativeText { get; }
    public int PatienceCost { get; }
    public ConversationChoice Choice { get; }

    // Narrative & Descriptions
    public string NarrativeDescription { get; set; }
    public string FormattedOutcomeSummary { get; set; }
    public string MechanicalDescription { get; set; }

    // Affordability & Availability
    public bool IsAffordable { get; set; }
    public bool IsAffordableFocus { get; set; }
    public bool IsAffordableAspectTokens { get; set; }
    public bool WillConversationEnd { get; set; }

    // Progress & Rewards
    public int ProgressGained { get; set; }
    public int FocusPointsGained { get; set; }

    // Skill Check Data
    public int LocationModifierValue { get; set; }
    public SkillOptionProjection SkillOption { get; set; }

    // Consequences
    public NegativeConsequenceTypes NegativeConsequenceType { get; }
    // Outcomes removed - consequences flow from choices, not endings

    public ChoiceProjection(ConversationChoice choice)
    {
        ChoiceID = choice.ChoiceID;
        NarrativeText = choice.NarrativeText;
        PatienceCost = choice.PatienceCost;
        Choice = choice;
    }
}

public class EffectProjection
{
    public string NarrativeEffect { get; set; }
    public string MechanicalDescription { get; set; }
}