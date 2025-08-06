using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
    }
    
    private Location GetCurrentLocation()
    {
        var (location, _) = GameFacade.GetCurrentLocation();
        return location;
    }
    
    private LocationSpot GetCurrentSpot()
    {
        var (_, spot) = GameFacade.GetCurrentLocation();
        return spot;
    }
    
    private List<NPC> GetNPCsAtCurrentSpot()
    {
        var spot = GetCurrentSpot();
        if (spot == null) return new List<NPC>();
        
        // Get NPCs at the current spot and time
        var currentTime = TimeManager.GetCurrentTimeBlock();
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
        var moveIntent = new MoveIntent(spot.SpotID);
        var success = await GameFacade.ExecuteIntent(moveIntent);
        
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
        var tokenBalance = GameFacade.GetTokensWithNPC(npcId);
        var result = new Dictionary<ConnectionType, int>();
        
        if (tokenBalance?.Balances != null)
        {
            foreach (var balance in tokenBalance.Balances)
            {
                result[balance.TokenType] = balance.Amount;
            }
        }
        
        return result;
    }
    
    private bool CanRest()
    {
        // Check if current location has rest options
        var location = GetCurrentLocation();
        return location?.RestOptions?.Any() ?? false;
    }
    
    private bool HasMarket()
    {
        // Check if current location has a market
        var location = GetCurrentLocation();
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