using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages;

public partial class LocationScreen : ComponentBase
{
    [Inject] private GameFacade GameFacade { get; set; }
    [Inject] private NPCRepository NPCRepository { get; set; }
    [Inject] private LocationRepository LocationRepository { get; set; }
    [Inject] private ITimeManager TimeManager { get; set; }
    [Inject] private NavigationCoordinator NavigationCoordinator { get; set; }

    [Parameter] public EventCallback OnActionExecuted { get; set; }
    [Parameter] public EventCallback<CurrentViews> OnNavigate { get; set; }

    // Current location and spot
    private Location CurrentLocation => GetCurrentLocation();
    private LocationSpot CurrentSpot => GetCurrentSpot();

    // Selected NPC for interaction
    private NPC SelectedNPC { get; set; }

    // NPCs at current spot
    private List<NPC> NPCsAtCurrentSpot => GetNPCsAtCurrentSpot();

    // View Model
    private LocationScreenViewModel Model { get; set; }
    
    // Modal state
    private bool ShowTravelModal { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadLocation();
    }

    private async Task LoadLocation()
    {
        Model = GameFacade.GetLocationScreen();
        StateHasChanged();
    }

    private string GetStateClass(string stateName)
    {
        return stateName?.ToLower() ?? "";
    }

    private async Task ExecuteAction(LocationActionViewModel action)
    {
        // Handle special action types
        if (action.ActionType == "wait")
        {
            // Execute wait action to advance time
            await GameFacade.ExecuteWaitAction();
        }
        else if (action.ActionType == "travel")
        {
            // Open travel modal instead of navigating
            OpenTravelModal();
            return;
        }
        else if (action.ActionType == "rest")
        {
            // Execute rest action - this should handle payment and stamina recovery
            // For now, use the basic 1-hour rest (will be extended for inn rooms later)
            await GameFacade.ExecuteRestAction(action.ActionType, action.Cost);
        }
        else
        {
            // Execute normal action through GameFacade
            // TODO: Implement other action types as needed
        }

        await HandleActionExecuted();
        await LoadLocation();
    }

    private async Task StartInteraction(NPCPresenceViewModel npc, InteractionOptionViewModel interaction)
    {
        // Start the conversation with the NPC through GameFacade
        ConversationViewModel conversationStarted = await GameFacade.StartConversationAsync(npc.Id, interaction.ConversationType);

        if (conversationStarted != null)
        {
            // Store the NPC ID and conversation type in NavigationCoordinator for the conversation screen
            NavigationCoordinator.SetConversationNpcId(npc.Id);
            NavigationCoordinator.SetConversationType(interaction.ConversationType);
            
            // Only navigate if conversation was successfully started
            await OnNavigate.InvokeAsync(CurrentViews.ConversationScreen);
        }
        else
        {
            Console.WriteLine($"[LocationScreen] Failed to start conversation with NPC {npc.Id}");
        }
    }

    private async Task TravelTo(RouteOptionViewModel route)
    {
        Console.WriteLine($"[LocationScreen.TravelTo] Starting travel to {route.Destination}");
        
        // Routes represent actual inter-location travel paths
        if (string.IsNullOrEmpty(route.RouteId))
        {
            Console.WriteLine($"[LocationScreen.TravelTo] ERROR: Route ID missing for travel to {route.Destination}");
            return;
        }

        Console.WriteLine($"[LocationScreen.TravelTo] Using RouteId: {route.RouteId}");
        
        // Use TravelIntent for inter-location movement
        TravelIntent travelIntent = new TravelIntent(route.RouteId);
        bool success = await GameFacade.ExecuteIntent(travelIntent);

        if (success)
        {
            Console.WriteLine($"[LocationScreen.TravelTo] Successfully traveled to {route.Destination}");
            // Reload location to show new location
            await LoadLocation();
        }
        else
        {
            Console.WriteLine($"[LocationScreen.TravelTo] Failed to travel to {route.Destination}");
        }

        await HandleActionExecuted();
    }
    
    private async Task NavigateToArea(AreaWithinLocationViewModel area)
    {
        if (!area.IsCurrent && !string.IsNullOrEmpty(area.SpotId))
        {
            // Navigate to area within location (no travel time)
            MoveIntent moveIntent = new MoveIntent(area.SpotId);
            bool success = await GameFacade.ExecuteIntent(moveIntent);
            
            if (success)
            {
                Console.WriteLine($"[LocationScreen] Successfully moved to {area.Name}");
                await LoadLocation();
            }
            else
            {
                Console.WriteLine($"[LocationScreen] Failed to move to {area.Name}");
            }
        }
    }

    private Location GetCurrentLocation()
    {
        (Location location, LocationSpot _) = GameFacade.GetCurrentLocation();
        return location;
    }

