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

    /// <summary>
    /// Get all NPCs assigned to a spot regardless of current availability
    /// </summary>
    public List<NPC> GetAllNPCsForSpot(LocationSpot spot)
    {
        return NPCRepository.GetNPCsForLocation(spot.LocationId)
            .Where(npc => npc.Location == spot.LocationId)
            .ToList();
    }

    /// <summary>
    /// Get human-readable description of NPC availability schedule
    /// </summary>
    public string GetNPCScheduleDescription(Schedule schedule)
    {
        return schedule switch
        {
            Schedule.Always => "Always available",
            Schedule.Market_Hours => "Morning, Afternoon",
            Schedule.Workshop_Hours => "Dawn, Morning, Afternoon",
            Schedule.Library_Hours => "Morning, Afternoon",
            Schedule.Business_Hours => "Morning, Afternoon",
            Schedule.Morning_Evening => "Morning, Evening",
            Schedule.Morning_Afternoon => "Morning, Afternoon",
            Schedule.Afternoon_Evening => "Afternoon, Evening",
            Schedule.Evening_Only => "Evening only",
            Schedule.Morning_Only => "Morning only",
            Schedule.Afternoon_Only => "Afternoon only",
            Schedule.Evening_Night => "Evening, Night",
            Schedule.Dawn_Only => "Dawn only",
            Schedule.Night_Only => "Night only",
            _ => "Unknown schedule"
        };
    }

    /// <summary>
    /// Get next time block when NPC will be available
    /// </summary>
    public string GetNextAvailableTime(NPC npc)
    {
        TimeBlocks currentTime = GameWorld.TimeManager.GetCurrentTimeBlock();
        
        if (npc.IsAvailable(currentTime))
        {
            return "Available now";
        }

        // Check upcoming time blocks in order
        List<TimeBlocks> timeBlocks = new List<TimeBlocks> 
        { 
            TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, 
            TimeBlocks.Evening, TimeBlocks.Night 
        };

        // Start checking from the next time block
        int currentIndex = timeBlocks.IndexOf(currentTime);
        for (int i = 1; i <= timeBlocks.Count; i++)
        {
            int nextIndex = (currentIndex + i) % timeBlocks.Count;
            TimeBlocks nextTime = timeBlocks[nextIndex];
            
            if (npc.IsAvailable(nextTime))
            {
                return $"Next available: {nextTime.ToString().Replace('_', ' ')}";
            }
        }

        return "Never available";
    }

    /// <summary>
    /// Check if NPC is currently available at this time
    /// </summary>
    public bool IsNPCCurrentlyAvailable(NPC npc)
    {
        TimeBlocks currentTime = GameWorld.TimeManager.GetCurrentTimeBlock();
        return npc.IsAvailable(currentTime);
    }

}