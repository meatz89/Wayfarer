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
    private readonly TokenMechanicsManager _tokenManager;
    // EndorsementManager removed - endorsements no longer exist
    private readonly RouteRepository _routeRepository;

    public SpecialLetterHandler(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        NPCRepository npcRepository,
        TokenMechanicsManager tokenManager,
        RouteRepository routeRepository)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
        _tokenManager = tokenManager;
        _routeRepository = routeRepository;
    }

    /// <summary>
    /// Process special letter effects when delivered
    /// ARCHITECTURAL FIX: Special letters are physical Letter objects with SpecialType, but need obligation metadata
    /// Takes both Letter (for type checking) and DeliveryObligation (for delivery metadata)
    /// </summary>
    public void ProcessSpecialLetterDelivery(Letter physicalLetter, DeliveryObligation obligation)
    {
        if (physicalLetter.SpecialType == LetterSpecialType.None) return;

        switch (physicalLetter.SpecialType)
        {
            case LetterSpecialType.Introduction:
                ProcessIntroductionLetter(physicalLetter, obligation);
                break;

            case LetterSpecialType.AccessPermit:
                ProcessAccessPermitLetter(physicalLetter, obligation);
                break;
        }

        // All special letters grant bonus tokens of their type
        int bonusTokens = CalculateSpecialLetterTokenBonus(obligation);
        if (bonusTokens > 0)
        {
            _tokenManager.AddTokensToNPC(obligation.TokenType, bonusTokens, obligation.RecipientId);
            _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
            {
                EventType = SpecialLetterEventType.SpecialLetterTokenBonus,
                LetterType = physicalLetter.SpecialType,
                TokenType = obligation.TokenType,
                TokenAmount = bonusTokens,
                SenderName = obligation.SenderName,
                RecipientName = obligation.RecipientName,
                Severity = NarrativeSeverity.Success
            });
        }
    }

    /// <summary>
    /// Trust - Introduction letters unlock new NPCs
    /// </summary>
    private void ProcessIntroductionLetter(Letter physicalLetter, DeliveryObligation obligation)
    {
        if (string.IsNullOrEmpty(physicalLetter.UnlocksNPCId))
        {
            _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
            {
                EventType = SpecialLetterEventType.IntroductionLetterIncomplete,
                LetterType = LetterSpecialType.Introduction,
                SenderName = obligation.SenderName,
                RecipientName = obligation.RecipientName,
                Severity = NarrativeSeverity.Warning
            });
            return;
        }

        NPC npc = _npcRepository.GetById(physicalLetter.UnlocksNPCId);
        if (npc == null)
        {
            _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
            {
                EventType = SpecialLetterEventType.IntroductionTargetNotFound,
                LetterType = LetterSpecialType.Introduction,
                TargetNPCId = physicalLetter.UnlocksNPCId,
                SenderName = obligation.SenderName,
                RecipientName = obligation.RecipientName,
                Severity = NarrativeSeverity.Warning
            });
            return;
        }

        // Mark NPC as discovered/unlocked
        Player player = _gameWorld.GetPlayer();
        player.AddMemory(
            $"npc_introduced_{npc.ID}",
            $"Introduced to {npc.Name} by {obligation.SenderName}",
            _gameWorld.CurrentDay,
            (int)obligation.Tier
        );

        // Grant initial trust tokens with the new NPC
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 1, npc.ID);

        _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
        {
            EventType = SpecialLetterEventType.NPCIntroduced,
            LetterType = LetterSpecialType.Introduction,
            TargetNPCId = npc.ID,
            SenderName = obligation.SenderName,
            RecipientName = obligation.RecipientName,
            TokenType = ConnectionType.Trust,
            TokenAmount = 1,
            Severity = NarrativeSeverity.Success
        });
    }

    /// <summary>
    /// Diplomacy - Access Permit letters unlock new locations and routes
    /// CONTENT EFFICIENT: Works with Transport NPCs to unlock routes
    /// </summary>
    private void ProcessAccessPermitLetter(Letter physicalLetter, DeliveryObligation obligation)
    {
        Player player = _gameWorld.GetPlayer();

        // Check if delivered to a Transport NPC
        NPC recipient = _npcRepository.GetById(obligation.RecipientId);
        if (recipient != null && recipient.ProvidedServices.Contains(ServiceTypes.Transport))
        {
            // Transport NPC - unlock routes they control
            ProcessTransportPermit(obligation, recipient);
            return;
        }

        // Original location unlocking logic
        if (!string.IsNullOrEmpty(physicalLetter.UnlocksRouteId))
        {
            // Find route by ID and unlock it
            RouteOption route = _routeRepository.GetRouteById(physicalLetter.UnlocksRouteId);
            if (route == null)
            {
                _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
                {
                    EventType = SpecialLetterEventType.AccessTargetNotFound,
                    LetterType = LetterSpecialType.AccessPermit,
                    TargetRouteId = physicalLetter.UnlocksRouteId,
                    SenderName = obligation.SenderName,
                    RecipientName = obligation.RecipientName,
                    Severity = NarrativeSeverity.Warning
                });
                return;
            }

            // Mark route as accessible
            route.IsDiscovered = true;
            route.HasPermitUnlock = true;

            player.AddMemory(
                $"route_permit_{route.Id}",
                $"Access permit for {route.Name} granted by {obligation.SenderName}",
                _gameWorld.CurrentDay,
                (int)obligation.Tier
            );

            _messageSystem.AddSpecialLetterEvent(new SpecialLetterEvent
            {
                EventType = SpecialLetterEventType.RouteAccessGranted,
                LetterType = LetterSpecialType.AccessPermit,
                TargetRouteId = route.Id,
                SenderName = obligation.SenderName,
                RecipientName = obligation.RecipientName,
                Severity = NarrativeSeverity.Success
            });

            // Route unlock completed - information discovery handled separately
        }
        else
        {
            // Generic permit - unlock a route from current location
            UnlockRouteFromCurrentLocation(obligation);
        }
    }

    /// <summary>
    /// Process travel permit delivered to Transport NPC
    /// CONTENT EFFICIENT: Unlocks routes without requiring extra NPCs
    /// </summary>
    private void ProcessTransportPermit(DeliveryObligation letter, NPC transportNPC)
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
            // All routes already available - grant diplomacy tokens as bonus
            _tokenManager.AddTokensToNPC(ConnectionType.Diplomacy, 5, transportNPC.ID);
            _messageSystem.AddSystemMessage(
                $"{transportNPC.Name} notes you already have access to all routes. Your permit is filed. (+5 Diplomacy)",
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

            // For other transport NPCs, return all routes - access will be determined by actual requirements
            // (tokens, permissions, etc.) defined in the route's AccessRequirement property
            _ => allRoutes.ToList()
        };
    }

    /// <summary>
    /// Unlock a route from player's current location
    /// </summary>
    private void UnlockRouteFromCurrentLocation(DeliveryObligation obligation)
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = player.CurrentLocationSpot?.LocationId != null ?
            _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == player.CurrentLocationSpot.LocationId) : null;
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

    // Endorsement and Information letters removed from game design

    /// <summary>
    /// Calculate bonus tokens for special letter delivery
    /// </summary>
    private int CalculateSpecialLetterTokenBonus(DeliveryObligation obligation)
    {
        // Base bonus based on tier
        int baseBonus = (int)obligation.Tier;

        // Additional bonus for matching token type
        switch (obligation.TokenType)
        {
            case ConnectionType.Trust: // Introduction letters
                return baseBonus + 2;
            case ConnectionType.Diplomacy: // Access permit letters
                return baseBonus + 2;
            default:
                return baseBonus;
        }
    }

    // Endorsement and Information letter methods removed

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
            LetterSpecialType.AccessPermit => ConnectionType.Diplomacy,
            _ => ConnectionType.Trust
        };
    }
}

