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
            HealthRequirement r => $"Health Required: {r.Amount}",
            CoinsRequirement r => $"Coins Required: {r.Amount}",
            FoodRequirement r => $"Food Required: {r.Amount}",
            SkillLevelRequirement r => $"Skill Required: {r.SkillType} level {r.Amount}",
            ItemRequirement r => $"Item Required: {r.Item.ToString()}",
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
            FoodOutcome o => $"({Player.Inventory.GetItemCount(ResourceTypes.Food)} -> <span class='{GetValueColor(o.Amount)}'>{Math.Max(0, Player.Inventory.GetItemCount(ResourceTypes.Food) + o.Amount)}</span>)",
            SkillLevelOutcome o => $"({Player.Skills[o.SkillType]} -> <span class='{GetValueColor(o.Amount)}'>{Math.Max(0, Player.Skills[o.SkillType] + o.Amount)}</span>)",
            _ => string.Empty
        };
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
            ItemOutcome o => $"Item {o.ChangeType.ToString()}: {o.Item}",
            _ => string.Empty
        };
        return new MarkupString(description);
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