    private LocationSpot GetCurrentSpot()
    {
        (Location _, LocationSpot spot) = GameFacade.GetCurrentLocation();
        return spot;
    }

    private List<NPC> GetNPCsAtCurrentSpot()
    {
        LocationSpot spot = GetCurrentSpot();
        if (spot == null) return new List<NPC>();

        // Get NPCs at the current spot and time
        TimeBlocks currentTime = TimeManager.GetCurrentTimeBlock();
        return NPCRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
    }
    private async Task HandleActionExecuted()
    {
        // Refresh UI after action
        StateHasChanged();

        // Notify parent
        if (OnActionExecuted.HasDelegate)
        {
            await OnActionExecuted.InvokeAsync();
        }
    }
    
    // Modal handling methods
    private void OpenTravelModal()
    {
        ShowTravelModal = true;
        StateHasChanged();
    }

    private void CloseTravelModal()
    {
        ShowTravelModal = false;
        StateHasChanged();
    }

    private async Task HandleRouteSelected(RouteOptionViewModel route)
    {
        // Close modal and execute travel
        ShowTravelModal = false;
        await TravelTo(route);
    }

    // Resource display methods for unified header
    protected int GetPlayerCoins() => GameFacade?.GetPlayer()?.Coins ?? 0;
    protected int GetPlayerHealth() => GameFacade?.GetPlayer()?.Health ?? 0;
    protected int GetPlayerHunger() => GameFacade?.GetPlayer()?.Food ?? 0;
    
    protected int GetPlayerAttention()
    {
        var attentionState = GameFacade?.GetCurrentAttentionState();
        return attentionState?.Current ?? 0;
    }
    
    protected int GetMaxAttention()
    {
        var attentionState = GameFacade?.GetCurrentAttentionState();
        return attentionState?.Max ?? 10;
    }
    
    protected string GetCurrentTimeDisplay() => Model?.CurrentTime ?? "";
    protected string GetCurrentTimeBlock() => TimeManager?.GetCurrentTimeBlock().ToString() ?? "";
    protected string GetUrgentDeadline() => Model?.DeadlineTimer;

    private async Task TakeObservation(ObservationViewModel observation)
    {
        if (observation.IsObserved)
        {
            return; // Already taken
        }

        // Debug the observation
        Console.WriteLine($"[LocationScreen] Clicked observation with text: '{observation.Text}'");
        Console.WriteLine($"[LocationScreen] Relevance: '{observation.Relevance}'");
        Console.WriteLine($"[LocationScreen] AttentionCost: {observation.AttentionCost}");

        // Use the observation ID from the text - we need to parse it or store it properly
        // For now, use the text as a simple identifier
        string observationId = ExtractObservationId(observation.Text);
        
        if (string.IsNullOrEmpty(observationId))
        {
            Console.WriteLine($"[LocationScreen] Could not determine observation ID from text: '{observation.Text}'");
            Console.WriteLine("[LocationScreen] Available mappings:");
            foreach (var mapping in GetAllObservationMappings())
            {
                Console.WriteLine($"  '{mapping.Key}' -> '{mapping.Value}'");
            }
            return;
        }

        Console.WriteLine($"[LocationScreen] Taking observation: {observationId}");
        bool success = await GameFacade.TakeObservationAsync(observationId);
        
        if (success)
        {
            await LoadLocation(); // Refresh the location to update observation status
            await HandleActionExecuted();
        }
    }

    /// <summary>
    /// Extract observation ID from observation text
    /// This is a temporary solution - ideally the observation ID should be stored in the view model
    /// </summary>
    private string ExtractObservationId(string observationText)
    {
        // Map common observation texts to their IDs from observations.json
        var textToIdMap = new Dictionary<string, string>
        {
            { "Notice guard checkpoint ahead", "guards_north" },
            { "Eavesdrop on merchant negotiations", "merchant_negotiations" },
            { "Guards blocking north road", "guards_blocking" },
            { "Notice board shows Noble District schedule", "noble_schedule" },
            { "Elena's visible distress", "elena_distress" },
            { "Lord preparing to leave", "lord_preparing" },
            { "Guard patrol patterns", "guard_positions" }
        };

        return textToIdMap.GetValueOrDefault(observationText?.Trim(), null);
    }

    /// <summary>
    /// Get all observation mappings for debugging
    /// </summary>
    private Dictionary<string, string> GetAllObservationMappings()
    {
        return new Dictionary<string, string>
        {
            { "Notice guard checkpoint ahead", "guards_north" },
            { "Eavesdrop on merchant negotiations", "merchant_negotiations" },
            { "Guards blocking north road", "guards_blocking" },
            { "Notice board shows Noble District schedule", "noble_schedule" },
            { "Elena's visible distress", "elena_distress" },
            { "Lord preparing to leave", "lord_preparing" },
            { "Guard patrol patterns", "guard_positions" }
        };
    }
}