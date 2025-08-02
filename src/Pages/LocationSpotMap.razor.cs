using Microsoft.AspNetCore.Components;
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
    [Inject] public ITimeManager TimeManager { get; set; }
    [Inject] public NarrativeManager NarrativeManager { get; set; }

    [Parameter] public Location CurrentLocation { get; set; }
    [Parameter] public LocationSpot CurrentSpot { get; set; }
    [Parameter] public EventCallback<LocationSpot> OnSpotSelected { get; set; }
    [Parameter] public EventCallback OnActionExecuted { get; set; }

    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    // Selected NPC for interaction
    public NPC SelectedNPC { get; set; }
    [Inject] public DebugLogger DebugLogger { get; set; }

    protected override void OnInitialized()
    {
        // Initialize component
    }

    // Action system removed - use LocationActionManager for location actions

    public void SelectNPC(NPC npc)
    {
        SelectedNPC = npc;
        StateHasChanged();
    }

    public void DeselectNPC()
    {
        SelectedNPC = null;
        StateHasChanged();
    }

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
        TimeBlocks currentTime = TimeManager.GetCurrentTimeBlock();
        return NPCRepository.GetNPCsForLocationAndTime(CurrentLocation.Id, currentTime);
    }

    public List<NPC> GetAvailableNPCsForSpot(LocationSpot spot)
    {
        TimeBlocks currentTime = TimeManager.GetCurrentTimeBlock();
        return NPCRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
    }

    public List<NPC> GetAvailableNPCsAtCurrentSpot()
    {
        TimeBlocks currentTime = TimeManager.GetCurrentTimeBlock();
        return NPCRepository.GetNPCsForLocationSpotAndTime(CurrentSpot.SpotID, currentTime);
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

    public List<NPC> GetAllNPCsAtCurrentSpot()
    {
        var npcs = NPCRepository.GetAllNPCs()
            .Where(npc => npc.SpotId == CurrentSpot.SpotID)
            .ToList();
        
        // Filter NPCs based on narrative visibility
        if (NarrativeManager != null && NarrativeManager.HasActiveNarrative())
        {
            npcs = npcs.Where(npc => NarrativeManager.IsNPCVisible(npc.ID)).ToList();
        }
        
        return npcs;
    }

    /// <summary>

    /// <summary>
    /// Get next time block when NPC will be available
    /// </summary>
    public string GetNextAvailableTime(NPC npc)
    {
        TimeBlocks currentTime = TimeManager.GetCurrentTimeBlock();

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
        TimeBlocks currentTime = TimeManager.GetCurrentTimeBlock();
        return npc.IsAvailable(currentTime);
    }

    /// <summary>
    /// Get expanded state for a specific NPC
    /// </summary>

    /// <summary>
    /// Toggle expansion state for a specific NPC
    /// </summary>

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
            ConnectionType.Commerce => "💙",
            ConnectionType.Status => "💜",
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
        NetworkReferral referral = NetworkReferralService.RequestReferral(npcId);
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