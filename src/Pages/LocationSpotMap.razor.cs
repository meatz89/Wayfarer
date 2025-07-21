using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
namespace Wayfarer.Pages;

public class LocationSpotMapBase : ComponentBase
{
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public LocationSystem LocationSystem { get; set; }
    [Inject] public NPCRepository NPCRepository { get; set; }
    [Inject] public NPCLetterOfferService NPCLetterOfferService { get; set; }
    [Inject] public NetworkReferralService NetworkReferralService { get; set; }
    [Inject] public ConnectionTokenManager TokenManager { get; set; }
    [Inject] public MessageSystem MessageSystem { get; set; }

    [Parameter] public Location CurrentLocation { get; set; }
    [Parameter] public LocationSpot CurrentSpot { get; set; }
    [Parameter] public EventCallback<LocationSpot> OnSpotSelected { get; set; }
    
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    // NPC expansion state management
    private Dictionary<string, bool> _npcExpandedStates = new Dictionary<string, bool>();

    protected override void OnInitialized()
    {
        // Initialize component
    }

    // Action system removed - use LocationActionManager for location actions

    public void Dispose()
    {
        // Component disposal
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
        return NPCRepository.GetAllNPCs()
            .Where(npc => npc.SpotId == spot.SpotID)
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

    /// <summary>
    /// Get expanded state for a specific NPC
    /// </summary>
    public bool GetNPCExpandedState(string npcId)
    {
        return _npcExpandedStates.ContainsKey(npcId) && _npcExpandedStates[npcId];
    }

    /// <summary>
    /// Toggle expansion state for a specific NPC
    /// </summary>
    public void ToggleNPCExpansion(string npcId)
    {
        if (_npcExpandedStates.ContainsKey(npcId))
        {
            _npcExpandedStates[npcId] = !_npcExpandedStates[npcId];
        }
        else
        {
            _npcExpandedStates[npcId] = true;
        }
        StateHasChanged();
    }

    /// <summary>
    /// Get service icon for UI display
    /// </summary>
    public string GetServiceIcon(ServiceTypes service)
    {
        return service switch
        {
            ServiceTypes.Rest => "🛌",
            ServiceTypes.Trade => "🛒",
            ServiceTypes.Healing => "❤️",
            ServiceTypes.Information => "📖",
            ServiceTypes.Training => "⚔️",
            ServiceTypes.EquipmentRepair => "🔨",
            ServiceTypes.FoodProduction => "🍲",
            _ => "⚙️"
        };
    }

    /// <summary>
    /// Check if NPC has letter offers based on relationship level
    /// </summary>
    public bool HasLetterOffers(NPC npc)
    {
        return NPCLetterOfferService.ShouldNPCOfferLetters(npc.ID);
    }

    /// <summary>
    /// Get letter offers from an NPC
    /// </summary>
    public List<LetterOffer> GetLetterOffers(NPC npc)
    {
        return NPCLetterOfferService.GenerateNPCLetterOffers(npc.ID);
    }

    /// <summary>
    /// Accept a letter offer from an NPC
    /// </summary>
    public async Task AcceptLetterOffer(string npcId, string offerId)
    {
        bool success = NPCLetterOfferService.AcceptNPCLetterOffer(npcId, offerId);
        if (success)
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Get token icon for UI display
    /// </summary>
    public string GetTokenIcon(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => "💚",
            ConnectionType.Trade => "💙",
            ConnectionType.Noble => "💜",
            ConnectionType.Common => "🤎",
            ConnectionType.Shadow => "🖤",
            _ => "⚪"
        };
    }
    
    /// <summary>
    /// Check if player can request referral from NPC
    /// </summary>
    public bool CanRequestReferral(NPC npc)
    {
        // Need at least 1 token with the NPC to ask for referrals
        return TokenManager.HasTokensWithNPC(npc.ID, 1);
    }
    
    /// <summary>
    /// Request network referral from NPC
    /// </summary>
    public async Task RequestReferral(string npcId)
    {
        var referral = NetworkReferralService.RequestReferral(npcId);
        if (referral != null)
        {
            MessageSystem.AddSystemMessage($"Received introduction to {referral.TargetNPCName}!", SystemMessageTypes.Success);
            StateHasChanged();
        }
        else
        {
            MessageSystem.AddSystemMessage($"Cannot get referral: insufficient tokens or no contacts available", SystemMessageTypes.Warning);
        }
    }

}