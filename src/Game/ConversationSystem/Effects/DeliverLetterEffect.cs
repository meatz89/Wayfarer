using System;

/// <summary>
/// Effect that delivers a letter from position 1 in the queue to the recipient
/// </summary>
public class DeliverLetterEffect : IMechanicalEffect
{
    private readonly string _letterId;
    private readonly LetterQueueManager _queueManager;
    private readonly ITimeManager _timeManager;
    
    public DeliverLetterEffect(string letterId, LetterQueueManager queueManager, ITimeManager timeManager)
    {
        _letterId = letterId;
        _queueManager = queueManager;
        _timeManager = timeManager;
    }
    
    public void Apply(ConversationState state)
    {
        // Deliver the letter from position 1
        bool success = _queueManager.DeliverFromPosition1();
        
        if (success)
        {
            // Mark conversation as complete after successful delivery
            // Player delivered the letter, conversation naturally ends
            state.IsConversationComplete = true;
        }
    }
    
    public string GetDescriptionForPlayer()
    {
        return "Deliver letter";
    }
}