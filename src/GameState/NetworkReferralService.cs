using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Service for managing network referral letters - NPCs recommending other NPCs for letter opportunities.
/// This gives players agency to actively seek letters when needed.
/// </summary>
public class NetworkReferralService
{
    private readonly GameWorld _gameWorld;
    private readonly NPCRepository _npcRepository;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly NPCLetterOfferService _letterOfferService;
    private readonly MessageSystem _messageSystem;
    private readonly Random _random = new Random();

    // Track active referrals
    private readonly Dictionary<string, List<NetworkReferral>> _activeReferrals = new();

    public NetworkReferralService(
        GameWorld gameWorld,
        NPCRepository npcRepository,
        LetterQueueManager letterQueueManager,
        LetterTemplateRepository letterTemplateRepository,
        ConnectionTokenManager connectionTokenManager,
        NPCLetterOfferService letterOfferService,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _npcRepository = npcRepository;
        _letterQueueManager = letterQueueManager;
        _letterTemplateRepository = letterTemplateRepository;
        _connectionTokenManager = connectionTokenManager;
        _letterOfferService = letterOfferService;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Check if an NPC can provide network referrals.
    /// Requires 5+ tokens with the NPC.
    /// </summary>
    public bool CanProvideReferrals(string npcId)
    {
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int totalTokens = tokens.Values.Sum();
        return totalTokens >= 5;
    }

    /// <summary>
    /// Request a network referral from an NPC.
    /// Costs 1 token of the NPC's primary type.
    /// </summary>
    public NetworkReferral RequestReferral(string npcId, ConnectionType? specificType = null)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null)
            return null;

        if (!CanProvideReferrals(npcId))
        {
            _messageSystem.AddSystemMessage(
                $"{npc.Name} doesn't know you well enough to make introductions. (Need 5+ tokens)",
                SystemMessageTypes.Warning
            );
            return null;
        }

        // Determine which token type to use
        ConnectionType tokenType = specificType ?? npc.LetterTokenTypes.FirstOrDefault();
        if (tokenType == default(ConnectionType))
            return null;

        // Check if player has tokens of this type with the NPC
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        if (tokens.GetValueOrDefault(tokenType, 0) < 1)
        {
            _messageSystem.AddSystemMessage(
                $"You need at least 1 {tokenType} token with {npc.Name} for a referral.",
                SystemMessageTypes.Warning
            );
            return null;
        }

        // Spend the token
        if (!_connectionTokenManager.SpendTokens(tokenType, 1, npcId))
            return null;

        // Generate referral
        NetworkReferral? referral = GenerateReferral(npc, tokenType);

        if (referral != null)
        {
            // Store the referral
            if (!_activeReferrals.ContainsKey(npcId))
                _activeReferrals[npcId] = new List<NetworkReferral>();
            _activeReferrals[npcId].Add(referral);

            // Show narrative
            _messageSystem.AddSystemMessage(
                $"🤝 {npc.Name}: \"{referral.IntroductionMessage}\"",
                SystemMessageTypes.Success
            );

            _messageSystem.AddSystemMessage(
                $"You've received a letter of introduction to {referral.TargetNPCName}!",
                SystemMessageTypes.Info
            );
        }

