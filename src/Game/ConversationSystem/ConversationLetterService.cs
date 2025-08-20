using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// HIGHLANDER PRINCIPLE: The ONE service that handles ALL letter operations.
/// Replaces 6 different services with a single, focused conversation-based service.
/// ALL letters are created through NPC conversation choices ONLY - no automatic generation.
/// </summary>
public class ConversationLetterService
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly ObligationQueueManager _queueManager;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly Random _random = new Random();

    // Token thresholds for special letter requests
    private const int SPECIAL_LETTER_THRESHOLD = 5;
    private const int SPECIAL_LETTER_TOKEN_COST = 3;

    public ConversationLetterService(
        GameWorld gameWorld,
        TokenMechanicsManager tokenManager,
        ObligationQueueManager queueManager,
        MessageSystem messageSystem,
        NPCRepository npcRepository,
        LocationRepository locationRepository)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _queueManager = queueManager;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
    }

    /// <summary>
    /// CONVERSATION CHOICE: Request a special letter (satchel only)
    /// Creates ONLY a Letter object - no obligation in queue
    /// </summary>
    public bool RequestSpecialLetter(string npcId, LetterSpecialType specialType)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return false;

        ConnectionType tokenType = GetTokenTypeForSpecial(specialType);
        if (!CanRequestSpecialLetter(npcId, tokenType))
        {
            _messageSystem.AddSpecialLetterRequestResult(npcId, tokenType, SpecialLetterRequestResult.InsufficientTokens);
            return false;
        }

        // Deduct token cost
        _tokenManager.RemoveTokensFromNPC(tokenType, SPECIAL_LETTER_TOKEN_COST, npcId);

        // Create ONLY physical Letter (no obligation) - goes directly to satchel
        Letter specialLetter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = npc.Name,
            RecipientName = GenerateRecipient(specialType),
            SpecialType = specialType,
            Size = 1, // Special letters have standard size
            PhysicalProperties = LetterPhysicalProperties.None // No special physical properties
        };

        // Set unlock properties based on special type
        string targetId = GenerateTargetId(specialType, npc);
        switch (specialType)
        {
            case LetterSpecialType.Introduction:
                specialLetter.UnlocksNPCId = targetId;
                break;
            case LetterSpecialType.AccessPermit:
                specialLetter.UnlocksRouteId = targetId;
                break;
        }

        // Add to satchel only (competes for space with delivery letters)
        _gameWorld.GetPlayer().CarriedLetters.Add(specialLetter);

        _messageSystem.AddSpecialLetterRequestResult(npcId, tokenType, SpecialLetterRequestResult.Success);
        return true;
    }

    /// <summary>
    /// CONVERSATION CHOICE: Request a delivery letter (queue + satchel)
    /// Creates BOTH DeliveryObligation (queue) AND Letter (satchel)
    /// </summary>
    public bool RequestDeliveryLetter(string npcId, ConnectionType tokenType, LetterTemplate template)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null || template == null) return false;

        // Find recipient NPC
        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        List<NPC> possibleRecipients = allNpcs.Where(n => n.Name != npc.Name).ToList();
        if (!possibleRecipients.Any()) return false;

        NPC recipient = possibleRecipients[_random.Next(possibleRecipients.Count)];

        // Create DeliveryObligation (goes to queue)
        DeliveryObligation obligation = new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = npc.ID,
            SenderName = npc.Name,
            RecipientId = recipient.ID,
            RecipientName = recipient.Name,
            TokenType = tokenType,
            DeadlineInMinutes = _random.Next(template.MinDeadlineInMinutes, template.MaxDeadlineInMinutes + 1),
            Payment = _random.Next(template.MinPayment, template.MaxPayment + 1),
            Description = template.Description,
            ConsequenceIfLate = template.ConsequenceIfLate,
            ConsequenceIfDelivered = template.ConsequenceIfDelivered,
            EmotionalWeight = template.EmotionalWeight,
            Stakes = template.Stakes,
            Tier = template.TierLevel
        };

        // Create physical Letter (goes to satchel)
        Letter physicalLetter = new Letter
        {
            Id = obligation.Id, // Same ID links them
            SenderName = obligation.SenderName,
            RecipientName = obligation.RecipientName,
            SpecialType = LetterSpecialType.None, // Regular delivery letter
            Size = 1, // Regular delivery letters have standard size
            PhysicalProperties = LetterPhysicalProperties.None // No special physical properties
        };

        // Add obligation to queue
        _queueManager.AddLetter(obligation);

        // Add physical letter to satchel
        _gameWorld.GetPlayer().CarriedLetters.Add(physicalLetter);

        return true;
    }

    /// <summary>
    /// CONVERSATION CHOICE: Move obligation to position 1 in queue
    /// Used when player has a letter and wants to prioritize its obligation
    /// </summary>
    public bool MoveObligationToPosition1(string obligationId)
    {
        // Find the obligation in the queue and move it to position 1
        Player player = _gameWorld.GetPlayer();
        DeliveryObligation targetObligation = null;
        int currentPosition = -1;
        
        for (int i = 0; i < player.ObligationQueue.Length; i++)
        {
            if (player.ObligationQueue[i]?.Id == obligationId)
            {
                targetObligation = player.ObligationQueue[i];
                currentPosition = i;
                break;
            }
        }
        
        if (targetObligation == null || currentPosition == -1) return false;
        if (currentPosition == 0) return true; // Already at position 1
        
        // Move to position 1
        _queueManager.MoveObligationToPosition(targetObligation, 1);
        return true;
    }

    /// <summary>
    /// CONVERSATION CHOICE: Burn tokens with all senders above a specific obligation
    /// Used to clear queue above a priority letter
    /// </summary>
    public bool BurnTokensAboveObligation(string obligationId, ConnectionType tokenType)
    {
        Player player = _gameWorld.GetPlayer();
        DeliveryObligation[] queue = player.ObligationQueue;
        
        int targetIndex = -1;
        for (int i = 0; i < queue.Length; i++)
        {
            if (queue[i]?.Id == obligationId)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1) return false;

        // Burn tokens with all senders above the target obligation
        for (int i = 0; i < targetIndex; i++)
        {
            if (queue[i] != null)
            {
                _tokenManager.RemoveTokensFromNPC(tokenType, 1, queue[i].SenderId);
            }
        }

        return true;
    }

    /// <summary>
    /// CONVERSATION CHOICE: Purge obligation from queue
    /// Removes obligation but keeps physical letter if it exists
    /// </summary>
    public bool PurgeObligation(string obligationId)
    {
        // Find the obligation in the queue and remove it
        Player player = _gameWorld.GetPlayer();
        int positionToRemove = -1;
        
        for (int i = 0; i < player.ObligationQueue.Length; i++)
        {
            if (player.ObligationQueue[i]?.Id == obligationId)
            {
                positionToRemove = i + 1; // Convert to 1-based position
                break;
            }
        }
        
        if (positionToRemove == -1) return false;
        
        return _queueManager.RemoveObligationFromQueue(positionToRemove);
    }

    /// <summary>
    /// Check if player can request a special letter
    /// </summary>
    public bool CanRequestSpecialLetter(string npcId, ConnectionType tokenType)
    {
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
        int tokenCount = tokens.GetValueOrDefault(tokenType, 0);
        return tokenCount >= SPECIAL_LETTER_THRESHOLD;
    }

    /// <summary>
    /// Get available special letter options for conversation choices
    /// </summary>
    public List<SpecialLetterOption> GetAvailableSpecialLetters(string npcId)
    {
        List<SpecialLetterOption> options = new List<SpecialLetterOption>();
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
        NPC npc = _npcRepository.GetById(npcId);

        if (npc == null) return options;

        // Only Trust (Introduction) and Commerce (AccessPermit) create special letters
        foreach (ConnectionType tokenType in new[] { ConnectionType.Trust, ConnectionType.Commerce })
        {
            if (CanRequestSpecialLetter(npcId, tokenType))
            {
                LetterSpecialType specialType = GetSpecialTypeForToken(tokenType);
                options.Add(new SpecialLetterOption
                {
                    TokenType = tokenType,
                    SpecialType = specialType,
                    RequiredTokens = SPECIAL_LETTER_THRESHOLD,
                    Cost = SPECIAL_LETTER_TOKEN_COST,
                    CurrentTokens = tokens.GetValueOrDefault(tokenType, 0),
                    Description = GetSpecialLetterDescription(specialType, npc),
                    TargetInfo = GetTargetInfo(specialType)
                });
            }
        }

        return options;
    }

    /// <summary>
    /// Get letter category based on token count with NPC
    /// </summary>
    public LetterCategory GetLetterCategory(string npcId, ConnectionType tokenType)
    {
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npcId);
        int tokenCount = npcTokens.GetValueOrDefault(tokenType, 0);

        if (tokenCount >= GameRules.TOKENS_PREMIUM_THRESHOLD) return LetterCategory.Premium;
        if (tokenCount >= GameRules.TOKENS_QUALITY_THRESHOLD) return LetterCategory.Quality;
        return LetterCategory.Basic;
    }

    /// <summary>
    /// Get available delivery letter templates for conversation choices
    /// </summary>
    public List<LetterTemplate> GetAvailableDeliveryTemplates(string npcId, ConnectionType tokenType)
    {
        LetterCategory category = GetLetterCategory(npcId, tokenType);
        List<LetterTemplate> allTemplates = _gameWorld.WorldState.LetterTemplates;

        return allTemplates
            .Where(t => t.TokenType == tokenType)
            .Where(t => t.Category == category)
            .Where(t => t.SpecialType == LetterSpecialType.None) // Only regular delivery templates
            .ToList();
    }

    // Helper methods
    private ConnectionType GetTokenTypeForSpecial(LetterSpecialType specialType)
    {
        return specialType switch
        {
            LetterSpecialType.Introduction => ConnectionType.Trust,
            LetterSpecialType.AccessPermit => ConnectionType.Commerce,
            _ => ConnectionType.Trust
        };
    }

    private LetterSpecialType GetSpecialTypeForToken(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => LetterSpecialType.Introduction,
            ConnectionType.Commerce => LetterSpecialType.AccessPermit,
            _ => LetterSpecialType.None
        };
    }

    private string GenerateRecipient(LetterSpecialType type)
    {
        return type switch
        {
            LetterSpecialType.Introduction => "Lord Blackwood",
            LetterSpecialType.AccessPermit => "Harbor Master",
            _ => "Unknown"
        };
    }

    private string GenerateTargetId(LetterSpecialType type, NPC sender)
    {
        return type switch
        {
            LetterSpecialType.Introduction => "npc_lord_blackwood",
            LetterSpecialType.AccessPermit => "route_harbor_passage",
            _ => ""
        };
    }

    private int GetSpecialLetterDeadline(LetterSpecialType type)
    {
        return type switch
        {
            LetterSpecialType.Introduction => 7200, // 5 days in minutes
            LetterSpecialType.AccessPermit => 5760, // 4 days in minutes
            _ => 5760
        };
    }

    private string GetSpecialLetterDescription(LetterSpecialType type, NPC npc)
    {
        return type switch
        {
            LetterSpecialType.Introduction => $"{npc.Name} can introduce you to someone in their network",
            LetterSpecialType.AccessPermit => $"{npc.Name} can arrange access to new routes",
            _ => ""
        };
    }

    private string GetTargetInfo(LetterSpecialType type)
    {
        return type switch
        {
            LetterSpecialType.Introduction => "Unlocks: New Contact",
            LetterSpecialType.AccessPermit => "Unlocks: New Routes",
            _ => ""
        };
    }
}

/// <summary>
/// Represents an available special letter option for conversation UI
/// </summary>
public class SpecialLetterOption
{
    public ConnectionType TokenType { get; set; }
    public LetterSpecialType SpecialType { get; set; }
    public int RequiredTokens { get; set; }
    public int Cost { get; set; }
    public int CurrentTokens { get; set; }
    public string Description { get; set; } = "";
    public string TargetInfo { get; set; } = "";

    public bool CanAfford => CurrentTokens >= Cost;
}