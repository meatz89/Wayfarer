using System;
using System.Collections.Generic;

/// <summary>
/// Effect that delivers a letter from position 1 in the queue to the recipient.
/// This is where trust is EARNED - by keeping your word, not making promises.
/// </summary>
public class DeliverLetterEffect : IMechanicalEffect
{
    private readonly string _letterId;
    private readonly LetterQueueManager _queueManager;
    private readonly ITimeManager _timeManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly Letter _letter;
    
    public DeliverLetterEffect(
        string letterId, 
        Letter letter,
        LetterQueueManager queueManager, 
        ITimeManager timeManager,
        TokenMechanicsManager tokenManager)
    {
        _letterId = letterId;
        _letter = letter;
        _queueManager = queueManager;
        _timeManager = timeManager;
        _tokenManager = tokenManager;
    }
    
    public void Apply(ConversationState state)
    {
        // Deliver the letter from position 1
        bool success = _queueManager.DeliverFromPosition1();
        
        if (success)
        {
            // THIS is where trust is earned - by keeping your word
            // Award trust based on the difficulty of the promise kept
            int trustReward = CalculateTrustReward(_letter);
            
            // Get sender ID to award trust to the right NPC
            string senderId = GetNPCIdByName(_letter.SenderName);
            if (!string.IsNullOrEmpty(senderId))
            {
                _tokenManager.AddTokensToNPC(ConnectionType.Trust, trustReward, senderId);
            }
            
            // Mark conversation as complete after successful delivery
            // Player delivered the letter, conversation naturally ends
            state.IsConversationComplete = true;
        }
    }
    
    private int CalculateTrustReward(Letter letter)
    {
        // Base trust for keeping your word
        int baseTrust = 3;
        
        // Urgent letters (deadline < 24h) give more trust - you kept a harder promise
        if (letter.DeadlineInHours < 24)
        {
            baseTrust = 4;
        }
        
        // Critical letters (deadline < 12h) give even more
        if (letter.DeadlineInHours < 12)
        {
            baseTrust = 5;
        }
        
        return baseTrust;
    }
    
    private string GetNPCIdByName(string npcName)
    {
        // TODO: This should ideally come from NPC repository
        // For now, use the standard mapping
        return npcName?.ToLower() switch
        {
            "elena" => "elena",
            "brother marcus" => "marcus",
            "lord aldwin" => "lord_aldwin",
            "captain thorne" => "captain_thorne",
            "sister agatha" => "sister_agatha",
            _ => ""
        };
    }
    
    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        int trustReward = _letter != null ? CalculateTrustReward(_letter) : 3;
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = $"Deliver letter | +{trustReward} Trust (kept promise)",
                Category = EffectCategory.StateChange,
                LetterId = _letterId,
                TokenType = ConnectionType.Trust,
                TokenAmount = trustReward
            }
        };
    }
}