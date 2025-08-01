using System;
using System.Collections.Generic;
using System.Linq;

public class NoticeBoardService
{
    private readonly GameWorld _gameWorld;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly Random _random = new Random();

    public NoticeBoardService(
        GameWorld gameWorld,
        LetterTemplateRepository letterTemplateRepository,
        NPCRepository npcRepository,
        ConnectionTokenManager connectionTokenManager,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _letterTemplateRepository = letterTemplateRepository;
        _npcRepository = npcRepository;
        _connectionTokenManager = connectionTokenManager;
        _messageSystem = messageSystem;
    }

    // Notice Board Options
    public enum NoticeBoardOption
    {
        AnythingHeading,    // "Anything heading [direction]?" - 2 tokens for random letter
        LookingForWork,     // "Looking for [type] work" - 3 tokens for specific type  
        UrgentDeliveries    // "Urgent deliveries?" - 5 tokens for high-pay urgent letter
    }

    // Check if player can afford a notice board option
    public bool CanAffordOption(NoticeBoardOption option)
    {
        switch (option)
        {
            case NoticeBoardOption.AnythingHeading:
                return GetTokenCostForOption(option) <= GetPlayerTotalTokens();
            case NoticeBoardOption.LookingForWork:
                return GetTokenCostForOption(option) <= GetPlayerTotalTokens();
            case NoticeBoardOption.UrgentDeliveries:
                return GetTokenCostForOption(option) <= GetPlayerTotalTokens();
            default:
                return false;
        }
    }

    // Get the token cost for each option
    public int GetTokenCostForOption(NoticeBoardOption option)
    {
        switch (option)
        {
            case NoticeBoardOption.AnythingHeading:
                return 2;
            case NoticeBoardOption.LookingForWork:
                return 3;
            case NoticeBoardOption.UrgentDeliveries:
                return 5;
            default:
                return 0;
        }
    }

    // Get player's total tokens across all types
    private int GetPlayerTotalTokens()
    {
        return _connectionTokenManager.GetPlayerTokens().Values.Sum();
    }

