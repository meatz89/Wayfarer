using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Service for managing NPC-initiated letter offers.
/// NPCs with 3+ connections proactively offer private letters to players they trust.
/// This implements the "Direct Approaches" gameplay mechanic.
/// </summary>
public class NPCLetterOfferService
{
    private readonly GameWorld _gameWorld;
    private readonly NPCRepository _npcRepository;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly MessageSystem _messageSystem;
    private readonly Random _random = new Random();
    
    // Store pending offers for each NPC
    private readonly Dictionary<string, List<LetterOffer>> _pendingOffers = new();
    
    // Track last generation time to prevent spam
    private TimeBlocks _lastGenerationTimeBlock = TimeBlocks.Dawn;
    private int _lastGenerationDay = 0;

    public NPCLetterOfferService(
        GameWorld gameWorld,
        NPCRepository npcRepository,
        LetterTemplateRepository letterTemplateRepository,
        ConnectionTokenManager connectionTokenManager,
        LetterQueueManager letterQueueManager,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _npcRepository = npcRepository;
        _letterTemplateRepository = letterTemplateRepository;
        _connectionTokenManager = connectionTokenManager;
        _letterQueueManager = letterQueueManager;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Determine if an NPC should offer letters based on relationship level.
    /// NPCs with 3+ connections offer "Direct Approaches" - private letters.
    /// </summary>
    public bool ShouldNPCOfferLetters(string npcId)
    {
        var npcTokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        var totalConnections = npcTokens.Values.Sum();
        
        // 3+ connections unlock "Direct Approaches"
        return totalConnections >= GameRules.TOKENS_BASIC_THRESHOLD;
    }

    /// <summary>
    /// Generate letter offers from an NPC to the player.
    /// Called when player visits NPC location.
    /// </summary>
    public List<LetterOffer> GenerateNPCLetterOffers(string npcId)
    {
        var offers = new List<LetterOffer>();
        
        // Check if NPC should offer letters
        if (!ShouldNPCOfferLetters(npcId))
        {
            return offers;
        }

        var npc = _npcRepository.GetNPCById(npcId);
        if (npc == null)
        {
            return offers;
        }

        // Get available letter types based on NPC definition
        var availableLetterTypes = GetAvailableLetterTypes(npc);
        
        // Get NPC's token counts
        var npcTokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        var totalConnections = npcTokens.Values.Sum();
        
        // Generate offers for each letter type the NPC can provide
        foreach (var letterType in availableLetterTypes)
        {
            // Check if player has enough tokens of this specific type with this NPC
            var tokensOfType = npcTokens.GetValueOrDefault(letterType, 0);
            
            // Only generate offer if player has enough relationship with this token type
            if (tokensOfType >= GameRules.TOKENS_BASIC_THRESHOLD) // Basic threshold for any offers
            {
                // Pass tokens of this specific type, not total tokens
                var offer = CreateLetterOffer(npc, letterType, tokensOfType);
                if (offer != null)
                {
                    offers.Add(offer);
                }
            }
        }

        return offers;
    }

    /// <summary>
    /// Get available letter types from NPC's defined token types.
    /// </summary>
    private List<ConnectionType> GetAvailableLetterTypes(NPC npc)
    {
        var availableTypes = new List<ConnectionType>();
        
        // Use the NPC's defined letter token types
        if (npc.LetterTokenTypes != null && npc.LetterTokenTypes.Any())
        {
            availableTypes.AddRange(npc.LetterTokenTypes);
        }
        else
        {
            // Fallback to profession-based defaults if no token types defined
            switch (npc.Profession)
            {
                case Professions.Merchant:
                    availableTypes.Add(ConnectionType.Trade);
                    break;
                case Professions.Courtier:
                    availableTypes.Add(ConnectionType.Noble);
                    break;
                case Professions.Thief:
                    availableTypes.Add(ConnectionType.Shadow);
                    break;
                case Professions.Scholar:
                    availableTypes.Add(ConnectionType.Trust);
                    break;
                case Professions.Ranger:
                case Professions.Soldier:
                default:
                    availableTypes.Add(ConnectionType.Common);
                    break;
            }
        }

        return availableTypes;
    }

    /// <summary>
    /// Create a letter offer from an NPC to the player.
    /// </summary>
    private LetterOffer CreateLetterOffer(NPC npc, ConnectionType letterType, int tokensOfType)
    {
        // Get appropriate letter templates for this type
        var allTemplates = _letterTemplateRepository.GetTemplatesByTokenType(letterType);
        
        // Filter templates by token threshold - player must have enough tokens to unlock each category
        var availableTemplates = allTemplates.Where(t => tokensOfType >= t.MinTokensRequired).ToList();
        
        if (!availableTemplates.Any())
        {
            return null;
        }

        // Prefer higher category templates if available
        var templatesByCategory = availableTemplates
            .GroupBy(t => t.Category)
            .OrderByDescending(g => g.Key) // Premium > Quality > Basic
            .First()
            .ToList();
        
        var template = templatesByCategory[_random.Next(templatesByCategory.Count)];
        
        // Create personal message based on NPC and relationship level
        var message = CreatePersonalMessage(npc, letterType, tokensOfType);
        
        // Payment comes directly from template ranges - no modifiers!
        var payment = _random.Next(template.MinPayment, template.MaxPayment + 1);
        
        // Calculate deadline - Direct Approaches tend to be more generous
        var baseDeadline = _random.Next(template.MinDeadline, template.MaxDeadline + 1);
        var deadlineBonus = tokensOfType >= GameRules.TOKENS_QUALITY_THRESHOLD ? 1 : 0; // +1 day for quality relationships
        var finalDeadline = baseDeadline + deadlineBonus;

        return new LetterOffer
        {
            Id = Guid.NewGuid().ToString(),
            NPCId = npc.ID,
            NPCName = npc.Name,
            LetterType = letterType,
            Message = message,
            Payment = payment,
            Deadline = finalDeadline,
            TemplateId = template.Id,
            IsDirectApproach = true,
            Category = template.Category
        };
    }

    /// <summary>
    /// Create a personal message for the letter offer.
    /// </summary>
    private string CreatePersonalMessage(NPC npc, ConnectionType letterType, int tokensOfType)
    {
        var messages = new List<string>();
        
        // Personal messages based on relationship strength
        if (tokensOfType >= GameRules.TOKENS_QUALITY_THRESHOLD)
        {
            messages.AddRange(new[]
            {
                "You're the only one I trust with this important task.",
                "I have something special that needs your particular expertise.",
                "There's a delicate matter that requires someone I can depend on completely."
            });
        }
        else
        {
            messages.AddRange(new[]
            {
                "My cousin needs this delivered, and I'd only trust you with it.",
                "I have a friend who could use your services for a private matter.",
                "Someone close to me needs a reliable person for a personal delivery."
            });
        }

        // Add profession-specific flavor
        switch (npc.Profession)
        {
            case Professions.Merchant:
                messages.Add("I have a business associate who needs discrete correspondence handled.");
                break;
            case Professions.Courtier:
                messages.Add("A noble friend requires someone trustworthy for a sensitive delivery.");
                break;
            case Professions.Thief:
                messages.Add("I know someone who needs... unconventional postal services.");
                break;
            case Professions.Scholar:
                messages.Add("A colleague needs someone reliable to carry important research.");
                break;
        }

        return messages[_random.Next(messages.Count)];
    }

    /// <summary>
    /// Accept an NPC's letter offer.
    /// No connection cost for Direct Approaches - these are gifts from relationships.
    /// </summary>
    public bool AcceptNPCLetterOffer(string npcId, string offerId)
    {
        var npc = _npcRepository.GetNPCById(npcId);
        if (npc == null)
        {
            _messageSystem.AddSystemMessage("NPC not found.", SystemMessageTypes.Danger);
            return false;
        }

        // Check if queue has space
        if (_letterQueueManager.IsQueueFull())
        {
            _messageSystem.AddSystemMessage("Letter queue is full. Cannot accept more letters.", SystemMessageTypes.Danger);
            return false;
        }

        // Generate the actual letter from the offer
        var offer = GetLetterOfferById(npcId, offerId);
        if (offer == null)
        {
            _messageSystem.AddSystemMessage("Letter offer not found.", SystemMessageTypes.Danger);
            return false;
        }

        var letter = GenerateLetterFromOffer(offer);
        if (letter == null)
        {
            _messageSystem.AddSystemMessage("Failed to generate letter from offer.", SystemMessageTypes.Danger);
            return false;
        }

        // Add letter to queue
        _letterQueueManager.AddLetterWithObligationEffects(letter);
        
        // Strengthen relationship with this NPC (1 token for accepting Direct Approach)
        _connectionTokenManager.AddTokens(offer.LetterType, 1, npcId);
        
        // Enhanced success feedback
        _messageSystem.AddSystemMessage($"💬 {npc.Name} appreciates your acceptance!", SystemMessageTypes.Success);
        _messageSystem.AddSystemMessage($"✉️ {offer.LetterType} letter added to queue", SystemMessageTypes.Info);
        _messageSystem.AddSystemMessage($"💰 Payment: {offer.Payment} coins", SystemMessageTypes.Info);
        _messageSystem.AddSystemMessage($"⏰ Deadline: {offer.Deadline} days", SystemMessageTypes.Info);
        _messageSystem.AddSystemMessage($"🤝 Relationship with {npc.Name} strengthened", SystemMessageTypes.Success);
        
        // Remove from pending offers
        RemovePendingOffer(npcId, offerId);

        return true;
    }

    /// <summary>
    /// Generate a letter from an accepted offer.
    /// </summary>
    private Letter GenerateLetterFromOffer(LetterOffer offer)
    {
        var template = _letterTemplateRepository.GetTemplateById(offer.TemplateId);
        if (template == null)
        {
            return null;
        }

        // Find recipient NPC (different from sender)
        var allNPCs = _npcRepository.GetAllNPCs();
        var possibleRecipients = allNPCs.Where(n => n.ID != offer.NPCId).ToList();
        
        if (!possibleRecipients.Any())
        {
            return null;
        }

        var recipient = possibleRecipients[_random.Next(possibleRecipients.Count)];
        
        // Generate letter using template
        var letter = _letterTemplateRepository.GenerateLetterFromTemplate(template, offer.NPCName, recipient.Name);
        
        if (letter != null)
        {
            // Apply offer-specific properties
            letter.Payment = offer.Payment;
            letter.Deadline = offer.Deadline;
            letter.TokenType = offer.LetterType;
            
            // Mark as Direct Approach
            letter.IsGenerated = true;
            letter.GenerationReason = "Direct Approach";
        }

        return letter;
    }

    /// <summary>
    /// Get a specific letter offer by ID (for UI state management).
    /// </summary>
    private LetterOffer GetLetterOfferById(string npcId, string offerId)
    {
        // Check stored offers first
        if (_pendingOffers.TryGetValue(npcId, out var offers))
        {
            var offer = offers.FirstOrDefault(o => o.Id == offerId);
            if (offer != null) return offer;
        }
        
        // Fallback to regeneration
        var generatedOffers = GenerateNPCLetterOffers(npcId);
        return generatedOffers.FirstOrDefault(o => o.Id == offerId);
    }

    /// <summary>
    /// Get all NPCs at the player's current location who can offer letters.
    /// </summary>
    public List<NPC> GetNPCsWithLetterOffers(string locationId)
    {
        var locationNPCs = _npcRepository.GetNPCsForLocation(locationId);
        var npcsWithOffers = new List<NPC>();

        foreach (var npc in locationNPCs)
        {
            if (ShouldNPCOfferLetters(npc.ID))
            {
                npcsWithOffers.Add(npc);
            }
        }

        return npcsWithOffers;
    }

    /// <summary>
    /// Check if any NPCs at the current location have letter offers.
    /// </summary>
    public bool HasLetterOffersAtLocation(string locationId)
    {
        return GetNPCsWithLetterOffers(locationId).Any();
    }
    
    /// <summary>
    /// Generate periodic letter offers from NPCs with strong relationships.
    /// Called during time block transitions.
    /// </summary>
    public void GeneratePeriodicOffers()
    {
        var currentDay = _gameWorld.CurrentDay;
        var currentTimeBlock = _gameWorld.TimeManager.GetCurrentTimeBlock();
        
        // Only generate once per time block
        if (currentDay == _lastGenerationDay && currentTimeBlock == _lastGenerationTimeBlock)
        {
            return;
        }
        
        _lastGenerationDay = currentDay;
        _lastGenerationTimeBlock = currentTimeBlock;
        
        // Get all NPCs with strong relationships (3+ tokens)
        var allNPCs = _npcRepository.GetAllNPCs();
        var eligibleNPCs = allNPCs.Where(npc => ShouldNPCOfferLetters(npc.ID)).ToList();
        
        if (!eligibleNPCs.Any()) return;
        
        // 20% chance per eligible NPC per time block to generate an offer
        foreach (var npc in eligibleNPCs)
        {
            if (_random.Next(100) < 20)
            {
                GeneratePendingOfferForNPC(npc);
            }
        }
    }
    
    /// <summary>
    /// Generate a pending offer for a specific NPC.
    /// </summary>
    private void GeneratePendingOfferForNPC(NPC npc)
    {
        // Check if NPC already has pending offers
        if (!_pendingOffers.TryGetValue(npc.ID, out var offers))
        {
            offers = new List<LetterOffer>();
            _pendingOffers[npc.ID] = offers;
        }
        
        // Limit to 2 pending offers per NPC
        if (offers.Count >= 2)
        {
            return;
        }
        
        // Generate one offer
        var newOffers = GenerateNPCLetterOffers(npc.ID);
        if (newOffers.Any())
        {
            var offer = newOffers.First();
            offer.GeneratedDay = _gameWorld.CurrentDay;
            offer.GeneratedTimeBlock = _gameWorld.TimeManager.GetCurrentTimeBlock();
            offers.Add(offer);
            
            // Add rich narrative about why NPC is offering now
            var timeNarrative = _gameWorld.TimeManager.GetCurrentTimeBlock() switch
            {
                TimeBlocks.Dawn => "early this morning",
                TimeBlocks.Morning => "this morning",
                TimeBlocks.Afternoon => "this afternoon",
                TimeBlocks.Evening => "this evening",
                TimeBlocks.Night => "late tonight",
                _ => "recently"
            };
            
            // Notify player with narrative context
            _messageSystem.AddSystemMessage(
                $"📮 {npc.Name} approached you {timeNarrative} with a letter request.", 
                SystemMessageTypes.Info
            );
            
            // Add the offer message if available
            if (!string.IsNullOrEmpty(offer.Message))
            {
                _messageSystem.AddSystemMessage(
                    $"\"{offer.Message}\"",
                    SystemMessageTypes.Info
                );
            }
        }
    }
    
    /// <summary>
    /// Get pending offers for a specific NPC.
    /// </summary>
    public List<LetterOffer> GetPendingOffersForNPC(string npcId)
    {
        if (_pendingOffers.TryGetValue(npcId, out var offers))
        {
            // Remove expired offers (older than 2 days)
            offers.RemoveAll(o => _gameWorld.CurrentDay - o.GeneratedDay > 2);
            return offers.ToList(); // Return copy
        }
        return new List<LetterOffer>();
    }
    
    /// <summary>
    /// Check if an NPC has pending offers.
    /// </summary>
    public bool HasPendingOffers(string npcId)
    {
        return GetPendingOffersForNPC(npcId).Any();
    }
    
    /// <summary>
    /// Clear accepted/refused offer from pending list.
    /// </summary>
    private void RemovePendingOffer(string npcId, string offerId)
    {
        if (_pendingOffers.TryGetValue(npcId, out var offers))
        {
            offers.RemoveAll(o => o.Id == offerId);
            if (!offers.Any())
            {
                _pendingOffers.Remove(npcId);
            }
        }
    }
    
    /// <summary>
    /// Generate a return letter when recipient wants to send a reply.
    /// </summary>
    public Letter GenerateReturnLetter(NPC recipient, Letter originalLetter)
    {
        // Find a suitable return recipient - prefer the original sender if they're an NPC
        NPC returnRecipient = null;
        if (!string.IsNullOrEmpty(originalLetter.SenderId))
        {
            returnRecipient = _npcRepository.GetNPCById(originalLetter.SenderId);
        }
        
        // If no valid sender, pick a random NPC from a different location
        if (returnRecipient == null)
        {
            var eligibleNPCs = _npcRepository.GetAllNPCs()
                .Where(n => n.ID != recipient.ID && n.Location != recipient.Location)
                .ToList();
                
            if (!eligibleNPCs.Any())
            {
                // Fall back to any NPC
                eligibleNPCs = _npcRepository.GetAllNPCs()
                    .Where(n => n.ID != recipient.ID)
                    .ToList();
            }
            
            if (eligibleNPCs.Any())
            {
                returnRecipient = eligibleNPCs[_random.Next(eligibleNPCs.Count)];
            }
        }
        
        if (returnRecipient == null) return null;
        
        // Generate return letter
        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = recipient.ID,
            SenderName = recipient.Name,
            RecipientId = returnRecipient.ID,
            RecipientName = returnRecipient.Name,
            Payment = _random.Next(8, 16), // Return letters pay slightly more
            Deadline = _gameWorld.CurrentDay + _random.Next(3, 5),
            IsGenerated = true,
            GenerationReason = "Return Letter",
            State = LetterState.Accepted,
            Message = $"Reply to: {originalLetter.SenderName}"
        };
        
        // Determine token type based on recipient
        if (returnRecipient.LetterTokenTypes.Any())
        {
            letter.TokenType = returnRecipient.LetterTokenTypes.First();
        }
        else
        {
            letter.TokenType = ConnectionType.Common;
        }
        
        // Add urgency sometimes
        if (_random.Next(100) < 30)
        {
            letter.PhysicalProperties |= LetterPhysicalProperties.Valuable;
            letter.Payment += 3;
        }
        
        return letter;
    }
}

/// <summary>
/// Represents a letter offer from an NPC to the player.
/// </summary>
public class LetterOffer
{
    public string Id { get; set; }
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public ConnectionType LetterType { get; set; }
    public string Message { get; set; }
    public int Payment { get; set; }
    public int Deadline { get; set; }
    public string TemplateId { get; set; }
    public bool IsDirectApproach { get; set; }
    public int GeneratedDay { get; set; }
    public TimeBlocks GeneratedTimeBlock { get; set; }
    public LetterCategory Category { get; set; }
}