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

public class EndDayOutcome : IOutcome
{
}

public class SkillLevelOutcome : IOutcome
{
    public SkillTypes SkillType { get; set; }
    public int Amount { get; set; }
}

public class ItemOutcome : IOutcome
{
    public ItemChangeType ChangeType { get; set; }
    public ResourceTypes ResourceType { get; set; }
    public int Count { get; set; }
}