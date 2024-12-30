using Microsoft.AspNetCore.Components;

public partial class ActionPreviewBase : ComponentBase
{
    [Parameter] public UserActionOption CurrentAction { get; set; }
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
            ItemRequirement r => $"Item Required: {r.Name}",
            _ => string.Empty
        };
    }

    public string GetOutcomeDescription(IOutcome outcome)
    {
        return outcome switch
        {
            PhysicalEnergyOutcome o => $"Physical Energy: {o.Amount}",
            FocusEnergyOutcome o => $"Focus Energy: {o.Amount}",
            SocialEnergyOutcome o => $"Social Energy: {o.Amount}",
            HealthOutcome o => $"Health: {o.Amount}",
            CoinsOutcome o => $"Coins: {o.Amount}",
            FoodOutcome o => $"Food: {o.Amount}",
            SkillLevelOutcome o => $"Skill {o.SkillType}: {o.Amount}",
            ItemOutcome o => $"Item: {o.Name}",
            _ => string.Empty
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