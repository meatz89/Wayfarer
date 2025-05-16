using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorRPG.Pages;

public partial class LocationSpotMap : ComponentBase
{
    [Inject] private GameManager GameManager { get; set; }
    [Inject] private GameState GameState { get; set; }
    [Inject] private LocationSystem LocationSystem { get; set; }
    [Inject] private CardSelectionService DragDropService { get; set; }
    [Inject] private CardHighlightService CardHighlightService { get; set; }

    [Parameter] public Location CurrentLocation { get; set; }
    [Parameter] public LocationSpot CurrentSpot { get; set; }
    [Parameter] public EventCallback<LocationSpot> OnSpotSelected { get; set; }
    [Parameter] public EventCallback<UserActionOption> OnActionSelected { get; set; }

    private bool showTooltip;
    private UserActionOption selectedAction = null;
    private ActionApproach hoveredApproach = null;
    private double mouseX;
    private double mouseY;

    protected override void OnInitialized()
    {
        DragDropService.OnStateChanged += StateHasChanged;
    }

    private async Task HandleApproachSelection(UserActionOption action, ActionApproach approach)
    {
        // Check if a card is selected and is valid for this approach
        if (DragDropService.IsValidDropTarget(approach.RequiredCardType))
        {
            ActionCardDefinition card = DragDropService.SelectedCard;
            DragDropService.Reset();

            await SelectApproach(action, approach, card);
        }
    }

    private void ActivateHighlightMode(CardTypes cardType)
    {
        CardHighlightService.ActivateHighlightMode(cardType, HighlightMode.Highlight);
    }

    public void Dispose()
    {
        DragDropService.OnStateChanged -= StateHasChanged;
    }

    private bool IsValidCardForApproach(CardTypes requiredCardType)
    {
        return DragDropService.IsValidDropTarget(requiredCardType);
    }

    private string GetCardTypeClass(CardTypes type)
    {
        return type switch
        {
            CardTypes.Physical => "physical",
            CardTypes.Intellectual => "intellectual",
            CardTypes.Social => "social",
            _ => ""
        };
    }

    private void ToggleActionApproaches(UserActionOption action)
    {
        if (selectedAction?.ActionImplementation.Id == action.ActionImplementation.Id)
        {
            selectedAction = null; // Collapse if already selected
        }
        else
        {
            selectedAction = action; // Expand this action
        }
    }

    private async Task SelectApproach(UserActionOption action, ActionApproach approach, ActionCardDefinition card)
    {
        showTooltip = false;

        if (!GameState.PlayerState.HasAvailableCard(approach.RequiredCardType))
        {
            return;
        }

        UserActionOption actionWithApproach = action with { SelectedApproach = approach, SelectedCard = card };

        await OnActionSelected.InvokeAsync(actionWithApproach);

        // Reset selection
        selectedAction = null;
    }

    private void HandleShowApproachTooltip(UserActionOption action, ActionApproach approach, MouseEventArgs e)
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