using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Wayfarer.Pages;

public class LocationSpotMapBase : ComponentBase
{
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public LocationSystem LocationSystem { get; set; }
    [Inject] public NPCRepository NPCRepository { get; set; }
    [Inject] public CardSelectionService DragDropService { get; set; }
    [Inject] public CardHighlightService CardHighlightService { get; set; }

    [Parameter] public Location CurrentLocation { get; set; }
    [Parameter] public LocationSpot CurrentSpot { get; set; }
    [Parameter] public EventCallback<LocationSpot> OnSpotSelected { get; set; }
    [Parameter] public EventCallback<UserActionOption> OnActionSelected { get; set; }

    public bool showTooltip;
    public UserActionOption selectedAction = null;
    public ApproachDefinition hoveredApproach = null;
    public double mouseX;
    public double mouseY;

    protected override void OnInitialized()
    {
        DragDropService.OnStateChanged += StateHasChanged;
    }

    public async Task HandleApproachSelection(UserActionOption action, ApproachDefinition approach)
    {
        bool isCompatible = DragDropService.IsValidDropTarget(approach.RequiredCardType);
        if (isCompatible)
        {
            SkillCard card = DragDropService.SelectedCard;
            DragDropService.Reset();
        }
        await SelectApproach(action, approach);
    }

    public async Task HandleActionSelection(UserActionOption action)
    {
        showTooltip = false;

        await OnActionSelected.InvokeAsync(action);

        selectedAction = null;
    }

    public void ActivateHighlightMode(SkillCategories cardType)
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

    public bool IsValidCardForApproach(SkillCategories requiredCardType)
    {
        bool isValidTarget = DragDropService.IsValidDropTarget(requiredCardType);
        return true;
    }

    public string GetCardTypeClass(SkillCategories type)
    {
        return type switch
        {
            SkillCategories.Physical => "physical",
            SkillCategories.Intellectual => "intellectual",
            SkillCategories.Social => "social",
            _ => ""
        };
    }

    public void ToggleActionApproaches(UserActionOption action)
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

    public async Task SelectApproach(UserActionOption action, ApproachDefinition approach)
    {
        showTooltip = false;

        //if (!GameWorld.GetPlayer()State.HasAvailableCard(approach.RequiredCardType))
        //{
        //    return;
        //}

        UserActionOption actionWithApproach = action with { ApproachId = approach.Id };

        await OnActionSelected.InvokeAsync(actionWithApproach);

        // Reset selection
        selectedAction = null;
    }

    public void HandleShowApproachTooltip(UserActionOption action, ApproachDefinition approach, MouseEventArgs e)
    {
        selectedAction = action;
        hoveredApproach = approach;
        mouseX = e.ClientX;
        mouseY = e.ClientY;
        showTooltip = true;
    }

    public void HandleHideTooltip()
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

    public List<NPC> GetAvailableNPCs()
    {
        TimeBlocks currentTime = GameWorld.TimeManager.GetCurrentTimeBlock();
        return NPCRepository.GetNPCsForLocationAndTime(CurrentLocation.Id, currentTime);
    }

    public List<NPC> GetAvailableNPCsForSpot(LocationSpot spot)
    {
        TimeBlocks currentTime = GameWorld.TimeManager.GetCurrentTimeBlock();
        return NPCRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
    }

}