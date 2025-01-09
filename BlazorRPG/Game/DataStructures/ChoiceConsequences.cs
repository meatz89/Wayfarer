public class ChoiceConsequences
{
    // Base values from choice pattern
    public List<ValueChange> BaseValueChanges { get; set; } = new();
    public List<Requirement> BaseRequirements { get; set; } = new();
    public List<Outcome> BaseCosts { get; set; } = new();
    public List<Outcome> BaseRewards { get; set; } = new();

    // Modified values after all calculations
    public List<ValueChange> ModifiedValueChanges { get; set; } = new();
    public List<Requirement> ModifiedRequirements { get; set; } = new();
    public List<Outcome> ModifiedCosts { get; set; } = new();
    public List<Outcome> ModifiedRewards { get; set; } = new();

    // Modification details for UI/preview
    public List<RequirementModification> RequirementModifications { get; set; } = new();
    public List<OutcomeModification> CostModifications { get; set; } = new();
    public List<OutcomeModification> RewardModifications { get; set; } = new();
    public List<ValueChangeDetail> ValueChangeDetails { get; set; } = new();

    // Modifier sources
    public ChoiceValueModifiers Modifiers { get; set; } = new();
}

public class RequirementModification
{
    public string Source { get; set; }  // e.g. "Pressure Increase"
    public string RequirementType { get; set; } // e.g. "Energy", "Resource"
    public int Amount { get; set; }
}

// New struct to track modifications to costs and rewards
public struct OutcomeModification
{
    public string Source { get; set; }
    public string OutcomeType { get; set; } // e.g., "Health", "Reputation", "Resource", etc.
    public int Amount { get; set; }
}