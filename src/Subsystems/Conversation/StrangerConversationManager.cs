using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages stranger conversations which provide practice XP and immediate rewards
/// Strangers don't provide tokens but offer scaled XP and basic resources
/// </summary>
public class StrangerConversationManager
{
    private readonly GameWorld _gameWorld;
    private readonly ConversationOrchestrator _conversationOrchestrator;

    public StrangerConversationManager(GameWorld gameWorld, ConversationOrchestrator conversationOrchestrator)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _conversationOrchestrator = conversationOrchestrator ?? throw new ArgumentNullException(nameof(conversationOrchestrator));
    }

    /// <summary>
    /// Start a conversation with a stranger NPC
    /// </summary>
    public ConversationSession StartStrangerConversation(string strangerId, string conversationType)
    {
        var stranger = _gameWorld.GetStrangerById(strangerId);
        if (stranger == null)
            throw new ArgumentException($"Stranger with ID '{strangerId}' not found");

        if (stranger.HasBeenTalkedTo)
            throw new InvalidOperationException($"Stranger '{stranger.Name}' has already been talked to this time block");

        var currentTimeBlock = _gameWorld.GetCurrentTimeBlock();
        if (!stranger.IsAvailableAtTime(currentTimeBlock))
            throw new InvalidOperationException($"Stranger '{stranger.Name}' is not available at time {currentTimeBlock}");

        // Get the conversation configuration
        var conversation = stranger.GetAvailableConversation(conversationType);
        if (conversation == null)
            throw new ArgumentException($"Conversation type '{conversationType}' not available for stranger '{stranger.Name}'");

        // Create a temporary NPC representation for the stranger
        var tempNPC = CreateTemporaryNPCFromStranger(stranger);

        // Create conversation session using orchestrator
        var session = _conversationOrchestrator.CreateSession(tempNPC, ConversationType.Stranger);

        // Mark as stranger conversation and set level for XP scaling
        session.IsStrangerConversation = true;
        session.StrangerLevel = stranger.Level;

        return session;
    }

    /// <summary>
    /// Complete a stranger conversation and apply rewards
    /// </summary>
    public StrangerConversationResult CompleteStrangerConversation(ConversationSession session, int finalRapport)
    {
        if (!session.IsStrangerConversation)
            throw new ArgumentException("Session is not a stranger conversation");

        var stranger = _gameWorld.GetStrangerById(session.NPC.ID);
        if (stranger == null)
            throw new ArgumentException($"Stranger with ID '{session.NPC.ID}' not found");

        // Mark stranger as talked to
        stranger.MarkAsTalkedTo();

        // Determine which reward tier based on rapport achieved
        var conversationType = stranger.ConversationTypes.First().Key; // Simplified - should match actual type used
        var conversation = stranger.ConversationTypes[conversationType];

        int rewardTier = DetermineRewardTier(conversation.RapportThresholds, finalRapport);

        StrangerReward reward = null;
        if (rewardTier >= 0 && rewardTier < conversation.Rewards.Count)
        {
            reward = conversation.Rewards[rewardTier];
            ApplyStrangerReward(reward);
        }

        return new StrangerConversationResult
        {
            Success = rewardTier >= 0,
            RewardTier = rewardTier,
            Reward = reward,
            XPGranted = CalculateXPGranted(session),
            StrangerName = stranger.Name
        };
    }

    /// <summary>
    /// Check if a stranger is available for conversation
    /// </summary>
    public bool IsStrangerAvailable(string strangerId)
    {
        var stranger = _gameWorld.GetStrangerById(strangerId);
        if (stranger == null) return false;

        var currentTimeBlock = _gameWorld.GetCurrentTimeBlock();
        return stranger.IsAvailableAtTime(currentTimeBlock) && !stranger.HasBeenTalkedTo;
    }

    /// <summary>
    /// Get all available strangers at a location
    /// </summary>
    public List<StrangerNPC> GetAvailableStrangersAtLocation(string locationId)
    {
        var currentTimeBlock = _gameWorld.GetCurrentTimeBlock();
        return _gameWorld.GetAvailableStrangers(locationId, currentTimeBlock);
    }

    /// <summary>
    /// Get conversation preview for a stranger
    /// </summary>
    public StrangerConversationPreview GetConversationPreview(string strangerId, string conversationType)
    {
        var stranger = _gameWorld.GetStrangerById(strangerId);
        if (stranger == null) return null;

        var conversation = stranger.GetAvailableConversation(conversationType);
        if (conversation == null) return null;

        return new StrangerConversationPreview
        {
            StrangerName = stranger.Name,
            ConversationType = conversationType,
            RapportThresholds = conversation.RapportThresholds,
            Rewards = conversation.Rewards,
            XPMultiplier = stranger.GetXPMultiplier(),
            Personality = stranger.Personality
        };
    }

    private NPC CreateTemporaryNPCFromStranger(StrangerNPC stranger)
    {
        // Create a minimal NPC representation for conversation mechanics
        return new NPC
        {
            ID = stranger.Id,
            Name = stranger.Name,
            ConversationModifier = new PersonalityModifier
            {
                Type = GetPersonalityModifierType(stranger.Personality)
            }
        };
    }

    private PersonalityModifierType GetPersonalityModifierType(PersonalityType personality)
    {
        return personality switch
        {
            PersonalityType.PROUD => PersonalityModifierType.AscendingFocusRequired,
            PersonalityType.DEVOTED => PersonalityModifierType.RapportLossMultiplier,
            PersonalityType.MERCANTILE => PersonalityModifierType.HighestFocusBonus,
            PersonalityType.CUNNING => PersonalityModifierType.RepeatFocusPenalty,
            PersonalityType.STEADFAST => PersonalityModifierType.RapportChangeCap,
            _ => PersonalityModifierType.None
        };
    }

    private int DetermineRewardTier(List<int> thresholds, int finalRapport)
    {
        for (int i = thresholds.Count - 1; i >= 0; i--)
        {
            if (finalRapport >= thresholds[i])
                return i;
        }
        return -1; // No reward tier achieved
    }

    private void ApplyStrangerReward(StrangerReward reward)
    {
        var player = _gameWorld.GetPlayer();

        // Apply immediate rewards
        if (reward.Coins > 0)
            player.AddCoins(reward.Coins);

        if (reward.Health > 0)
            player.ModifyHealth(reward.Health);

        if (reward.Food > 0)
            player.ReduceHunger(reward.Food);

        // TODO: Apply other rewards (items, permits, observations, tokens)
        // These would need additional systems to handle properly
    }

    private int CalculateXPGranted(ConversationSession session)
    {
        // Calculate total XP granted during the conversation
        // This would be tracked during card plays in ConversationOrchestrator
        return session.TurnHistory.Count * (session.StrangerLevel ?? 1);
    }
}

/// <summary>
/// Result of a completed stranger conversation
/// </summary>
public class StrangerConversationResult
{
    public bool Success { get; set; }
    public int RewardTier { get; set; }
    public StrangerReward Reward { get; set; }
    public int XPGranted { get; set; }
    public string StrangerName { get; set; }
}

/// <summary>
/// Preview information for a stranger conversation
/// </summary>
public class StrangerConversationPreview
{
    public string StrangerName { get; set; }
    public string ConversationType { get; set; }
    public List<int> RapportThresholds { get; set; }
    public List<StrangerReward> Rewards { get; set; }
    public int XPMultiplier { get; set; }
    public PersonalityType Personality { get; set; }
}