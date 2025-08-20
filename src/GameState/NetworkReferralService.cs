using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Game.MainSystem;
using Wayfarer.GameState;
using Wayfarer.Content;
using Wayfarer.GameState.Constants;

namespace Wayfarer.GameState
/// <summary>
/// Service for managing network referral letters - NPCs recommending other NPCs for letter opportunities.
/// This gives players agency to actively seek letters when needed.
/// </summary>
public class NetworkReferralService
{
    private readonly GameWorld _gameWorld;
    private readonly NPCRepository _npcRepository;
    private readonly ObligationQueueManager _letterQueueManager;
    private readonly TokenMechanicsManager _connectionTokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly ITimeManager _timeManager;
    private readonly Random _random = new Random();

    // Track active referrals
    private readonly Dictionary<string, List<NetworkReferral>> _activeReferrals = new();

    public NetworkReferralService(
        GameWorld gameWorld,
        NPCRepository npcRepository,
        ObligationQueueManager letterQueueManager,
        TokenMechanicsManager connectionTokenManager,
        MessageSystem messageSystem,
        ITimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _npcRepository = npcRepository;
        _letterQueueManager = letterQueueManager;
        _connectionTokenManager = connectionTokenManager;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
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
            ConnectionType.Shadow => new[]
            {
                $"{targetNPC.Name}... operates in similar circles. Mention my name.",
                $"I know someone who knows {targetNPC.Name}. Tread carefully.",
                $"{targetNPC.Name} values discretion above all else."
            },
            _ => new[] { $"{targetNPC.Name} might have work for you." }
        };

        DeliveryObligation referralDeliveryObligation = GenerateReferralLetter(referringNPC, targetNPC, tokenType);

        return new NetworkReferral
        {
            Id = Guid.NewGuid().ToString(),
            ReferringNPCId = referringNPC.ID,
            ReferringNPCName = referringNPC.Name,
            TargetNPCId = targetNPC.ID,
            TargetNPCName = targetNPC.Name,
            TokenType = tokenType,
            IntroductionMessage = introMessages[_random.Next(introMessages.Length)],
            ReferralDeliveryObligation = referralDeliveryObligation,
            ExpiresDay = _timeManager.GetCurrentDay() + 7, // Referrals last 7 days
            IsUsed = false
        };
    }

    /// <summary>
    /// Generate the actual referral letter to deliver.
    /// </summary>
    private DeliveryObligation GenerateReferralLetter(NPC referrer, NPC target, ConnectionType tokenType)
    {
        // Use network referral templates
        string templateIds = tokenType switch
        {
            ConnectionType.Trust => "network_referral_trust",
            ConnectionType.Commerce => "network_referral_trade",
            _ => "introduction_letter"
        };

        // Create referral letter directly without template lookup
        int minDeadline = 4320; // 3 days in minutes
        int maxDeadline = 7200; // 5 days in minutes
        int minPayment = 8;
        int maxPayment = 12;

        DeliveryObligation letter = new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = referrer.ID,
            SenderName = referrer.Name,
            RecipientId = target.ID,
            RecipientName = target.Name,
            Description = $"Introduction letter to {target.Name}",
            TokenType = tokenType,
            Payment = _random.Next(minPayment, maxPayment + 1),
            DeadlineInMinutes = _random.Next(minDeadline, maxDeadline + 1),
            IsGenerated = true,
            GenerationReason = "Network Referral",
            Message = $"{referrer.Name} speaks highly of your courier services and suggests we should meet."
        };

        // Network referrals don't use templates - they're direct letters

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

        if (_timeManager.GetCurrentDay() > referral.ExpiresDay)
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

        // Network referrals now only unlock the ability to request letters through conversation
        // No automatic letter generation - player must choose to request letters
        _messageSystem.AddSystemMessage(
            $"You can now request letters from {targetNPC.Name} through conversation!",
            SystemMessageTypes.Info
        );

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
                    .Where(r => !r.IsUsed && _timeManager.GetCurrentDay() <= r.ExpiresDay)
                    .ToList();
            }
        }
        else
        {
            foreach (List<NetworkReferral> referrals in _activeReferrals.Values)
            {
                activeReferrals.AddRange(referrals
                    .Where(r => !r.IsUsed && _timeManager.GetCurrentDay() <= r.ExpiresDay));
            }
        }

        return activeReferrals;
    }

    /// <summary>
    /// Check if player has referral letter to deliver.
    /// </summary>
    public bool HasReferralLetter(string targetNPCId)
    {
        DeliveryObligation[] queue = _letterQueueManager.GetPlayerQueue();
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
    public DeliveryObligation ReferralDeliveryObligation { get; set; }
    public int ExpiresDay { get; set; }
    public bool IsUsed { get; set; }
}
}