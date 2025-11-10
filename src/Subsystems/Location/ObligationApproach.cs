
/// <summary>
/// Different approaches to investigate a location, unlocked by player stats
/// </summary>
public enum ObligationApproach
{
Standard,      // Always available
Systematic,    // Insight 2+: +1 familiarity
LocalInquiry,  // Rapport 2+: Learn NPC preferences
DemandAccess,  // Authority 2+: Access restricted Locations
PurchaseInfo,  // Diplomacy 2+: Pay for familiarity
CovertSearch   // Cunning 2+: No alerts
}
