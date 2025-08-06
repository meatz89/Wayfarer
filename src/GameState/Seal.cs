using System;

public enum SealType
{
    Commerce,   // Merchant guilds, trade associations
    Status,     // Noble houses, court positions
    Shadow      // Criminal networks, underground guilds
    // No Trust seals - too personal for formal recognition
}

public enum SealTier
{
    Apprentice = 1,   // Basic recognition
    Journeyman = 2,   // Established member
    Master = 3        // Full authority
}

public class Seal
{
    public string Id { get; set; }
    public string Name { get; set; }
    public SealType Type { get; set; }
    public SealTier Tier { get; set; }
    public string IssuingGuildId { get; set; }  // Which guild hall issued this
    public int DayIssued { get; set; }
    public string Description { get; set; }
    
    // Visual/flavor properties
    public string Material { get; set; } = "Bronze";  // Bronze/Silver/Gold based on tier
    public string Insignia { get; set; }  // Visual description
    
    public string GetFullName()
    {
        return $"{Tier} {Type} Seal";
    }
    
    public bool MeetsRequirement(SealType requiredType, SealTier minimumTier)
    {
        return Type == requiredType && Tier >= minimumTier;
    }
}