public interface IRequirement
{
}

public class CoinsRequirement : IRequirement
{
    public int Amount { get; set; }
}

public class FoodRequirement : IRequirement
{
    public int Amount { get; set; }
}

public class InventorySlotsRequirement : IRequirement
{
    public int Count { get; set; }
}

public class HealthRequirement : IRequirement
{
    public int Amount { get; set; }
}

public class PhysicalEnergyRequirement : IRequirement
{
    public int Amount { get; set; }
}

public class FocusEnergyRequirement : IRequirement
{
    public int Amount { get; set; }
}

public class SocialEnergyRequirement : IRequirement
{
    public int Amount { get; set; }
}

public class SkillLevelRequirement : IRequirement
{
    public SkillTypes SkillType { get; set; }
    public int Amount { get; set; }
}

public class ItemRequirement : IRequirement
{
    public ResourceTypes ResourceType { get; set; }
    public int Count { get; set; }
}
