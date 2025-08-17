using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles the special mechanics for each type of special letter
/// </summary>
public class SpecialLetterHandler
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly InformationDiscoveryManager _informationManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly EndorsementManager _endorsementManager;
    private readonly RouteRepository _routeRepository;

    public SpecialLetterHandler(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        InformationDiscoveryManager informationManager,
        TokenMechanicsManager tokenManager,
        EndorsementManager endorsementManager = null,
        RouteRepository routeRepository = null)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _informationManager = informationManager;
        _tokenManager = tokenManager;
        _endorsementManager = endorsementManager ?? new EndorsementManager(gameWorld, messageSystem);
        _routeRepository = routeRepository ?? new RouteRepository(gameWorld, new ItemRepository(gameWorld));
    }

    /// <summary>
    /// Process special letter effects when delivered
    /// </summary>
    public void ProcessSpecialLetterDelivery(Letter letter)
    {
        if (letter.SpecialType == LetterSpecialType.None) return;

        switch (letter.SpecialType)
        {
            case LetterSpecialType.Introduction:
                ProcessIntroductionLetter(letter);
                break;

            case LetterSpecialType.AccessPermit:
                ProcessAccessPermitLetter(letter);
                break;

            case LetterSpecialType.Endorsement:
                ProcessEndorsementLetter(letter);
                break;

            case LetterSpecialType.Information:
                ProcessInformationLetter(letter);
                break;
        }

        // All special letters grant bonus tokens of their type
        int bonusTokens = CalculateSpecialLetterTokenBonus(letter);
        if (bonusTokens > 0)
        {
            _tokenManager.AddTokensToNPC(letter.TokenType, bonusTokens, letter.RecipientId);
            _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
            {
                EventType = SpecialLetterEventType.SpecialLetterTokenBonus,
                LetterType = letter.SpecialType,
                TokenType = letter.TokenType,
                TokenAmount = bonusTokens,
                SenderName = letter.SenderName,
                RecipientName = letter.RecipientName,
                Severity = NarrativeSeverity.Success
            });
        }
    }

    /// <summary>
    /// Trust - Introduction letters unlock new NPCs
    /// </summary>
    private void ProcessIntroductionLetter(Letter letter)
    {
        if (string.IsNullOrEmpty(letter.UnlocksNPCId))
        {
            _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
            {
                EventType = SpecialLetterEventType.IntroductionLetterIncomplete,
                LetterType = LetterSpecialType.Introduction,
                SenderName = letter.SenderName,
                RecipientName = letter.RecipientName,
                Severity = NarrativeSeverity.Warning
            });
            return;
        }

        NPC npc = _npcRepository.GetById(letter.UnlocksNPCId);
        if (npc == null)
        {
            _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
            {
                EventType = SpecialLetterEventType.IntroductionTargetNotFound,
                LetterType = LetterSpecialType.Introduction,
                TargetNPCId = letter.UnlocksNPCId,
                SenderName = letter.SenderName,
                RecipientName = letter.RecipientName,
                Severity = NarrativeSeverity.Warning
            });
            return;
        }

        // Mark NPC as discovered/unlocked
        Player player = _gameWorld.GetPlayer();
        player.AddMemory(
            $"npc_introduced_{npc.ID}",
            $"Introduced to {npc.Name} by {letter.SenderName}",
            _gameWorld.CurrentDay,
            (int)letter.Tier
        );

        // Grant initial trust tokens with the new NPC
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 1, npc.ID);

        _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
        {
            EventType = SpecialLetterEventType.NPCIntroduced,
            LetterType = LetterSpecialType.Introduction,
            TargetNPCId = npc.ID,
            SenderName = letter.SenderName,
            RecipientName = letter.RecipientName,
            TokenType = ConnectionType.Trust,
            TokenAmount = 1,
            Severity = NarrativeSeverity.Success
        });

        // Discover any information this NPC might share with new contacts
        _informationManager.TryDiscoverFromNPC(npc.ID, ConnectionType.Trust);
    }

    /// <summary>
    /// Commerce - Access Permit letters unlock new locations and routes
    /// CONTENT EFFICIENT: Works with Transport NPCs to unlock routes
    /// </summary>
    private void ProcessAccessPermitLetter(Letter letter)
    {
        Player player = _gameWorld.GetPlayer();

        // Check if delivered to a Transport NPC
        NPC recipient = _npcRepository.GetById(letter.RecipientId);
        if (recipient != null && recipient.ProvidedServices.Contains(ServiceTypes.Transport))
        {
            // Transport NPC - unlock routes they control
            ProcessTransportPermit(letter, recipient);
            return;
        }

        // Original location unlocking logic
        if (!string.IsNullOrEmpty(letter.UnlocksLocationId))
        {
            Location location = _locationRepository.GetLocation(letter.UnlocksLocationId);
            if (location == null)
            {
                _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
                {
                    EventType = SpecialLetterEventType.AccessTargetNotFound,
                    LetterType = LetterSpecialType.AccessPermit,
                    TargetLocationId = letter.UnlocksLocationId,
                    SenderName = letter.SenderName,
                    RecipientName = letter.RecipientName,
                    Severity = NarrativeSeverity.Warning
                });
                return;
            }

            // Mark location as accessible
            player.AddMemory(
                $"location_permit_{location.Id}",
                $"Access permit to {location.Name} granted by {letter.SenderName}",
                _gameWorld.CurrentDay,
                (int)letter.Tier
            );

            _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
            {
                EventType = SpecialLetterEventType.LocationAccessGranted,
                LetterType = LetterSpecialType.AccessPermit,
                TargetLocationId = location.Id,
                SenderName = letter.SenderName,
                RecipientName = letter.RecipientName,
                Severity = NarrativeSeverity.Success
            });

            // Discover information about this location
            _informationManager.DiscoverFromLocationVisit(location.Id);
        }
        else
        {
            // Generic permit - unlock a route from current location
            UnlockRouteFromCurrentLocation(letter);
        }
    }

    /// <summary>
    /// Process travel permit delivered to Transport NPC
    /// CONTENT EFFICIENT: Unlocks routes without requiring extra NPCs
    /// </summary>
    private void ProcessTransportPermit(Letter letter, NPC transportNPC)
    {
        // Find routes this Transport NPC controls based on their profession and location
        List<RouteOption> controlledRoutes = GetTransportNPCRoutes(transportNPC, _routeRepository);

        // Unlock the first locked route, or mark permit as used for access requirements
        bool routeUnlocked = false;
        foreach (RouteOption route in controlledRoutes)
        {
            // Check if route has permit-based access requirement
            if (route.AccessRequirement != null &&
                route.AccessRequirement.AlternativeLetterUnlock == LetterSpecialType.AccessPermit)
            {
                route.AccessRequirement.HasReceivedPermit = true;
                route.HasPermitUnlock = true; // Also mark tier bypass
                _messageSystem.AddSystemMessage(
                    $"🚢 {transportNPC.Name} accepts your permit. The {route.Name} is now available!",
                    SystemMessageTypes.Success
                );
                routeUnlocked = true;
                break;
            }
            else if (!route.IsDiscovered)
            {
                route.IsDiscovered = true;
                _messageSystem.AddSystemMessage(
                    $"🚢 {transportNPC.Name} stamps your permit. You can now use the {route.Name}!",
                    SystemMessageTypes.Success
                );
                routeUnlocked = true;
                break;
            }
        }

        if (!routeUnlocked)
        {
            // All routes already available - grant commerce tokens as bonus
            _tokenManager.AddTokensToNPC(ConnectionType.Commerce, 5, transportNPC.ID);
            _messageSystem.AddSystemMessage(
                $"{transportNPC.Name} notes you already have access to all routes. Your permit is filed. (+5 Commerce)",
                SystemMessageTypes.Info
            );
        }
    }

    /// <summary>
    /// Get routes controlled by a Transport NPC
    /// CONTENT EFFICIENT: Based on profession and location, not extra data
    /// </summary>
    private List<RouteOption> GetTransportNPCRoutes(NPC transportNPC, RouteRepository routeRepository)
    {
        IEnumerable<RouteOption> allRoutes = routeRepository.GetRoutesFromLocation(transportNPC.Location);

        // Filter based on Transport NPC's profession
        return transportNPC.Profession switch
        {
            Professions.Ferryman => allRoutes.Where(r =>
                r.Method == TravelMethods.Boat ||
                r.TerrainCategories.Contains(TerrainCategory.Requires_Water_Transport)).ToList(),

            Professions.Harbor_Master => allRoutes.Where(r =>
                r.Method == TravelMethods.Boat ||
                r.Method == TravelMethods.Carriage).ToList(),

            _ => allRoutes.Where(r => (int)r.TierRequired <= (int)TierLevel.T2).ToList()
        };
    }

    /// <summary>
    /// Unlock a route from player's current location
    /// </summary>
    private void UnlockRouteFromCurrentLocation(Letter letter)
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = player.GetCurrentLocation(_locationRepository);
        if (currentLocation == null) return;

        List<RouteOption> lockedRoutes = _routeRepository.GetRoutesFromLocation(currentLocation.Id)
            .Where(r => !r.IsDiscovered ||
                   (r.AccessRequirement != null && !r.AccessRequirement.HasReceivedPermit))
            .ToList();

        if (lockedRoutes.Any())
        {
            RouteOption routeToUnlock = lockedRoutes.First();
            if (routeToUnlock.AccessRequirement != null)
            {
                routeToUnlock.AccessRequirement.HasReceivedPermit = true;
            }
            routeToUnlock.IsDiscovered = true;
            routeToUnlock.HasPermitUnlock = true; // Mark tier bypass

            _messageSystem.AddSystemMessage(
                $"🔓 Travel permit accepted! The {routeToUnlock.Name} is now available.",
                SystemMessageTypes.Success
            );
        }
        else
        {
            _messageSystem.AddSystemMessage(
                "All routes from this location are already available.",
                SystemMessageTypes.Info
            );
        }
    }

    /// <summary>
    /// Status - Endorsement letters grant temporary bonuses
    /// </summary>
    private void ProcessEndorsementLetter(Letter letter)
    {
        Player player = _gameWorld.GetPlayer();

        // Track the endorsement for seal conversion
        _endorsementManager.RecordEndorsement(letter);

        // Also record temporary benefits
        int endDate = _gameWorld.CurrentDay + letter.BonusDuration;
        string endorsementKey = $"endorsement_{letter.SenderId}_{(int)letter.Tier}";
        string description = GetEndorsementDescription(letter);

        player.AddMemory(
            endorsementKey,
            description,
            _gameWorld.CurrentDay,
            (int)letter.Tier
        );

        // Store expiration date in a separate memory entry
        player.AddMemory(
            $"{endorsementKey}_expires",
            endDate.ToString(),
            _gameWorld.CurrentDay,
            1 // Low importance
        );

        _messageSystem.AddSystemMessage(
            $"⭐ {letter.SenderName}'s endorsement grants you {description} for {letter.BonusDuration} days!",
            SystemMessageTypes.Success
        );

        // Log the effects that would be applied
        LogEndorsementEffects(letter);
    }

    /// <summary>
    /// Shadow - Information letters trigger discovery events
    /// </summary>
    private void ProcessInformationLetter(Letter letter)
    {
        if (string.IsNullOrEmpty(letter.InformationId))
        {
            // Generic information discovery
            _messageSystem.AddSystemMessage(
                $"🔍 The letter contains valuable information about local activities...",
                SystemMessageTypes.Info
            );

            // Discover random shadow-related information
            DiscoverShadowInformation(letter);
        }
        else
        {
            // Specific information discovery
            bool discovered = _informationManager.DiscoverInformation(letter.InformationId);
            if (!discovered)
            {
                _messageSystem.AddSystemMessage(
                    "You already knew the information in this letter.",
                    SystemMessageTypes.Info
                );
            }
        }
    }

    /// <summary>
    /// Calculate bonus tokens for special letter delivery
    /// </summary>
    private int CalculateSpecialLetterTokenBonus(Letter letter)
    {
        // Base bonus based on tier
        int baseBonus = (int)letter.Tier;

        // Additional bonus for matching token type
        switch (letter.SpecialType)
        {
            case LetterSpecialType.Introduction when letter.TokenType == ConnectionType.Trust:
                return baseBonus + 2;
            case LetterSpecialType.AccessPermit when letter.TokenType == ConnectionType.Commerce:
                return baseBonus + 2;
            case LetterSpecialType.Endorsement when letter.TokenType == ConnectionType.Status:
                return baseBonus + 2;
            case LetterSpecialType.Information when letter.TokenType == ConnectionType.Shadow:
                return baseBonus + 2;
            default:
                return baseBonus;
        }
    }

    /// <summary>
    /// Get description of endorsement effects
    /// </summary>
    private string GetEndorsementDescription(Letter letter)
    {
        return letter.Tier switch
        {
            TierLevel.T1 => "minor social privileges",
            TierLevel.T2 => "improved status letter handling",
            TierLevel.T3 => "significant social advantages",
            _ => "social benefits"
        };
    }

    /// <summary>
    /// Log what effects the endorsement would apply
    /// </summary>
    private void LogEndorsementEffects(Letter letter)
    {
        // This demonstrates what a full implementation would do
        switch (letter.Tier)
        {
            case TierLevel.T1:
                _messageSystem.AddSystemMessage(
                    "  • Status letters will pay +3 bonus coins",
                    SystemMessageTypes.Info
                );
                break;

            case TierLevel.T2:
                _messageSystem.AddSystemMessage(
                    "  • Status letters enter queue at position 3",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "  • Automatic priority for Status obligations",
                    SystemMessageTypes.Info
                );
                break;

            case TierLevel.T3:
                _messageSystem.AddSystemMessage(
                    "  • Status letters enter queue at position 3",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "  • Status letters get +2 days deadline",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "  • Status letters pay bonus based on your Status tokens",
                    SystemMessageTypes.Info
                );
                break;
        }
    }

    /// <summary>
    /// Discover shadow-related information from information letters
    /// </summary>
    private void DiscoverShadowInformation(Letter letter)
    {
        Player player = _gameWorld.GetPlayer();
        Random random = new Random();

        // Types of shadow information based on tier
        string[] tier1Info = new[]
        {
            "Location of a hidden dead drop",
            "Password for a secret meeting",
            "Identity of a local informant"
        };

        string[] tier2Info = new[]
        {
            "Schedule of guard patrols",
            "Location of smuggling routes",
            "Identity of corrupt officials"
        };

        string[] tier3PlusInfo = new[]
        {
            "Blackmail material on a noble",
            "Plans for an upcoming heist",
            "Location of hidden treasure"
        };

        string discoveredInfo = letter.Tier switch
        {
            TierLevel.T1 => tier1Info[random.Next(tier1Info.Length)],
            TierLevel.T2 => tier2Info[random.Next(tier2Info.Length)],
            TierLevel.T3 => tier3PlusInfo[random.Next(tier3PlusInfo.Length)],
            _ => tier3PlusInfo[random.Next(tier3PlusInfo.Length)]
        };

        player.AddMemory(
            $"shadow_info_{letter.Id}",
            discoveredInfo,
            _gameWorld.CurrentDay,
            (int)letter.Tier
        );

        _messageSystem.AddSystemMessage(
            $"🔍 Discovered: {discoveredInfo}",
            SystemMessageTypes.Success
        );
    }

    /// <summary>
    /// Check if player meets requirements to receive a special letter
    /// </summary>
    public bool CanReceiveSpecialLetter(LetterSpecialType type, int tier)
    {
        Player player = _gameWorld.GetPlayer();

        // Basic tier requirements
        if (tier > 1)
        {
            // Need sufficient tokens of the matching type
            ConnectionType requiredType = GetRequiredTokenType(type);
            int totalTokens = _tokenManager.GetTokenCount(requiredType);

            if (totalTokens < (tier - 1) * 3)
            {
                return false;
            }
        }

        // Specific requirements per type
        return type switch
        {
            LetterSpecialType.Introduction => true, // Always available if tier met
            LetterSpecialType.AccessPermit => player.HasMemory("trader_recognized", _gameWorld.CurrentDay),
            LetterSpecialType.Endorsement => player.HasMemory("noble_contact", _gameWorld.CurrentDay),
            LetterSpecialType.Information => player.HasMemory("shadow_contact", _gameWorld.CurrentDay),
            _ => true
        };
    }

    /// <summary>
    /// Get the primary token type for a special letter type
    /// </summary>
    private ConnectionType GetRequiredTokenType(LetterSpecialType type)
    {
        return type switch
        {
            LetterSpecialType.Introduction => ConnectionType.Trust,
            LetterSpecialType.AccessPermit => ConnectionType.Commerce,
            LetterSpecialType.Endorsement => ConnectionType.Status,
            LetterSpecialType.Information => ConnectionType.Shadow,
            _ => ConnectionType.Trust
        };
    }
}

