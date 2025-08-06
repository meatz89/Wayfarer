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
    private readonly ConnectionTokenManager _tokenManager;
    private readonly EndorsementManager _endorsementManager;

    public SpecialLetterHandler(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        InformationDiscoveryManager informationManager,
        ConnectionTokenManager tokenManager,
        EndorsementManager endorsementManager = null)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _informationManager = informationManager;
        _tokenManager = tokenManager;
        _endorsementManager = endorsementManager ?? new EndorsementManager(gameWorld, messageSystem);
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
            _messageSystem.AddSystemMessage(
                $"+{bonusTokens} {letter.TokenType} bonus tokens for delivering special letter!",
                SystemMessageTypes.Success
            );
        }
    }

    /// <summary>
    /// Trust - Introduction letters unlock new NPCs
    /// </summary>
    private void ProcessIntroductionLetter(Letter letter)
    {
        if (string.IsNullOrEmpty(letter.UnlocksNPCId))
        {
            _messageSystem.AddSystemMessage(
                "This introduction letter seems incomplete...",
                SystemMessageTypes.Warning
            );
            return;
        }

        NPC npc = _npcRepository.GetById(letter.UnlocksNPCId);
        if (npc == null)
        {
            _messageSystem.AddSystemMessage(
                "The person this letter introduces cannot be found...",
                SystemMessageTypes.Warning
            );
            return;
        }

        // Mark NPC as discovered/unlocked
        Player player = _gameWorld.GetPlayer();
        player.AddMemory(
            $"npc_introduced_{npc.ID}",
            $"Introduced to {npc.Name} by {letter.SenderName}",
            _gameWorld.CurrentDay,
            letter.Tier
        );

        // Grant initial trust tokens with the new NPC
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 1, npc.ID);

        _messageSystem.AddSystemMessage(
            $"📜 You've been introduced to {npc.Name}! They will now offer you work.",
            SystemMessageTypes.Success
        );

        // Discover any information this NPC might share with new contacts
        _informationManager.TryDiscoverFromNPC(npc.ID, ConnectionType.Trust);
    }

    /// <summary>
    /// Commerce - Access Permit letters unlock new locations
    /// </summary>
    private void ProcessAccessPermitLetter(Letter letter)
    {
        if (string.IsNullOrEmpty(letter.UnlocksLocationId))
        {
            _messageSystem.AddSystemMessage(
                "This access permit seems incomplete...",
                SystemMessageTypes.Warning
            );
            return;
        }

        Location location = _locationRepository.GetLocation(letter.UnlocksLocationId);
        if (location == null)
        {
            _messageSystem.AddSystemMessage(
                "The location this permit grants access to cannot be found...",
                SystemMessageTypes.Warning
            );
            return;
        }

        // Mark location as accessible
        Player player = _gameWorld.GetPlayer();
        player.AddMemory(
            $"location_permit_{location.Id}",
            $"Access permit to {location.Name} granted by {letter.SenderName}",
            _gameWorld.CurrentDay,
            letter.Tier
        );

        _messageSystem.AddSystemMessage(
            $"🔓 Access granted to {location.Name}! New routes may now be available.",
            SystemMessageTypes.Success
        );

        // Discover information about this location
        _informationManager.DiscoverFromLocationVisit(location.Id);
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
        string endorsementKey = $"endorsement_{letter.SenderId}_{letter.Tier}";
        string description = GetEndorsementDescription(letter);

        player.AddMemory(
            endorsementKey,
            description,
            _gameWorld.CurrentDay,
            letter.Tier
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
        int baseBonus = letter.Tier;

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
            1 => "minor social privileges",
            2 or 3 => "improved status letter handling",
            4 or 5 => "significant social advantages",
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
            case 1:
                _messageSystem.AddSystemMessage(
                    "  • Status letters will pay +3 bonus coins",
                    SystemMessageTypes.Info
                );
                break;

            case 2:
            case 3:
                _messageSystem.AddSystemMessage(
                    "  • Status letters enter queue at position 3",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "  • Status letters get +2 days deadline",
                    SystemMessageTypes.Info
                );
                break;

            case 4:
            case 5:
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
            1 => tier1Info[random.Next(tier1Info.Length)],
            2 => tier2Info[random.Next(tier2Info.Length)],
            _ => tier3PlusInfo[random.Next(tier3PlusInfo.Length)]
        };

        player.AddMemory(
            $"shadow_info_{letter.Id}",
            discoveredInfo,
            _gameWorld.CurrentDay,
            letter.Tier
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