    // Use the notice board - returns generated letter or null
    public Letter UseNoticeBoard(NoticeBoardOption option, string direction = null, ConnectionType? specificType = null)
    {
        int tokenCost = GetTokenCostForOption(option);

        // Show what the player is attempting
        _messageSystem.AddSystemMessage(
            "📋 Approaching the Notice Board at the market square...",
            SystemMessageTypes.Info
        );

        // Validate player can afford it
        if (!CanAffordOption(option))
        {
            _messageSystem.AddSystemMessage(
                $"❌ This option requires {tokenCost} tokens. You don't have enough social capital.",
                SystemMessageTypes.Danger
            );
            return null;
        }

        // Determine which tokens to spend
        Dictionary<ConnectionType, int> tokenPayment = DetermineTokenPayment(tokenCost);
        if (tokenPayment == null || tokenPayment.Count == 0)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Cannot gather {tokenCost} tokens for the board keeper.",
                SystemMessageTypes.Danger
            );
            return null;
        }

        // Spend the tokens
        foreach (KeyValuePair<ConnectionType, int> payment in tokenPayment)
        {
            if (!_connectionTokenManager.SpendTokens(payment.Key, payment.Value))
            {
                _messageSystem.AddSystemMessage(
                    $"❌ Failed to spend {payment.Value} {payment.Key} tokens.",
                    SystemMessageTypes.Danger
                );
                return null;
            }
        }

        // Generate appropriate letter based on option
        Letter generatedLetter = null;
        switch (option)
        {
            case NoticeBoardOption.AnythingHeading:
                generatedLetter = GenerateDirectionalLetter(direction);
                break;
            case NoticeBoardOption.LookingForWork:
                generatedLetter = GenerateSpecificTypeLetter(specificType ?? ConnectionType.Trust);
                break;
            case NoticeBoardOption.UrgentDeliveries:
                generatedLetter = GenerateUrgentHighPayLetter();
                break;
        }

        if (generatedLetter != null)
        {
            // Provide narrative feedback
            ShowNoticeBoardSuccess(option, generatedLetter, tokenPayment);
        }
        else
        {
            _messageSystem.AddSystemMessage(
                "📭 The board keeper shrugs. 'Nothing matching that description today.'",
                SystemMessageTypes.Warning
            );
        }

        return generatedLetter;
    }

    // Determine how to pay the token cost (mix of different types if needed)
    private Dictionary<ConnectionType, int> DetermineTokenPayment(int totalCost)
    {
        Dictionary<ConnectionType, int> payment = new Dictionary<ConnectionType, int>();
        Dictionary<ConnectionType, int> availableTokens = _connectionTokenManager.GetPlayerTokens();
        int remaining = totalCost;

        // Try to use tokens evenly across types
        List<ConnectionType> tokenTypes = Enum.GetValues<ConnectionType>().OrderBy(t => _random.Next()).ToList();

        foreach (ConnectionType tokenType in tokenTypes)
        {
            if (remaining <= 0) break;

            int available = availableTokens.ContainsKey(tokenType) ? availableTokens[tokenType] : 0;
            if (available > 0)
            {
                int toSpend = Math.Min(available, remaining);
                payment[tokenType] = toSpend;
                remaining -= toSpend;
            }
        }

        return remaining == 0 ? payment : new Dictionary<ConnectionType, int>();
    }

    // Generate a letter heading in a specific direction
    private Letter GenerateDirectionalLetter(string direction)
    {
        // Get all locations in that general direction
        List<string> targetLocations = GetLocationsInDirection(direction);
        if (!targetLocations.Any()) return null;

        string targetLocation = targetLocations[_random.Next(targetLocations.Count)];

        // Get a random template
        LetterTemplate template = _letterTemplateRepository.GetRandomTemplate();
        if (template == null) return null;

        // Find NPCs at target location
        List<NPC> targetNpcs = _npcRepository.GetNPCsForLocation(targetLocation);
        if (!targetNpcs.Any()) return null;

        NPC recipient = targetNpcs[_random.Next(targetNpcs.Count)];

        // Get a random sender from current location
        string currentLocation = _gameWorld.GetPlayer().CurrentLocationSpot?.LocationId ?? "millbrook";
        List<NPC> senderNpcs = _npcRepository.GetNPCsForLocation(currentLocation);
        if (!senderNpcs.Any()) return null;

        NPC sender = senderNpcs[_random.Next(senderNpcs.Count)];

        // Generate the letter
        Letter? letter = _letterTemplateRepository.GenerateLetterFromTemplate(template, sender.Name, recipient.Name);
        if (letter != null)
        {
            letter.Description = $"Letter to {recipient.Name} in {targetLocation}";
        }

        return letter;
    }

    // Generate a letter of a specific token type
    private Letter GenerateSpecificTypeLetter(ConnectionType tokenType)
    {
        // Get templates of the requested type
        List<LetterTemplate> templates = _letterTemplateRepository.GetTemplatesByTokenType(tokenType);
        if (!templates.Any()) return null;

        LetterTemplate template = templates[_random.Next(templates.Count)];

        // Find NPCs that deal in this token type
        List<NPC> relevantNpcs = _npcRepository.GetAllNPCs()
            .Where(npc => npc.LetterTokenTypes.Contains(tokenType))
            .ToList();

        if (relevantNpcs.Count < 2) return null;

        NPC sender = relevantNpcs[_random.Next(relevantNpcs.Count)];
        NPC recipient = relevantNpcs.Where(n => n.ID != sender.ID).ToList()[_random.Next(relevantNpcs.Count - 1)];

        // Generate letter with the specific type
        return _letterTemplateRepository.GenerateLetterFromTemplate(template, sender.Name, recipient.Name);
    }

    // Generate an urgent, high-paying letter
    private Letter GenerateUrgentHighPayLetter()
    {
        // Get all templates and prefer those with higher payment ranges
        List<LetterTemplate> templates = _letterTemplateRepository.GetAllTemplates()
            .OrderByDescending(t => t.MaxPayment)
            .Take(5)
            .ToList();

        if (!templates.Any())
        {
            templates = _letterTemplateRepository.GetAllTemplates();
        }

        if (!templates.Any()) return null;

        LetterTemplate template = templates[_random.Next(templates.Count)];

        // Get random NPCs
        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        if (allNpcs.Count < 2) return null;

        NPC sender = allNpcs[_random.Next(allNpcs.Count)];
        NPC recipient = allNpcs.Where(n => n.ID != sender.ID).ToList()[_random.Next(allNpcs.Count - 1)];

        // Generate the letter
        Letter? letter = _letterTemplateRepository.GenerateLetterFromTemplate(template, sender.Name, recipient.Name);

        if (letter != null)
        {
            // Make it urgent and high-paying
            letter.Deadline = _random.Next(2, 4); // 2-3 days only
            letter.Payment = (int)(letter.Payment * 1.5); // 50% higher payment
            letter.Description = "URGENT: " + letter.Description;
        }

        return letter;
    }

    // Get locations in a general direction from current location
    private List<string> GetLocationsInDirection(string direction)
    {
        // This is simplified - in a full implementation, you'd have actual geography
        List<string> allLocations = new List<string> { "millbrook", "crossbridge", "thornwood" };
        string currentLocation = _gameWorld.GetPlayer().CurrentLocationSpot?.LocationId ?? "millbrook";

        // Remove current location
        allLocations.Remove(currentLocation);

        // For now, just return available locations
        // In full implementation, filter by actual direction
        return allLocations;
    }

    // Show success narrative for notice board usage
    private void ShowNoticeBoardSuccess(NoticeBoardOption option, Letter letter, Dictionary<ConnectionType, int> payment)
    {
        // Show token payment
        _messageSystem.AddSystemMessage(
            "💸 You slide tokens across the board to the keeper:",
            SystemMessageTypes.Warning
        );

        foreach (KeyValuePair<ConnectionType, int> tokenPayment in payment)
        {
            _messageSystem.AddSystemMessage(
                $"  • {tokenPayment.Value} {tokenPayment.Key} token{(tokenPayment.Value > 1 ? "s" : "")}",
                SystemMessageTypes.Info
            );
        }

        // Show result based on option
        switch (option)
        {
            case NoticeBoardOption.AnythingHeading:
                _messageSystem.AddSystemMessage(
                    "📬 'Ah yes, got something heading that way. Needs a reliable carrier.'",
                    SystemMessageTypes.Success
                );
                break;
            case NoticeBoardOption.LookingForWork:
                _messageSystem.AddSystemMessage(
                    $"📬 'Perfect timing! Fresh {letter.TokenType} work just posted this morning.'",
                    SystemMessageTypes.Success
                );
                break;
            case NoticeBoardOption.UrgentDeliveries:
                _messageSystem.AddSystemMessage(
                    "📬 'This one's hot! Good pay but the deadline's tight. You up for it?'",
                    SystemMessageTypes.Success
                );
                break;
        }

        _messageSystem.AddSystemMessage(
            $"✉️ {letter.SenderName} → {letter.RecipientName} ({letter.Payment} coins, {letter.Deadline} days)",
            SystemMessageTypes.Info
        );
    }

    // Get available directions from current location
    public List<string> GetAvailableDirections()
    {
        // Simplified - in full implementation, base on actual routes
        return new List<string> { "North", "South", "East", "West" };
    }

    // Get description for notice board option
    public string GetOptionDescription(NoticeBoardOption option)
    {
        switch (option)
        {
            case NoticeBoardOption.AnythingHeading:
                return "Ask about letters heading in a specific direction (2 tokens)";
            case NoticeBoardOption.LookingForWork:
                return "Request work of a specific type (3 tokens)";
            case NoticeBoardOption.UrgentDeliveries:
                return "Check for urgent, high-paying deliveries (5 tokens)";
            default:
                return "";
        }
    }

    /// <summary>
    /// Unlock a chain letter that was previously locked.
    /// This is called when a player delivers a chain letter that unlocks follow-up letters.
    /// </summary>
    public void UnlockChainLetter(string letterId)
    {
        // Store unlocked chain letters in player's memories
        Player player = _gameWorld.GetPlayer();
        string chainLetterKey = $"ChainLetter_{letterId}";
        MemoryFlag chainLetterMemory = new MemoryFlag
        {
            Key = chainLetterKey,
            Description = $"Unlocked chain letter opportunity: {letterId}",
            CreationDay = _gameWorld.CurrentDay,
            Importance = 3
        };

        if (!player.Memories.Any(m => m.Key == chainLetterKey))
        {
            player.Memories.Add(chainLetterMemory);

            _messageSystem.AddSystemMessage(
                $"🔓 New chain letter opportunity unlocked: {letterId}",
                SystemMessageTypes.Info
            );
        }
    }
}