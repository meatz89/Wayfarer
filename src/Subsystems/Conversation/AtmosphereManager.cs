using System;

// AtmosphereManager handles persistence and effects of conversation atmospheres
// CRITICAL: Atmosphere persists until changed by card or cleared by failure - NEVER resets on LISTEN
public class AtmosphereManager
{
    private AtmosphereType currentAtmosphere = AtmosphereType.Neutral;
    private bool atmosphereSet = false;
    
    // One-time effect flags for special atmospheres
    private bool nextCardAutoSucceeds = false; // For Informed atmosphere
    private bool nextEffectDoubled = false; // For Synchronized atmosphere
    private bool nextSpeakFree = false; // For observation effects
    private bool nextActionFreePatience = false; // For observation effects

    public AtmosphereType CurrentAtmosphere => currentAtmosphere;
    public bool HasActiveAtmosphere => currentAtmosphere != AtmosphereType.Neutral;

    // Set new atmosphere (from card effects or observation cards)
    public void SetAtmosphere(AtmosphereType atmosphere)
    {
        currentAtmosphere = atmosphere;
        atmosphereSet = true;
        
        // Handle one-time effect atmospheres
        if (atmosphere == AtmosphereType.Informed)
        {
            nextCardAutoSucceeds = true;
        }
        else if (atmosphere == AtmosphereType.Synchronized)
        {
            nextEffectDoubled = true;
        }
    }

    // Clear atmosphere on failure (resets to Neutral)
    public void ClearAtmosphereOnFailure()
    {
        currentAtmosphere = AtmosphereType.Neutral;
        atmosphereSet = false;
        ClearTemporaryEffects();
    }
    
    // Called after successful card play to consume one-time effects
    public void OnCardSuccess()
    {
        // Consume auto-success if it was used
        if (nextCardAutoSucceeds)
        {
            nextCardAutoSucceeds = false;
            // Informed atmosphere consumed after use
            if (currentAtmosphere == AtmosphereType.Informed)
            {
                currentAtmosphere = AtmosphereType.Neutral;
            }
        }
        
        // Consume double effect if it was used
        if (nextEffectDoubled)
        {
            nextEffectDoubled = false;
            // Synchronized consumed after use
            if (currentAtmosphere == AtmosphereType.Synchronized)
            {
                currentAtmosphere = AtmosphereType.Neutral;
            }
        }
    }
    
    // CRITICAL: LISTEN does NOT reset atmosphere - atmosphere persists
    public void OnListenAction()
    {
        // DO NOTHING - atmosphere persists through LISTEN
        // This is critical for strategic setup
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
        if (currentAtmosphere == AtmosphereType.Patient || nextActionFreePatience)
        {
            nextActionFreePatience = false; // Consume one-time effect
            return true;
        }
        return false;
    }

    // Check if card should auto-succeed (bypasses dice roll)
    public bool ShouldAutoSucceed()
    {
        if (nextCardAutoSucceeds)
        {
            return true;
        }
        return false;
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
        if (nextEffectDoubled)
        {
            return true;
        }
        return false;
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
    
    // Special observation effects
    public void SetNextSpeakFree()
    {
        nextSpeakFree = true;
    }
    
    public void SetNextActionFreePatience()
    {
        nextActionFreePatience = true;
    }
    
    public bool IsNextSpeakFree()
    {
        bool wasFree = nextSpeakFree;
        nextSpeakFree = false; // Consume
        return wasFree;
    }
    
    // Check if we have temporary effects active
    public bool HasTemporaryEffects()
    {
        return nextCardAutoSucceeds || nextEffectDoubled || nextSpeakFree || nextActionFreePatience;
    }
    
    // Get description of temporary effects for UI
    public string GetTemporaryEffectsDescription()
    {
        if (nextCardAutoSucceeds)
            return "⚡ Next card guaranteed success!";
        if (nextEffectDoubled)
            return "⚡ Next effect will happen twice!";
        if (nextSpeakFree)
            return "⚡ Next SPEAK costs 0 weight!";
        if (nextActionFreePatience)
            return "⚡ Next action costs 0 patience!";
        return "";
    }
    
    private void ClearTemporaryEffects()
    {
        nextCardAutoSucceeds = false;
        nextEffectDoubled = false;
        nextSpeakFree = false;
        nextActionFreePatience = false;
    }

    // Reset atmosphere (for new conversation)
    public void Reset()
    {
        currentAtmosphere = AtmosphereType.Neutral;
        atmosphereSet = false;
        ClearTemporaryEffects();
    }
}