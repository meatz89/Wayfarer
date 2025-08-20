using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.ViewModels;

namespace Wayfarer.Pages;

public partial class LocationScreen : ComponentBase
{
    [Inject] private GameFacade GameFacade { get; set; }
    [Inject] private NPCRepository NPCRepository { get; set; }
    [Inject] private LocationRepository LocationRepository { get; set; }
    [Inject] private ITimeManager TimeManager { get; set; }

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

    protected override async Task OnInitializedAsync()
    {
        await LoadLocation();
    }

    private async Task LoadLocation()
    {
        Model = GameFacade.GetLocationScreen();
        StateHasChanged();
    }

    private async Task ExecuteAction(Wayfarer.ViewModels.LocationActionViewModel action)
    {
        // Handle special action types
        if (action.ActionType == "wait")
        {
            // Execute wait action to advance time
            await GameFacade.ExecuteWaitAction();
        }
        else if (action.ActionType == "travel")
        {
            // Navigate to travel screen
            await NavigateToTravel();
            return; // Don't reload location since we're navigating away
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
        ConversationViewModel conversationStarted = await GameFacade.StartConversationAsync(npc.Id);

        if (conversationStarted != null)
        {
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
        // Phase 2: Routes now represent spots within current location, not inter-location travel
        if (string.IsNullOrEmpty(route.RouteId))
        {
            Console.WriteLine($"[LocationScreen] Route ID missing for movement to {route.Destination}");
            return;
        }

        // Route.RouteId now contains SpotID for intra-location movement
        // Create move intent and execute through GameFacade (free movement)
        MoveIntent moveIntent = new MoveIntent(route.RouteId);
        bool success = await GameFacade.ExecuteIntent(moveIntent);

        if (success)
        {
            Console.WriteLine($"[LocationScreen] Successfully moved to {route.Destination}");
            // Reload location to show new spot perspective
            await LoadLocation();
        }
        else
        {
            Console.WriteLine($"[LocationScreen] Failed to move to {route.Destination}");
        }

        await HandleActionExecuted();
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

    private bool IsCurrentSpot(LocationSpot spot)
    {
        return spot.SpotID == CurrentSpot?.SpotID;
    }

    private async Task MoveToSpot(LocationSpot spot)
    {
        if (spot.IsClosed || IsCurrentSpot(spot)) return;

        // Use MoveIntent to move to the spot
        MoveIntent moveIntent = new MoveIntent(spot.SpotID);
        bool success = await GameFacade.ExecuteIntent(moveIntent);

        if (success)
        {
            // Clear selected NPC when moving
            SelectedNPC = null;
            StateHasChanged();
        }
    }

    private void SelectNPC(NPC npc)
    {
        SelectedNPC = npc;
        StateHasChanged();
    }

    private Dictionary<ConnectionType, int> GetTokensWithNPC(string npcId)
    {
        NPCTokenBalance tokenBalance = GameFacade.GetTokensWithNPC(npcId);
        Dictionary<ConnectionType, int> result = new Dictionary<ConnectionType, int>();

        if (tokenBalance?.Balances != null)
        {
            foreach (TokenBalance balance in tokenBalance.Balances)
            {
                result[balance.TokenType] = balance.Amount;
            }
        }

        return result;
    }

    private bool CanRest()
    {
        // Check if current location has rest options
        Location location = GetCurrentLocation();
        return location?.RestOptions?.Any() ?? false;
    }

    private bool HasMarket()
    {
        // Check if current location has a market
        Location location = GetCurrentLocation();
        return location?.AvailableServices?.Contains(ServiceTypes.Trade) ?? false;
    }

    private async Task NavigateToRest()
    {
        if (OnNavigate.HasDelegate)
        {
            await OnNavigate.InvokeAsync(CurrentViews.RestScreen);
        }
    }

    private async Task NavigateToMarket()
    {
        if (OnNavigate.HasDelegate)
        {
            await OnNavigate.InvokeAsync(CurrentViews.MarketScreen);
        }
    }

    private async Task NavigateToTravel()
    {
        if (OnNavigate.HasDelegate)
        {
            await OnNavigate.InvokeAsync(CurrentViews.TravelScreen);
        }
    }

    private async Task NavigateToQueue()
    {
        if (OnNavigate.HasDelegate)
        {
            await OnNavigate.InvokeAsync(CurrentViews.LetterQueueScreen);
        }
    }

    private string GetQueueDisplay()
    {
        try
        {
            LetterQueueViewModel queueVM = GameFacade.GetLetterQueue();
            if (queueVM?.Status != null)
            {
                // Show count/max and urgent indicator if needed
                bool hasUrgent = queueVM.QueueSlots?.Any(s => s.IsOccupied && s.DeliveryObligation?.DeadlineInHours <= 6) ?? false;
                string display = $"{queueVM.Status.LetterCount}/8";
                if (hasUrgent)
                {
                    return $"⚠️ {display}";
                }
                return display;
            }
            return "0/8";
        }
        catch
        {
            return "QUEUE";
        }
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
}