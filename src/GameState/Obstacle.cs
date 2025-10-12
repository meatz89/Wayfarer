/// <summary>
/// Obstacle - Strategic information entity representing challenges that multiple tactical approaches can address
/// Lives on Route/Location/NPC entities as List&lt;Obstacle&gt;
/// Properties compose through simple addition (multiple obstacles sum their properties)
/// Design principle: Obstacles are INFORMATION for player decision-making, NOT mechanical modifiers
/// </summary>
public class Obstacle
{
    /// <summary>
    /// Narrative identifier for this obstacle
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// What player sees and understands about this obstacle
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Bodily harm risk - combat, falling, traps, structural hazards
    /// Natural meaning: actual physical danger in game world
    /// </summary>
    public int PhysicalDanger { get; set; }

    /// <summary>
    /// Cognitive load - puzzle difficulty, pattern obscurity, evidence volume
    /// Natural meaning: actual mental challenge complexity
    /// </summary>
    public int MentalComplexity { get; set; }

    /// <summary>
    /// Interpersonal challenge - suspicious NPC, hostile faction, complex negotiation
    /// Natural meaning: actual social barrier difficulty
    /// Note: NPCs can ONLY have SocialDifficulty obstacles (other properties = 0)
    /// </summary>
    public int SocialDifficulty { get; set; }

    /// <summary>
    /// Physical exertion required - distance, terrain difficulty, labor intensity
    /// Natural meaning: actual stamina expenditure
    /// </summary>
    public int StaminaCost { get; set; }

    /// <summary>
    /// Real-time duration - waiting, traveling, careful work
    /// Natural meaning: actual time passage in game segments
    /// </summary>
    public int TimeCost { get; set; }

    /// <summary>
    /// Whether obstacle persists when all properties reach zero
    /// false: Removed when cleared (investigation obstacles, quest obstacles)
    /// true: Persists even at zero, can increase again (weather obstacles, patrol obstacles)
    /// </summary>
    public bool IsPermanent { get; set; }

    /// <summary>
    /// Check if obstacle is fully cleared (all properties at or below zero)
    /// </summary>
    public bool IsCleared()
    {
        return PhysicalDanger <= 0 &&
               MentalComplexity <= 0 &&
               SocialDifficulty <= 0 &&
               StaminaCost <= 0 &&
               TimeCost <= 0;
    }

    /// <summary>
    /// Get total challenge magnitude (sum of all properties)
    /// Useful for UI display of overall difficulty
    /// </summary>
    public int GetTotalMagnitude()
    {
        return PhysicalDanger + MentalComplexity + SocialDifficulty + StaminaCost + TimeCost;
    }
}
