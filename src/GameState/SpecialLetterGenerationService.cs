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
    private readonly ConnectionTokenManager _tokenManager;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly InformationDiscoveryManager _informationManager;

    // Token thresholds for special letter generation
    private const int SPECIAL_LETTER_THRESHOLD = 5;
    private const int SPECIAL_LETTER_TOKEN_COST = 3;

    public SpecialLetterGenerationService(
        GameWorld gameWorld,
        ConnectionTokenManager tokenManager,
        LetterQueueManager letterQueueManager,
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
            _messageSystem.AddSystemMessage(
                "You don't meet the requirements for this special letter.",
                SystemMessageTypes.Warning
            );
            return false;
        }

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return false;

        // Deduct token cost
        _tokenManager.RemoveTokensFromNPC(tokenType, SPECIAL_LETTER_TOKEN_COST, npcId);

        // Generate the special letter
        LetterSpecialType specialType = GetSpecialTypeForTokenType(tokenType);
        Letter specialLetter = CreateSpecialLetter(npc, tokenType, specialType);

        // Check if this is an Information letter that goes to inventory
        if (specialLetter.SpecialType == LetterSpecialType.Information)
        {
            // Add Information letter to carried letters (satchel) instead of queue
            _gameWorld.GetPlayer().CarriedLetters.Add(specialLetter);
            specialLetter.State = LetterState.Collected; // Mark as physical item

            // Announce the special letter (position 0 means satchel)
            ShowSpecialLetterNarrative(specialLetter, npc, tokenType, 0);
        }
        else
        {
            // Regular special letters go to queue
            int position = _letterQueueManager.AddLetterWithObligationEffects(specialLetter);

            // Announce the special letter
            ShowSpecialLetterNarrative(specialLetter, npc, tokenType, position);
        }

        return true;
    }

    private Letter CreateSpecialLetter(NPC npc, ConnectionType tokenType, LetterSpecialType specialType)
    {
        // Generate appropriate recipient based on special type
        string recipientName = GenerateRecipient(specialType, npc);
        string targetId = GenerateTargetId(specialType, npc);

        Letter letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = npc.ID,
            SenderName = npc.Name,
            RecipientName = recipientName,
            TokenType = tokenType,
            SpecialType = specialType,
            Payment = GetSpecialLetterPayment(specialType),
            DeadlineInHours = GetSpecialLetterDeadline(specialType),
            Size = SizeCategory.Medium,
            Tier = 3, // Special letters are tier 3
            State = LetterState.Offered
        };

        // Set special properties based on type
        switch (specialType)
        {
            case LetterSpecialType.Introduction:
                letter.UnlocksNPCId = targetId;
                break;
            case LetterSpecialType.AccessPermit:
                letter.UnlocksLocationId = targetId;
                break;
            case LetterSpecialType.Endorsement:
                letter.BonusDuration = 7;
                break;
            case LetterSpecialType.Information:
                letter.InformationId = targetId;
                break;
        }

        return letter;
    }

    private void ShowSpecialLetterNarrative(Letter letter, NPC npc, ConnectionType tokenType, int position)
    {
        string icon = letter.GetSpecialIcon();

        _messageSystem.AddSystemMessage(
            $"{icon} {npc.Name} trusts you with a SPECIAL LETTER!",
            SystemMessageTypes.Success
        );

        switch (letter.SpecialType)
        {
            case LetterSpecialType.Introduction:
                _messageSystem.AddSystemMessage(
                    $"📜 Letter of Introduction to {letter.RecipientName}",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "This will unlock access to a new contact when delivered!",
                    SystemMessageTypes.Info
                );
                break;

            case LetterSpecialType.AccessPermit:
                _messageSystem.AddSystemMessage(
                    $"🔓 Access Permit for restricted areas",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "Deliver this to unlock new routes and locations!",
                    SystemMessageTypes.Info
                );
                break;

            case LetterSpecialType.Endorsement:
                _messageSystem.AddSystemMessage(
                    $"⭐ Letter of Endorsement to {letter.RecipientName}",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "Take this to a guild to progress your standing!",
                    SystemMessageTypes.Info
                );
                break;

            case LetterSpecialType.Information:
                _messageSystem.AddSystemMessage(
                    $"🔍 Confidential Information about hidden secrets",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "Carrying this will reveal hidden opportunities!",
                    SystemMessageTypes.Info
                );
                break;
        }

        // Show appropriate message based on where letter was placed
        if (position == 0 && letter.SpecialType == LetterSpecialType.Information)
        {
            _messageSystem.AddSystemMessage(
                $"📨 Added to your satchel • Cost: {SPECIAL_LETTER_TOKEN_COST} {tokenType} tokens",
                SystemMessageTypes.Info
            );
        }
        else
        {
            _messageSystem.AddSystemMessage(
                $"📨 Added to queue at position {position} • Cost: {SPECIAL_LETTER_TOKEN_COST} {tokenType} tokens",
                SystemMessageTypes.Info
            );
        }
    }

    private LetterSpecialType GetSpecialTypeForTokenType(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => LetterSpecialType.Introduction,
            ConnectionType.Commerce => LetterSpecialType.AccessPermit,
            ConnectionType.Status => LetterSpecialType.Endorsement,
            ConnectionType.Shadow => LetterSpecialType.Information,
            _ => LetterSpecialType.None
        };
    }

    private string GetSpecialLetterDescription(LetterSpecialType type, NPC npc)
    {
        return type switch
        {
            LetterSpecialType.Introduction =>
                $"{npc.Name} can introduce you to someone in their network",
            LetterSpecialType.AccessPermit =>
                $"{npc.Name} can arrange access to restricted areas",
            LetterSpecialType.Endorsement =>
                $"{npc.Name} will vouch for your reputation",
            LetterSpecialType.Information =>
                $"{npc.Name} has valuable information to share",
            _ => ""
        };
    }

    private string GetTargetInfo(LetterSpecialType type, NPC npc)
    {
        return type switch
        {
            LetterSpecialType.Introduction => "Unlocks: New Contact",
            LetterSpecialType.AccessPermit => "Unlocks: New Routes",
            LetterSpecialType.Endorsement => "Benefit: Guild Standing",
            LetterSpecialType.Information => "Reveals: Hidden Content",
            _ => ""
        };
    }

    private string GenerateRecipient(LetterSpecialType type, NPC sender)
    {
        // In a full implementation, this would select from appropriate NPCs/locations
        return type switch
        {
            LetterSpecialType.Introduction => "Lord Blackwood", // TODO: Select from high-tier NPCs
            LetterSpecialType.AccessPermit => "City Watch Captain",
            LetterSpecialType.Endorsement => "Merchant Guildmaster",
            LetterSpecialType.Information => sender.Name, // Information letters go back to sender
            _ => "Unknown"
        };
    }

    private string GenerateTargetId(LetterSpecialType type, NPC sender)
    {
        // In a full implementation, this would select appropriate IDs
        return type switch
        {
            LetterSpecialType.Introduction => "npc_lord_blackwood",
            LetterSpecialType.AccessPermit => "location_noble_district",
            LetterSpecialType.Endorsement => "",
            LetterSpecialType.Information => GenerateInformationId(sender),
            _ => ""
        };
    }

    private string GenerateInformationId(NPC sender)
    {
        // Generate InformationId based on sender's connections
        // Format: "type:targetId"
        Random random = new Random();
        List<string> options = new List<string>();

        // Add potential NPC reveals
        options.Add("npc:shadow_informant");
        options.Add("npc:underground_merchant");

        // Add potential location reveals
        options.Add("location:hidden_market");
        options.Add("location:shadow_safehouse");

        // Add potential service reveals
        options.Add("service:black_market_access");
        options.Add("service:information_network");

        return options[random.Next(options.Count)];
    }

    private int GetSpecialLetterPayment(LetterSpecialType type)
    {
        return type switch
        {
            LetterSpecialType.Introduction => 15,
            LetterSpecialType.AccessPermit => 20,
            LetterSpecialType.Endorsement => 25,
            LetterSpecialType.Information => 10,
            _ => 10
        };
    }

    private int GetSpecialLetterDeadline(LetterSpecialType type)
    {
        return type switch
        {
            LetterSpecialType.Introduction => 5,
            LetterSpecialType.AccessPermit => 4,
            LetterSpecialType.Endorsement => 6,
            LetterSpecialType.Information => 3,
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