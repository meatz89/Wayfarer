using System;
using System.Collections.Generic;

/// <summary>
/// Effect for refusing a letter already in the queue.
/// This is a heavy decision - burning bridges to survive.
/// The player is choosing immediate relief over long-term relationships.
/// </summary>
public class RefuseLetterEffect : IMechanicalEffect
{
    private readonly string _letterId;
    private readonly string _senderId;
    private readonly string _senderName;
    private readonly LetterQueueManager _queueManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly int _trustPenalty = 3; // Heavy cost - this is betrayal

    public RefuseLetterEffect(
        string letterId,
        string senderId,
        string senderName,
        LetterQueueManager queueManager,
        TokenMechanicsManager tokenManager)
    {
        _letterId = letterId;
        _senderId = senderId;
        _senderName = senderName;
        _queueManager = queueManager;
        _tokenManager = tokenManager;
    }

    public void Apply(ConversationState state)
    {
        // Find the letter's position
        int? position = _queueManager.GetLetterPosition(_letterId);
        if (!position.HasValue)
        {
            throw new InvalidOperationException($"Letter {_letterId} not found in queue - cannot refuse what you don't have");
        }

        // Remove the letter from queue - this is the mechanical relief
        _queueManager.RemoveLetterFromQueue(position.Value);

        // Burn trust - this is the emotional cost
        // You're not just returning a letter, you're breaking a promise
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Trust, _trustPenalty, _senderId);

        // This creates a permanent scar in the relationship
        // The NPC will remember this betrayal
        Player player = state.Player;
        if (!player.NPCLetterHistory.ContainsKey(_senderId))
        {
            player.NPCLetterHistory[_senderId] = new LetterHistory();
        }

        // Record this as a special type of failure - worse than expiry
        // You CHOSE to break your word, rather than failing to keep it
        player.NPCLetterHistory[_senderId].RecordRefusal();
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription>
        {
            new MechanicalEffectDescription
            {
                Text = $"REFUSE {_senderName}'s letter | Burn 3 Trust (permanent damage)",
                Category = EffectCategory.LetterRemove,
                TokenType = ConnectionType.Trust,
                TokenAmount = _trustPenalty,
                NpcId = _senderId,
                LetterId = _letterId
            }
        };
    }
}