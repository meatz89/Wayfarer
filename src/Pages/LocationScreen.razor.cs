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
    [Inject] private TimeManager TimeManager { get; set; }
    [Inject] private NavigationCoordinator NavigationCoordinator { get; set; }
    [Inject] private ObligationQueueManager QueueManager { get; set; }

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
        // Create conversation context with the NPC through GameFacade
        ConversationContext conversationContext = await GameFacade.CreateConversationContext(npc.Id, interaction.ConversationType);

        if (conversationContext != null && conversationContext.IsValid)
        {
            // ConversationContext is now passed directly to ConversationScreen
            // No need to store state in NavigationCoordinator
            
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
        return GameFacade.GetCurrentLocation();
    }

    private LocationSpot GetCurrentSpot()
    {
        return GameFacade.GetCurrentLocationSpot();
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
        Console.WriteLine($"[LocationScreen] Clicked observation with ID: '{observation.Id}'");
        Console.WriteLine($"[LocationScreen] Text: '{observation.Text}'");
        Console.WriteLine($"[LocationScreen] Relevance: '{observation.Relevance}'");
        Console.WriteLine($"[LocationScreen] AttentionCost: {observation.AttentionCost}");

        // Use the observation ID directly from the ViewModel
        if (string.IsNullOrEmpty(observation.Id))
        {
            Console.WriteLine($"[LocationScreen] ERROR: Observation has no ID! Text: '{observation.Text}'");
            return;
        }

        Console.WriteLine($"[LocationScreen] Taking observation: {observation.Id}");
        bool success = await GameFacade.TakeObservationAsync(observation.Id);
        
        if (success)
        {
            await LoadLocation(); // Refresh the location to update observation status
            await HandleActionExecuted();
        }
    }
    
    private async Task OpenObligationQueue()
    {
        Console.WriteLine("[LocationScreen] OpenObligationQueue called");
        // Navigate to the letter queue screen
        await NavigationCoordinator.NavigateToAsync(CurrentViews.ObligationQueueScreen);
    }
    
    // Obligation display helpers
    private class ObligationItem
    {
        public string Action { get; set; }
        public string Target { get; set; }
        public string Time { get; set; }
        public int DeadlineInMinutes { get; set; }
    }
    
    private bool HasActiveObligations()
    {
        var deliveries = QueueManager?.GetPlayerQueue();
        var meetings = QueueManager?.GetActiveMeetingObligations();
        return (deliveries?.Any(d => d?.DeadlineInMinutes > 0) == true) ||
               (meetings?.Any(m => m?.DeadlineInMinutes > 0) == true);
    }
    
    private List<ObligationItem> GetTopObligations()
    {
        var obligations = new List<ObligationItem>();
        
        // Add delivery obligations
        var deliveries = QueueManager?.GetPlayerQueue();
        if (deliveries != null)
        {
            foreach (var d in deliveries.Where(d => d?.DeadlineInMinutes > 0).Take(2))
            {
                var npc = NPCRepository.GetByName(d.RecipientName);
                var location = npc != null ? LocationRepository.GetLocation(npc.Location) : null;
                obligations.Add(new ObligationItem
                {
                    Action = "Deliver",
                    Target = location?.Name ?? d.RecipientName,
                    Time = FormatDeadlineTime(d.DeadlineInMinutes),
                    DeadlineInMinutes = d.DeadlineInMinutes
                });
            }
        }
        
        // Add meeting obligations
        var meetings = QueueManager?.GetActiveMeetingObligations();
        if (meetings != null)
        {
            foreach (var m in meetings.Where(m => m?.DeadlineInMinutes > 0).Take(1))
            {
                var npc = NPCRepository.GetByName(m.RequesterName);
                var location = npc != null ? LocationRepository.GetLocation(npc.Location) : null;
                obligations.Add(new ObligationItem
                {
                    Action = "Meet",
                    Target = $"{m.RequesterName} • {location?.Name ?? "Unknown"}",
                    Time = FormatDeadlineTime(m.DeadlineInMinutes),
                    DeadlineInMinutes = m.DeadlineInMinutes
                });
            }
        }
        
        // Sort by deadline and take top 3
        return obligations
            .OrderBy(o => o.DeadlineInMinutes)
            .Take(3)
            .ToList();
    }
    
    private string GetObligationClass(ObligationItem item)
    {
        if (item.DeadlineInMinutes <= 120) return "critical"; // 2 hours
        if (item.DeadlineInMinutes <= 480) return "urgent"; // 8 hours
        return "normal";
    }
    
    private string FormatDeadlineTime(int minutes)
    {
        if (minutes <= 0) return "EXPIRED";
        if (minutes < 60) return $"{minutes}m";
        if (minutes < 120) return $"1h {minutes - 60}m";
        if (minutes < 1440) return $"{minutes / 60}h";
        
        int days = minutes / 1440;
        return days == 1 ? "Tomorrow" : $"{days} days";
    }
    
    private async Task NavigateToLetterQueue()
    {
        if (OnNavigate.HasDelegate)
        {
            await OnNavigate.InvokeAsync(CurrentViews.ObligationQueueScreen);
        }
    }
}