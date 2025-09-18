using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.TokenSubsystem;

/// <summary>
/// Manages stranger conversations which provide practice XP and immediate rewards
/// Strangers don't provide tokens but offer scaled XP and basic resources
/// </summary>
public class StrangerConversationManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;

    public StrangerConversationManager(GameWorld gameWorld, MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Prepare data for GameFacade to start a stranger conversation
    /// </summary>
    public StrangerConversationData PrepareStrangerConversation(string strangerId, string conversationType)
    {
        StrangerNPC stranger = _gameWorld.GetStrangerById(strangerId);
        if (stranger == null)
            throw new ArgumentException($"Stranger with ID '{strangerId}' not found");

        if (stranger.HasBeenTalkedTo)
            throw new InvalidOperationException($"Stranger '{stranger.Name}' has already been talked to this time block");

        TimeBlock currentTimeBlock = _gameWorld.GetCurrentTimeBlock();
        if (!stranger.IsAvailableAtTime(currentTimeBlock))
            throw new InvalidOperationException($"Stranger '{stranger.Name}' is not available at time {currentTimeBlock}");

        // Get the conversation configuration
        StrangerConversation conversation = stranger.GetAvailableConversation(conversationType);
        if (conversation == null)
            throw new ArgumentException($"Conversation type '{conversationType}' not available for stranger '{stranger.Name}'");

        // Create a temporary NPC representation for the stranger
        NPC tempNPC = CreateTemporaryNPCFromStranger(stranger);

        // Return data for GameFacade to use
        return new StrangerConversationData
        {
            Stranger = stranger,
            TempNPC = tempNPC,
            ConversationType = ConversationType.Stranger,
            StrangerLevel = stranger.Level,
            Conversation = conversation
        };
    }

    /// <summary>
    /// Complete a stranger conversation and prepare reward data
    /// </summary>
    public StrangerConversationResult CompleteStrangerConversation(ConversationSession session, int finalRapport)
    {
        if (!session.IsStrangerConversation)
            throw new ArgumentException("Session is not a stranger conversation");

        StrangerNPC stranger = _gameWorld.GetStrangerById(session.NPC.ID);
        if (stranger == null)
            throw new ArgumentException($"Stranger with ID '{session.NPC.ID}' not found");

        // Mark stranger as talked to
        stranger.MarkAsTalkedTo();

        // Remove temporary NPC from GameWorld
        NPC? tempNPC = _gameWorld.NPCs.FirstOrDefault(n => n.ID == session.NPC.ID);
        if (tempNPC != null)
        {
            _gameWorld.NPCs.Remove(tempNPC);
        }

        // Determine which reward tier based on rapport achieved
        string conversationType = stranger.ConversationTypes.First().Key; // Simplified - should match actual type used
        StrangerConversation conversation = stranger.ConversationTypes[conversationType];

        int rewardTier = DetermineRewardTier(conversation.RapportThresholds, finalRapport);

        StrangerReward reward = null;
        if (rewardTier >= 0 && rewardTier < conversation.Rewards.Count)
        {
            reward = conversation.Rewards[rewardTier];
        }

        return new StrangerConversationResult
        {
            Success = rewardTier >= 0,
            RewardTier = rewardTier,
            Reward = reward,
            XPGranted = CalculateXPGranted(session),
            StrangerName = stranger.Name,
            Stranger = stranger,
            RewardData = reward
        };
    }

    /// <summary>
    /// Check if a stranger is available for conversation
    /// </summary>
    public bool IsStrangerAvailable(string strangerId)
    {
        StrangerNPC stranger = _gameWorld.GetStrangerById(strangerId);
        if (stranger == null) return false;

        TimeBlock currentTimeBlock = _gameWorld.GetCurrentTimeBlock();
        return stranger.IsAvailableAtTime(currentTimeBlock) && !stranger.HasBeenTalkedTo;
    }

    /// <summary>
    /// Get all available strangers at a location
    /// </summary>
    public List<StrangerNPC> GetAvailableStrangersAtLocation(string locationId)
    {
        TimeBlock currentTimeBlock = _gameWorld.GetCurrentTimeBlock();
        return _gameWorld.GetAvailableStrangers(locationId, currentTimeBlock);
    }

    /// <summary>
    /// Get conversation preview for a stranger
    /// </summary>
    public StrangerConversationPreview GetConversationPreview(string strangerId, string conversationType)
    {
        StrangerNPC stranger = _gameWorld.GetStrangerById(strangerId);
        if (stranger == null) return null;

        StrangerConversation conversation = stranger.GetAvailableConversation(conversationType);
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


    private int CalculateXPGranted(ConversationSession session)
    {
        // Calculate total XP granted during the conversation
        // This would be tracked during card plays in ConversationFacade
        return session.TurnHistory.Count * (session.StrangerLevel ?? 1);
    }
}

/// <summary>
/// Data for GameFacade to start a stranger conversation
/// </summary>
public class StrangerConversationData
{
    public StrangerNPC Stranger { get; set; }
    public NPC TempNPC { get; set; }
    public ConversationType ConversationType { get; set; }
    public int StrangerLevel { get; set; }
    public StrangerConversation Conversation { get; set; }
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
    public StrangerNPC Stranger { get; set; }
    public StrangerReward RewardData { get; set; }
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