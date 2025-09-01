using System;

// AtmosphereManager handles persistence and effects of conversation atmospheres
public class AtmosphereManager
{
    private AtmosphereType currentAtmosphere = AtmosphereType.Neutral;
    private bool atmosphereSet = false;

    public AtmosphereType CurrentAtmosphere => currentAtmosphere;
    public bool HasActiveAtmosphere => currentAtmosphere != AtmosphereType.Neutral;

    // Set new atmosphere (from card effects or observation cards)
    public void SetAtmosphere(AtmosphereType atmosphere)
    {
        currentAtmosphere = atmosphere;
        atmosphereSet = true;
    }

    // Clear atmosphere on failure (resets to Neutral)
    public void ClearAtmosphereOnFailure()
    {
        currentAtmosphere = AtmosphereType.Neutral;
        atmosphereSet = false;
    }

    // Get weight capacity bonus from atmosphere
    public int GetWeightCapacityBonus()
    {
        return currentAtmosphere == AtmosphereType.Prepared ? 1 : 0;
    }

    // Get draw count modifier from atmosphere
    public int GetDrawCountModifier()
    {
        return currentAtmosphere switch
        {
            AtmosphereType.Receptive => 1,
            AtmosphereType.Pressured => -1,
            _ => 0
        };
    }

    // Get success percentage bonus
    public int GetSuccessPercentageBonus()
    {
        return currentAtmosphere == AtmosphereType.Focused ? 20 : 0;
    }

    // Check if next action should have zero patience cost
    public bool ShouldWaivePatienceCost()
    {
        return currentAtmosphere == AtmosphereType.Patient;
    }

    // Check if card should auto-succeed (bypasses dice roll)
    public bool ShouldAutoSucceed()
    {
        return currentAtmosphere == AtmosphereType.Informed;
    }

    // Modify comfort change based on atmosphere
    public int ModifyComfortChange(int baseComfort)
    {
        int modified = baseComfort;

        // Volatile: ±1 to all changes
        if (currentAtmosphere == AtmosphereType.Volatile)
        {
            if (baseComfort > 0)
                modified = baseComfort + 1;
            else if (baseComfort < 0)
                modified = baseComfort - 1;
            // Zero stays zero
        }

        // Exposed: Double all changes
        if (currentAtmosphere == AtmosphereType.Exposed)
        {
            modified = baseComfort * 2;
        }

        return modified;
    }

    // Check if failure should end conversation immediately
    public bool ShouldEndOnFailure()
    {
        return currentAtmosphere == AtmosphereType.Final;
    }

    // Check if next effect should happen twice
    public bool ShouldDoubleNextEffect()
    {
        return currentAtmosphere == AtmosphereType.Synchronized;
    }

    // Get atmosphere description for UI
    public string GetAtmosphereTypeDescription()
    {
        return currentAtmosphere switch
        {
            AtmosphereType.Neutral => "No special effects",
            AtmosphereType.Prepared => "+1 weight capacity on all SPEAK actions",
            AtmosphereType.Receptive => "+1 card on all LISTEN actions",
            AtmosphereType.Focused => "+20% success on all cards",
            AtmosphereType.Patient => "All actions cost 0 patience",
            AtmosphereType.Volatile => "All comfort changes ±1",
            AtmosphereType.Final => "Any failure ends conversation immediately",
            AtmosphereType.Informed => "Next card cannot fail (automatic success)",
            AtmosphereType.Exposed => "Double all comfort changes",
            AtmosphereType.Synchronized => "Next card effect happens twice",
            AtmosphereType.Pressured => "-1 card on all LISTEN actions",
            _ => "Unknown atmosphere"
        };
    }

    // Check if atmosphere is observation-only (can only be set by observation cards)
    public bool IsObservationOnlyAtmosphereType(AtmosphereType atmosphere)
    {
        return atmosphere switch
        {
            AtmosphereType.Informed => true,
            AtmosphereType.Exposed => true,
            AtmosphereType.Synchronized => true,
            AtmosphereType.Pressured => true,
            _ => false
        };
    }

    // Reset atmosphere (for new conversation)
    public void Reset()
    {
        currentAtmosphere = AtmosphereType.Neutral;
        atmosphereSet = false;
    }
}