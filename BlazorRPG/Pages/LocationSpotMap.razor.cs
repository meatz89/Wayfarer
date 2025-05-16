using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Threading;

namespace BlazorRPG.Pages;

public partial class LocationSpotMap : ComponentBase
{
    [Inject] private GameManager GameManager { get; set; }
    [Inject] private GameState GameState { get; set; }
    [Inject] private LocationSystem LocationSystem { get; set; }

    [Parameter] public Location CurrentLocation { get; set; }
    [Parameter] public LocationSpot CurrentSpot { get; set; }
    [Parameter] public EventCallback<LocationSpot> OnSpotSelected { get; set; }
    [Parameter] public EventCallback<UserActionOption> OnActionSelected { get; set; }

    private bool showTooltip;
    private UserActionOption selectedAction = null;
    private ApproachOption hoveredApproach = null;
    private double mouseX;
    private double mouseY;

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

    private void SelectApproach(UserActionOption action, ApproachOption approach)
    {
        showTooltip = false;

        if (!GameState.PlayerState.HasAvailableCard(approach.RequiredCardType))
        {
            return;
        }

        // Update the selected action with the chosen approach
        UserActionOption actionWithApproach = action with { SelectedApproach = approach };

        // Execute the action with the selected approach
        OnActionSelected.InvokeAsync(actionWithApproach);

        // Reset selection
        selectedAction = null;
    }

    private void HandleShowApproachTooltip(UserActionOption action, ApproachOption approach, MouseEventArgs e)
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