using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for generating special letters based on token thresholds.
/// Implements the design principle: players must explicitly request special letters.
/// </summary>
public class SpecialLetterGenerationService
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly ObligationQueueManager _letterQueueManager;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly InformationDiscoveryManager _informationManager;

    // Token thresholds for special letter generation
    private const int SPECIAL_LETTER_THRESHOLD = 5;
    private const int SPECIAL_LETTER_TOKEN_COST = 3;

    public SpecialLetterGenerationService(
        GameWorld gameWorld,
        TokenMechanicsManager tokenManager,
        ObligationQueueManager letterQueueManager,
        MessageSystem messageSystem,
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        InformationDiscoveryManager informationManager)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _letterQueueManager = letterQueueManager;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _informationManager = informationManager;
    }

    /// <summary>
    /// Check if player can request a special letter from an NPC
    /// </summary>
    public bool CanRequestSpecialLetter(string npcId, ConnectionType tokenType)
    {
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
        int tokenCount = tokens.GetValueOrDefault(tokenType, 0);

        // Must have threshold tokens and correct type match
        bool hasEnoughTokens = tokenCount >= SPECIAL_LETTER_THRESHOLD;
        bool typeMatches = GetSpecialTypeForTokenType(tokenType) != LetterSpecialType.None;

        return hasEnoughTokens && typeMatches;
    }

    /// <summary>
    /// Get available special letter types for an NPC based on token counts
    /// </summary>
    public List<SpecialLetterOption> GetAvailableSpecialLetters(string npcId)
    {
        List<SpecialLetterOption> options = new List<SpecialLetterOption>();
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
        NPC npc = _npcRepository.GetById(npcId);

        if (npc == null) return options;

        // Check each token type
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            if (CanRequestSpecialLetter(npcId, tokenType))
            {
                LetterSpecialType specialType = GetSpecialTypeForTokenType(tokenType);

                SpecialLetterOption option = new SpecialLetterOption
                {
                    TokenType = tokenType,
                    SpecialType = specialType,
                    RequiredTokens = SPECIAL_LETTER_THRESHOLD,
                    Cost = SPECIAL_LETTER_TOKEN_COST,
                    CurrentTokens = tokens.GetValueOrDefault(tokenType, 0),
                    Description = GetSpecialLetterDescription(specialType, npc),
                    TargetInfo = GetTargetInfo(specialType, npc)
                };

                options.Add(option);
            }
        }

        return options;
    }

    /// <summary>
    /// Request a special letter from an NPC
    /// </summary>
    public bool RequestSpecialLetter(string npcId, ConnectionType tokenType)
    {
        if (!CanRequestSpecialLetter(npcId, tokenType))
        {
            _messageSystem.AddSpecialLetterRequestResult(npcId, tokenType, SpecialLetterRequestResult.InsufficientTokens);
            return false;
        }

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return false;

        // Deduct token cost
        _tokenManager.RemoveTokensFromNPC(tokenType, SPECIAL_LETTER_TOKEN_COST, npcId);

        // Generate the special letter
        LetterSpecialType specialType = GetSpecialTypeForTokenType(tokenType);
        DeliveryObligation specialDeliveryObligation = CreateSpecialLetter(npc, tokenType, specialType);

        // Create both obligation (queue) and physical letter (satchel)
        Letter physicalLetter = new Letter
        {
            Id = specialDeliveryObligation.Id,
            SenderName = specialDeliveryObligation.SenderName,
            RecipientName = specialDeliveryObligation.RecipientName,
            SpecialType = specialType
        };
        
        // Set unlock properties on physical letter based on type
        switch (specialType)
        {
            case LetterSpecialType.Introduction:
                physicalLetter.UnlocksNPCId = targetId;
                break;
            case LetterSpecialType.AccessPermit:
                physicalLetter.UnlocksRouteId = targetId;
                break;
        }

        // Add physical letter to satchel
        _gameWorld.GetPlayer().CarriedLetters.Add(physicalLetter);

        // Announce the special letter (position 0 means satchel)
        ShowSpecialLetterNarrative(specialDeliveryObligation, npc, tokenType, 0);

        return true;
    }

    private DeliveryObligation CreateSpecialLetter(NPC npc, ConnectionType tokenType, LetterSpecialType specialType)
    {
        // Generate appropriate recipient based on special type
        string recipientName = GenerateRecipient(specialType, npc);
        string targetId = GenerateTargetId(specialType, npc);

        DeliveryObligation obligation = new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = npc.ID,
            SenderName = npc.Name,
            RecipientName = recipientName,
            TokenType = tokenType,
            Payment = 0, // Special letters have no payment - they unlock mechanics, not earn coins
            DeadlineInMinutes = GetSpecialLetterDeadline(specialType),
            Tier = TierLevel.T3 // Special letters are tier 3
        };

        // Special properties are set on physical Letter, not DeliveryObligation

        return obligation;
    }

    private void ShowSpecialLetterNarrative(DeliveryObligation obligation, NPC npc, ConnectionType tokenType, int position)
    {
        // Use categorical success message
        _messageSystem.AddSpecialLetterRequestResult(npc.ID, tokenType, SpecialLetterRequestResult.Success);

        // Create categorical special letter event for detailed UI handling
        var specialLetterEvent = new SpecialLetterEvent
        {
            EventType = GetSpecialTypeForTokenType(tokenType) switch
            {
                LetterSpecialType.Introduction => SpecialLetterEventType.IntroductionLetterGenerated,
                LetterSpecialType.AccessPermit => SpecialLetterEventType.AccessPermitGenerated,
                _ => throw new ArgumentException($"Unsupported token type: {tokenType}")
            },
            TargetNPCId = npc.ID,
            LetterType = GetSpecialTypeForTokenType(tokenType),
            Severity = NarrativeSeverity.Success,
            Position = position,
            TokenCost = SPECIAL_LETTER_TOKEN_COST,
            TokenType = tokenType,
            RecipientName = obligation.RecipientName
        };

        _messageSystem.AddSpecialLetterEvent(specialLetterEvent);
    }

    private LetterSpecialType GetSpecialTypeForTokenType(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => LetterSpecialType.Introduction,
            ConnectionType.Commerce => LetterSpecialType.AccessPermit,
            _ => LetterSpecialType.None // Only Trust and Commerce supported
        };
    }

    private string GetSpecialLetterDescription(LetterSpecialType type, NPC npc)
    {
        return type switch
        {
            LetterSpecialType.Introduction =>
                $"{npc.Name} can introduce you to someone in their network",
            LetterSpecialType.AccessPermit =>
                $"{npc.Name} can arrange access to new routes",
            _ => ""
        };
    }

    private string GetTargetInfo(LetterSpecialType type, NPC npc)
    {
        return type switch
        {
            LetterSpecialType.Introduction => "Unlocks: New Contact",
            LetterSpecialType.AccessPermit => "Unlocks: New Routes",
            _ => ""
        };
    }

    private string GenerateRecipient(LetterSpecialType type, NPC sender)
    {
        // In a full implementation, this would select from appropriate NPCs/locations
        return type switch
        {
            LetterSpecialType.Introduction => "Lord Blackwood", // TODO: Select from high-tier NPCs
            LetterSpecialType.AccessPermit => "Harbor Master", // Transport NPCs handle route permits
            _ => "Unknown"
        };
    }

    private string GenerateTargetId(LetterSpecialType type, NPC sender)
    {
        // In a full implementation, this would select appropriate IDs
        return type switch
        {
            LetterSpecialType.Introduction => "npc_lord_blackwood",
            LetterSpecialType.AccessPermit => "route_harbor_passage", // Routes, not locations
            _ => ""
        };
    }

    // GenerateInformationId removed - Information letters no longer exist

    // GetSpecialLetterPayment removed - special letters have no payment

    private int GetSpecialLetterDeadline(LetterSpecialType type)
    {
        return type switch
        {
            LetterSpecialType.Introduction => 5,
            LetterSpecialType.AccessPermit => 4,
            _ => 4
        };
    }
}

/// <summary>
/// Represents an available special letter option
/// </summary>
public class SpecialLetterOption
{
    public ConnectionType TokenType { get; set; }
    public LetterSpecialType SpecialType { get; set; }
    public int RequiredTokens { get; set; }
    public int Cost { get; set; }
    public int CurrentTokens { get; set; }
    public string Description { get; set; }
    public string TargetInfo { get; set; }

    public bool CanAfford => CurrentTokens >= Cost;
}