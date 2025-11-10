/// <summary>
/// The 6 core conversation resources that cards can manipulate.
/// Each stat maps to one or two resources.
/// </summary>
public enum SocialChallengeResourceType
{
/// <summary>Initiative - Action economy within turn (Cunning stat PRIMARY)</summary>
Initiative,

/// <summary>Momentum - Situation progress (Authority stat PRIMARY)</summary>
Momentum,

/// <summary>Doubt - Failure timer (Diplomacy stat reduces PRIMARY)</summary>
Doubt,

/// <summary>Cadence - Conversation rhythm/balance (via Delivery property, not effects)</summary>
Cadence,

/// <summary>Cards - Information/options (Insight stat PRIMARY)</summary>
Cards,

/// <summary>Understanding - Sophistication/connection depth, unlocks tiers (Rapport stat PRIMARY)</summary>
Understanding
}