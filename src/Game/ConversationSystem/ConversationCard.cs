using System.Collections.Generic;
using Wayfarer.GameState;

/// <summary>
/// Represents a conversation option in an NPC's deck
/// Cards are added/removed through letter delivery and conversation outcomes
/// </summary>
public class ConversationCard
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Mechanical properties
    public int Difficulty { get; set; }
    public int PatienceCost { get; set; }
    public int ComfortGain { get; set; }
    
    // Requirements to play this card
    public Dictionary<ConnectionType, int> Requirements { get; set; } = new Dictionary<ConnectionType, int>();
    
    // Card category affects when it appears
    public RelationshipCardCategory Category { get; set; }
    
    // Outcomes based on success roll
    public string SuccessOutcome { get; set; }
    public string NeutralOutcome { get; set; }
    public string FailureOutcome { get; set; }
    
    // Success probability calculation based on current patience
    public int CalculateSuccessProbability(int currentPatience)
    {
        return (currentPatience - Difficulty + 5) * 12;
    }
    
    // Check if requirements are met to play this card
    public bool CanPlay(Dictionary<ConnectionType, int> currentTokens, int currentComfort = 0)
    {
        foreach (var requirement in Requirements)
        {
            if (!currentTokens.ContainsKey(requirement.Key) || 
                currentTokens[requirement.Key] < requirement.Value)
            {
                return false;
            }
        }
        return true;
    }
    
    // Check if card is available in current emotional state
    public bool IsAvailableInState(NPCEmotionalState emotionalState)
    {
        return Category switch
        {
            RelationshipCardCategory.Crisis => 
                emotionalState == NPCEmotionalState.DESPERATE || 
                emotionalState == NPCEmotionalState.ANXIOUS,
            RelationshipCardCategory.Basic => 
                emotionalState != NPCEmotionalState.HOSTILE,
            RelationshipCardCategory.Personal => 
                emotionalState != NPCEmotionalState.HOSTILE,
            RelationshipCardCategory.Special => 
                emotionalState != NPCEmotionalState.HOSTILE,
            _ => true
        };
    }
}

public enum RelationshipCardCategory
{
    Basic,      // Always available
    Personal,   // Requires relationship progress
    Crisis,     // Only when NPC is desperate/anxious
    Special     // Unlocked through specific letter deliveries
}