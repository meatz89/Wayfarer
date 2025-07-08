public enum GoalType
{
    Core,       // Main story goal
    Supporting, // Major milestone toward core goal
    Opportunity // Optional side goal
}

public class Goal
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public GoalType Type { get; private set; }
    public int CreationDay { get; private set; }
    public int Deadline { get; private set; } // -1 for no deadline
    public List<string> Requirements { get; private set; } = new List<string>();
    public List<string> CompletedRequirements { get; private set; } = new List<string>();
    public bool IsCompleted { get; private set; }
    public bool HasFailed { get; private set; }

    public float Progress
    {
        get
        {
            return Requirements.Count > 0
        ? (float)CompletedRequirements.Count / Requirements.Count
        : 0f;
        }
    }

    public void CompleteRequirement(string requirement)
    {
        if (!Requirements.Contains(requirement) || CompletedRequirements.Contains(requirement))
            return;

        CompletedRequirements.Add(requirement);

        if (CompletedRequirements.Count == Requirements.Count)
        {
            IsCompleted = true;
        }
    }

    public bool CheckFailure(int currentDay)
    {
        if (Deadline != -1 && currentDay > Deadline && !IsCompleted)
        {
            HasFailed = true;
            return true;
        }

        return false;
    }
}