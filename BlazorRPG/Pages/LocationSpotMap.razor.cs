using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorRPG.Pages;

public partial class LocationSpotMap : ComponentBase
{
    [Inject] private GameWorldManager GameManager { get; set; }
    [Inject] private GameWorld GameState { get; set; }
    [Inject] private LocationSystem LocationSystem { get; set; }
    [Inject] private CardSelectionService DragDropService { get; set; }
    [Inject] private CardHighlightService CardHighlightService { get; set; }

    [Parameter] public Location CurrentLocation { get; set; }
    [Parameter] public LocationSpot CurrentSpot { get; set; }
    [Parameter] public EventCallback<LocationSpot> OnSpotSelected { get; set; }
    [Parameter] public EventCallback<UserActionOption> OnActionSelected { get; set; }

    private bool showTooltip;
    private UserActionOption selectedAction = null;
    private ApproachDefinition hoveredApproach = null;
    private double mouseX;
    private double mouseY;

    protected override void OnInitialized()
    {
        DragDropService.OnStateChanged += StateHasChanged;
    }

    private async Task HandleApproachSelection(UserActionOption action, ApproachDefinition approach)
    {
        bool isCompatible = DragDropService.IsValidDropTarget(approach.RequiredCardType);
        if (isCompatible)
        {
            SkillCard card = DragDropService.SelectedCard;
            DragDropService.Reset();
        }
        await SelectApproach(action, approach);
    }

    private async Task HandleActionSelection(UserActionOption action)
    {
        showTooltip = false;

        await OnActionSelected.InvokeAsync(action);

        selectedAction = null;
    }

    private void ActivateHighlightMode(SkillCategories cardType)
    {
        if (CardHighlightService.IsHighlightModeActive)
        {
            CardHighlightService.DeactivateHighlightMode();
        }
        else
        {
            CardHighlightService.ActivateHighlightMode(cardType, HighlightMode.Highlight);
        }
    }

    public void Dispose()
    {
        DragDropService.OnStateChanged -= StateHasChanged;
    }

    private bool IsValidCardForApproach(SkillCategories requiredCardType)
    {
        bool isValidTarget = DragDropService.IsValidDropTarget(requiredCardType);
        return true;
    }

    private string GetCardTypeClass(SkillCategories type)
    {
        return type switch
        {
            SkillCategories.Physical => "physical",
            SkillCategories.Intellectual => "intellectual",
            SkillCategories.Social => "social",
            _ => ""
        };
    }

    private void ToggleActionApproaches(UserActionOption action)
    {
        if (selectedAction?.locationAction.ActionId == action.locationAction.ActionId)
        {
            selectedAction = null; // Collapse if already selected
        }
        else
        {
            selectedAction = action; // Expand this action
        }
    }

    private async Task SelectApproach(UserActionOption action, ApproachDefinition approach)
    {
        showTooltip = false;

        //if (!GameState.PlayerState.HasAvailableCard(approach.RequiredCardType))
        //{
        //    return;
        //}

        UserActionOption actionWithApproach = action with { ApproachId = approach.Id };

        await OnActionSelected.InvokeAsync(actionWithApproach);

        // Reset selection
        selectedAction = null;
    }

    private void HandleShowApproachTooltip(UserActionOption action, ApproachDefinition approach, MouseEventArgs e)
    {
        selectedAction = action;
        hoveredApproach = approach;
        mouseX = e.ClientX;
        mouseY = e.ClientY;
        showTooltip = true;
    }

    private void HandleHideTooltip()
    {
        hoveredApproach = null;
        showTooltip = false;
    }

    public List<LocationSpot> GetKnownSpots()
    {
        List<LocationSpot> locationSpots = LocationSystem.GetLocationSpots(CurrentLocation.Id);
        return locationSpots;
    }

    public Location GetLocation()
    {
        return LocationSystem.GetLocation(CurrentLocation.Id);
    }

}