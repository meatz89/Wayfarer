using System.Collections.Generic;

/// <summary>
/// Effect that ends the current conversation and returns to location screen.
/// This is THE way to exit conversations - used everywhere for consistency.
/// </summary>
public class EndConversationEffect : IMechanicalEffect
{
    public EndConversationEffect()
    {
        // No parameters needed - ending a conversation is ending a conversation
    }

    public void Apply(ConversationState state)
    {
        // Mark conversation as complete
        // The UI will detect this and navigate back to LocationScreen
        state.IsConversationComplete = true;
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = "End conversation",
                Category = EffectCategory.StateChange
            }
        };
    }
}