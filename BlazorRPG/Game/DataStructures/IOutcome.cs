public interface IOutcome
{
}

public class FoodOutcome : IOutcome
{
    public int Amount { get; set; }
}

public class CoinsOutcome : IOutcome
{
    public int Amount { get; set; }
}

public class HealthOutcome : IOutcome
{
    public int Amount { get; set; }
}

public class PhysicalEnergyOutcome : IOutcome
{
    public int Amount { get; set; }
}

public class FocusEnergyOutcome : IOutcome
{
    public int Amount { get; set; }
}

public class SocialEnergyOutcome : IOutcome
{
    public int Amount { get; set; }
}

public class ReputationOutcome : IOutcome
{
    public ReputationTypes ReputationType { get; set; }
    public int Amount { get; set; }
}

public class AchievementOutcome : IOutcome
{
    public AchievementTypes AchievementType { get; set; }
}

public class EndDayOutcome : IOutcome
{
}

public class SkillLevelOutcome : IOutcome
{
    public SkillTypes SkillType { get; set; }
    public int Amount { get; set; }
}

public class ResourceOutcome : IOutcome
{
    public ResourceChangeType ChangeType { get; set; }
    public ResourceTypes Resource { get; set; }
    public int Count { get; set; }
}