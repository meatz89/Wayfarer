// Skill system removed - conversations use tokens and relationships
public class SkillCheckRequirement
{
    public string RequirementType { get; private set; }
    public int Difficulty { get; private set; }

    public SkillCheckRequirement(string requirementType, int difficulty)
    {
        RequirementType = requirementType;
        Difficulty = difficulty;
    }

    // For JSON serialization
    public object ToJsonObject()
    {
        return new
        {
            RequirementType = RequirementType,
            Difficulty = Difficulty
        };
    }
}