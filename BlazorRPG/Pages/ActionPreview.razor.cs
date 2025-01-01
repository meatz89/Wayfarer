using Microsoft.AspNetCore.Components;

public partial class ActionPreviewBase : ComponentBase
{
    [Parameter] public UserActionOption CurrentAction { get; set; }
    [Parameter] public Player Player { get; set; }  // Add this
    [Parameter] public EventCallback<bool> OnActionConfirmed { get; set; }
    [Parameter] public EventCallback OnBack { get; set; }

    public string GetRequirementDescription(IRequirement requirement)
    {
        return requirement switch
        {
            PhysicalEnergyRequirement r => $"Physical Energy Required: {r.Amount}",
            FocusEnergyRequirement r => $"Focus Energy Required: {r.Amount}",
            SocialEnergyRequirement r => $"Social Energy Required: {r.Amount}",
            InventorySlotsRequirement r => $"Empty Inventory Slots Required: {r.Count}",
            HealthRequirement r => $"Health Required: {r.Amount}",
            CoinsRequirement r => $"Coins Required: {r.Amount}",
            FoodRequirement r => $"Food Required: {r.Amount}",
            SkillLevelRequirement r => $"Skill Required: {r.SkillType} level {r.Amount}",
            ItemRequirement r => $"Requires {r.Count} x {r.ResourceType}",
            _ => string.Empty
        };
    }

    private string GetValueColor(int amount)
    {
        return amount >= 0 ? "positive" : "negative";
    }

    private string FormatValuePreview(IOutcome outcome)
    {
        return outcome switch
        {
            PhysicalEnergyOutcome o => $"({Player.PhysicalEnergy} -> <span class='{GetValueColor(o.Amount)}'>{Math.Clamp(Player.PhysicalEnergy + o.Amount, 0, Player.MaxPhysicalEnergy)}</span>)",
            FocusEnergyOutcome o => $"({Player.FocusEnergy} -> <span class='{GetValueColor(o.Amount)}'>{Math.Clamp(Player.FocusEnergy + o.Amount, 0, Player.MaxFocusEnergy)}</span>)",
            SocialEnergyOutcome o => $"({Player.SocialEnergy} -> <span class='{GetValueColor(o.Amount)}'>{Math.Clamp(Player.SocialEnergy + o.Amount, 0, Player.MaxSocialEnergy)}</span>)",
            HealthOutcome o => $"({Player.Health} -> <span class='{GetValueColor(o.Amount)}'>{Math.Clamp(Player.Health + o.Amount, 0, Player.MaxHealth)}</span>)",
            CoinsOutcome o => $"({Player.Coins} -> <span class='{GetValueColor(o.Amount)}'>{Math.Max(0, Player.Coins + o.Amount)}</span>)",
            FoodOutcome o => $"({Player.Inventory.GetItemCount(ResourceTypes.Food)} -> <span class='{GetValueColor(o.Amount)}'>{GetNewFood(o)}</span>)",
            SkillLevelOutcome o => $"({Player.Skills[o.SkillType]} -> <span class='{GetValueColor(o.Amount)}'>{Math.Max(0, Player.Skills[o.SkillType] + o.Amount)}</span>)",
            ItemOutcome o => $"({Player.Inventory.GetItemCount(o.ResourceType)} -> <span class='{GetValueColor(o.Count)}'>{GetNewItemCount(o)}</span>)",
            _ => string.Empty
        };
    }

    private int GetNewFood(FoodOutcome o)
    {
        return Math.Max(0, Player.Inventory.GetItemCount(ResourceTypes.Food) + o.Amount);
    }

    private int GetNewItemCount(ItemOutcome o)
    {
        if(o.ChangeType == ItemChangeType.Added)
        {
            return Math.Max(0, Player.Inventory.GetItemCount(o.ResourceType) + o.Count);
        }
        else
        {
            return Math.Max(0, Player.Inventory.GetItemCount(o.ResourceType) - o.Count);
        }
    }

    public MarkupString GetOutcomeDescription(IOutcome outcome)
    {
        string description = outcome switch
        {
            PhysicalEnergyOutcome o => $"Physical Energy {FormatValuePreview(o)}",
            FocusEnergyOutcome o => $"Focus Energy {FormatValuePreview(o)}",
            SocialEnergyOutcome o => $"Social Energy {FormatValuePreview(o)}",
            HealthOutcome o => $"Health {FormatValuePreview(o)}",
            CoinsOutcome o => $"Coins {FormatValuePreview(o)}",
            FoodOutcome o => $"Food {FormatValuePreview(o)}",
            SkillLevelOutcome o => $"Skill {o.SkillType} {FormatValuePreview(o)}",
            ItemOutcome o => $"{(o.ChangeType == ItemChangeType.Added ? "Gain" : "Lose")} {o.Count} x {o.ResourceType} {FormatValuePreview(o)}",
            _ => string.Empty
        };
        return new MarkupString(description);
    }

    protected string GetRequirementColor(IRequirement requirement)
    {
        return requirement switch
        {
            PhysicalEnergyRequirement r => Player.PhysicalEnergy >= r.Amount ? "green" : "red",
            FocusEnergyRequirement r => Player.FocusEnergy >= r.Amount ? "green" : "red",
            SocialEnergyRequirement r => Player.SocialEnergy >= r.Amount ? "green" : "red",
            InventorySlotsRequirement r => Player.Inventory.GetEmptySlots() >= r.Count ? "green" : "red",
            HealthRequirement r => Player.Health >= r.Amount ? "green" : "red",
            CoinsRequirement r => Player.Coins >= r.Amount ? "green" : "red",
            FoodRequirement r => Player.Inventory.GetItemCount(ResourceTypes.Food) >= r.Amount ? "green" : "red",
            SkillLevelRequirement r => Player.Skills.ContainsKey(r.SkillType) && Player.Skills[r.SkillType] >= r.Amount ? "green" : "red",
            ItemRequirement r => Player.Inventory.GetItemCount(r.ResourceType) >= r.Count ? "green" : "red",
            _ => "black"
        };
    }

    public bool IsCost(IOutcome outcome)
    {
        return outcome switch
        {
            PhysicalEnergyOutcome o => o.Amount < 0,
            FocusEnergyOutcome o => o.Amount < 0,
            SocialEnergyOutcome o => o.Amount < 0,
            HealthOutcome o => o.Amount < 0,
            CoinsOutcome o => o.Amount < 0,
            FoodOutcome o => o.Amount < 0,
            SkillLevelOutcome o => o.Amount < 0,
            ItemOutcome o => o.ChangeType == ItemChangeType.Removed,
            _ => false
        };
    }

    public bool IsReward(IOutcome outcome)
    {
        return outcome switch
        {
            PhysicalEnergyOutcome o => o.Amount >= 0,
            FocusEnergyOutcome o => o.Amount >= 0,
            SocialEnergyOutcome o => o.Amount >= 0,
            HealthOutcome o => o.Amount >= 0,
            CoinsOutcome o => o.Amount >= 0,
            FoodOutcome o => o.Amount >= 0,
            SkillLevelOutcome o => o.Amount >= 0,
            ItemOutcome o => o.ChangeType == ItemChangeType.Added,
            _ => false
        };
    }

    public async Task HandleConfirm()
    {
        await OnActionConfirmed.InvokeAsync(true);
    }

    public async Task HandleBack()
    {
        await OnBack.InvokeAsync();
    }
}