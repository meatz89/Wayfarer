/// <summary>
/// Types of consumable items that can clear player states
/// Used by StateClearingBehavior to specify which items can clear which states
/// </summary>
public enum ItemType
{
/// <summary>
/// Medical supplies (bandages, medicine, etc.) - clears physical injury states
/// </summary>
Medical,

/// <summary>
/// Food items - clears hunger and some exhaustion states
/// </summary>
Food,

/// <summary>
/// Remedies and potions - clears mental/social states
/// </summary>
Remedy,

/// <summary>
/// Travel provisions - clears exhaustion and hunger during travel
/// </summary>
Provisions,

/// <summary>
/// Generic consumable - general purpose
/// </summary>
Consumable
}