        return referral;
    }

    /// <summary>
    /// Generate a network referral to another NPC.
    /// </summary>
    private NetworkReferral GenerateReferral(NPC referringNPC, ConnectionType tokenType)
    {
        // Find NPCs who can offer letters of the same type
        List<NPC> allNPCs = _npcRepository.GetAllNPCs();
        List<NPC> eligibleTargets = allNPCs
            .Where(n => n.ID != referringNPC.ID)
            .Where(n => n.LetterTokenTypes.Contains(tokenType))
            .ToList();

        if (!eligibleTargets.Any())
            return null;

        NPC targetNPC = eligibleTargets[_random.Next(eligibleTargets.Count)];

        // Create introduction messages based on token type
        string[] introMessages = tokenType switch
        {
            ConnectionType.Trust => new[]
            {
                $"My friend {targetNPC.Name} has been looking for a reliable courier.",
                $"{targetNPC.Name} is good people - tell them I sent you.",
                $"I've known {targetNPC.Name} for years. They'll treat you right."
            },
            ConnectionType.Commerce => new[]
            {
                $"{targetNPC.Name} runs a solid business and needs courier services.",
                $"I do regular trade with {targetNPC.Name} - profitable connection.",
                $"{targetNPC.Name} pays well for prompt deliveries."
            },
            ConnectionType.Status => new[]
            {
                $"{targetNPC.Name} serves the same circles and requires discretion.",
                $"I can arrange an introduction to {targetNPC.Name} - quite influential.",
                $"{targetNPC.Name} appreciates proper etiquette and timely service."
            },
            ConnectionType.Trust => new[]
            {
                $"{targetNPC.Name} is one of us - always has work for honest folk.",
                $"My neighbor {targetNPC.Name} mentioned needing help.",
                $"{targetNPC.Name} takes care of those who help the community."
            },
            ConnectionType.Shadow => new[]
            {
                $"{targetNPC.Name}... operates in similar circles. Mention my name.",
                $"I know someone who knows {targetNPC.Name}. Tread carefully.",
                $"{targetNPC.Name} values discretion above all else."
            },
            _ => new[] { $"{targetNPC.Name} might have work for you." }
        };

        Letter referralLetter = GenerateReferralLetter(referringNPC, targetNPC, tokenType);

        return new NetworkReferral
        {
            Id = Guid.NewGuid().ToString(),
            ReferringNPCId = referringNPC.ID,
            ReferringNPCName = referringNPC.Name,
            TargetNPCId = targetNPC.ID,
            TargetNPCName = targetNPC.Name,
            TokenType = tokenType,
            IntroductionMessage = introMessages[_random.Next(introMessages.Length)],
            ReferralLetter = referralLetter,
            ExpiresDay = _gameWorld.CurrentDay + 7, // Referrals last 7 days
            IsUsed = false
        };
    }

    /// <summary>
    /// Generate the actual referral letter to deliver.
    /// </summary>
    private Letter GenerateReferralLetter(NPC referrer, NPC target, ConnectionType tokenType)
    {
        // Use network referral templates
        string templateIds = tokenType switch
        {
            ConnectionType.Trust => "network_referral_trust",
            ConnectionType.Commerce => "network_referral_trade",
            _ => "introduction_letter"
        };

        LetterTemplate template = _letterTemplateRepository.GetTemplateById(templateIds);
        if (template == null)
        {
            // Fallback template
            template = new LetterTemplate
            {
                Id = "generic_referral",
                Description = $"Letter of introduction from {referrer.Name}",
                TokenType = tokenType,
                Category = LetterCategory.Quality,
                MinDeadline = 3,
                MaxDeadline = 5,
                MinPayment = 8,
                MaxPayment = 12
            };
        }

        Letter letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = referrer.ID,
            SenderName = referrer.Name,
            RecipientId = target.ID,
            RecipientName = target.Name,
            Description = $"Introduction letter to {target.Name}",
            TokenType = tokenType,
            Payment = _random.Next(template.MinPayment, template.MaxPayment + 1),
            Deadline = _random.Next(template.MinDeadline, template.MaxDeadline + 1),
            IsGenerated = true,
            GenerationReason = "Network Referral",
            Message = $"{referrer.Name} speaks highly of your courier services and suggests we should meet."
        };

        // Add chain letter properties if template has them
        if (template.UnlocksLetterIds?.Length > 0)
        {
            letter.UnlocksLetterIds = template.UnlocksLetterIds.ToList();
        }

        return letter;
    }

    /// <summary>
    /// Use a referral when visiting the target NPC.
    /// </summary>
    public bool UseReferral(string targetNPCId, string referralId)
    {
        NetworkReferral referral = null;
        string referringNPCId = null;

        // Find the referral
        foreach (KeyValuePair<string, List<NetworkReferral>> kvp in _activeReferrals)
        {
            NetworkReferral? found = kvp.Value.FirstOrDefault(r => r.Id == referralId && r.TargetNPCId == targetNPCId);
            if (found != null)
            {
                referral = found;
                referringNPCId = kvp.Key;
                break;
            }
        }

        if (referral == null || referral.IsUsed)
            return false;

        if (_gameWorld.CurrentDay > referral.ExpiresDay)
        {
            _messageSystem.AddSystemMessage(
                "This referral has expired. Network connections fade without use.",
                SystemMessageTypes.Warning
            );
            return false;
        }

        NPC targetNPC = _npcRepository.GetById(targetNPCId);
        if (targetNPC == null)
            return false;

        // Mark as used
        referral.IsUsed = true;

        // Add initial tokens with the new NPC
        _connectionTokenManager.AddTokensToNPC(referral.TokenType, 3, targetNPCId);

        // Show introduction narrative
        _messageSystem.AddSystemMessage(
            $"🤝 {targetNPC.Name}: \"Ah, {referral.ReferringNPCName} sent you! Any friend of theirs...\"",
            SystemMessageTypes.Success
        );

        _messageSystem.AddSystemMessage(
            $"You've established a {referral.TokenType} connection with {targetNPC.Name}!",
            SystemMessageTypes.Success
        );

        // Generate immediate letter offer from the new connection
        List<LetterOffer> offers = _letterOfferService.GenerateNPCLetterOffers(targetNPCId);
        if (offers.Any())
        {
            _messageSystem.AddSystemMessage(
                $"{targetNPC.Name} has letters that need delivering!",
                SystemMessageTypes.Info
            );
        }

        return true;
    }

    /// <summary>
    /// Get active referrals for a specific NPC or all.
    /// </summary>
    public List<NetworkReferral> GetActiveReferrals(string npcId = null)
    {
        List<NetworkReferral> activeReferrals = new List<NetworkReferral>();

        if (npcId != null)
        {
            if (_activeReferrals.ContainsKey(npcId))
            {
                activeReferrals = _activeReferrals[npcId]
                    .Where(r => !r.IsUsed && _gameWorld.CurrentDay <= r.ExpiresDay)
                    .ToList();
            }
        }
        else
        {
            foreach (List<NetworkReferral> referrals in _activeReferrals.Values)
            {
                activeReferrals.AddRange(referrals
                    .Where(r => !r.IsUsed && _gameWorld.CurrentDay <= r.ExpiresDay));
            }
        }

        return activeReferrals;
    }

    /// <summary>
    /// Check if player has referral letter to deliver.
    /// </summary>
    public bool HasReferralLetter(string targetNPCId)
    {
        Letter[] queue = _letterQueueManager.GetPlayerQueue();
        return queue.Any(l => l != null && l.RecipientId == targetNPCId && l.GenerationReason == "Network Referral");
    }
}

/// <summary>
/// Represents a network referral from one NPC to another.
/// </summary>
public class NetworkReferral
{
    public string Id { get; set; }
    public string ReferringNPCId { get; set; }
    public string ReferringNPCName { get; set; }
    public string TargetNPCId { get; set; }
    public string TargetNPCName { get; set; }
    public ConnectionType TokenType { get; set; }
    public string IntroductionMessage { get; set; }
    public Letter ReferralLetter { get; set; }
    public int ExpiresDay { get; set; }
    public bool IsUsed { get; set; }
}