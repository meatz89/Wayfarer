using System;

// AtmosphereManager handles persistence and effects of conversation atmospheres
public class AtmosphereManager
{
    private ConversationAtmosphere currentAtmosphere = ConversationAtmosphere.Neutral;
    private bool atmosphereSet = false;

    public ConversationAtmosphere CurrentAtmosphere => currentAtmosphere;
    public bool HasActiveAtmosphere => currentAtmosphere != ConversationAtmosphere.Neutral;

    // Set new atmosphere (from card effects or observation cards)
    public void SetAtmosphere(ConversationAtmosphere atmosphere)
    {
        currentAtmosphere = atmosphere;
        atmosphereSet = true;
    }

    // Clear atmosphere on failure (resets to Neutral)
    public void ClearAtmosphereOnFailure()
    {
        currentAtmosphere = ConversationAtmosphere.Neutral;
        atmosphereSet = false;
    }

    // Get weight capacity bonus from atmosphere
    public int GetWeightCapacityBonus()
    {
        return currentAtmosphere == ConversationAtmosphere.Prepared ? 1 : 0;
    }

    // Get draw count modifier from atmosphere
    public int GetDrawCountModifier()
    {
        return currentAtmosphere switch
        {
            ConversationAtmosphere.Receptive => 1,
            ConversationAtmosphere.Pressured => -1,
            _ => 0
        };
    }

    // Get success percentage bonus
    public int GetSuccessPercentageBonus()
    {
        return currentAtmosphere == ConversationAtmosphere.Focused ? 20 : 0;
    }

    // Check if next action should have zero patience cost
    public bool ShouldWaivePatienceCost()
    {
        return currentAtmosphere == ConversationAtmosphere.Patient;
    }

    // Check if card should auto-succeed (bypasses dice roll)
    public bool ShouldAutoSucceed()
    {
        return currentAtmosphere == ConversationAtmosphere.Informed;
    }

    // Modify comfort change based on atmosphere
    public int ModifyComfortChange(int baseComfort)
    {
        int modified = baseComfort;

        // Volatile: ±1 to all changes
        if (currentAtmosphere == ConversationAtmosphere.Volatile)
        {
            if (baseComfort > 0)
                modified = baseComfort + 1;
            else if (baseComfort < 0)
                modified = baseComfort - 1;
            // Zero stays zero
        }

        // Exposed: Double all changes
        if (currentAtmosphere == ConversationAtmosphere.Exposed)
        {
            modified = baseComfort * 2;
        }

        return modified;
    }

    // Check if failure should end conversation immediately
    public bool ShouldEndOnFailure()
    {
        return currentAtmosphere == ConversationAtmosphere.Final;
    }

    // Check if next effect should happen twice
    public bool ShouldDoubleNextEffect()
    {
        return currentAtmosphere == ConversationAtmosphere.Synchronized;
    }

    // Get atmosphere description for UI
    public string GetConversationAtmosphereDescription()
    {
        return currentAtmosphere switch
        {
            ConversationAtmosphere.Neutral => "No special effects",
            ConversationAtmosphere.Prepared => "+1 weight capacity on all SPEAK actions",
            ConversationAtmosphere.Receptive => "+1 card on all LISTEN actions",
            ConversationAtmosphere.Focused => "+20% success on all cards",
            ConversationAtmosphere.Patient => "All actions cost 0 patience",
            ConversationAtmosphere.Volatile => "All comfort changes ±1",
            ConversationAtmosphere.Final => "Any failure ends conversation immediately",
            ConversationAtmosphere.Informed => "Next card cannot fail (automatic success)",
            ConversationAtmosphere.Exposed => "Double all comfort changes",
            ConversationAtmosphere.Synchronized => "Next card effect happens twice",
            ConversationAtmosphere.Pressured => "-1 card on all LISTEN actions",
            _ => "Unknown atmosphere"
        };
    }

    // Check if atmosphere is observation-only (can only be set by observation cards)
    public bool IsObservationOnlyConversationAtmosphere(ConversationAtmosphere atmosphere)
    {
        return atmosphere switch
        {
            ConversationAtmosphere.Informed => true,
            ConversationAtmosphere.Exposed => true,
            ConversationAtmosphere.Synchronized => true,
            ConversationAtmosphere.Pressured => true,
            _ => false
        };
    }

    // Reset atmosphere (for new conversation)
    public void Reset()
    {
        currentAtmosphere = ConversationAtmosphere.Neutral;
        atmosphereSet = false;
    }
}