/// <summary>
/// The 5 core conversation resources that cards can manipulate.
/// Each stat maps to one primary resource.
/// </summary>
public enum ConversationResourceType
{
    /// <summary>Initiative - Action economy within turn (Cunning stat)</summary>
    Initiative,

    /// <summary>Momentum - Goal progress (Authority stat)</summary>
    Momentum,

    /// <summary>Doubt - Failure timer (Commerce stat reduces)</summary>
    Doubt,

    /// <summary>Cadence - Conversation rhythm/balance (Rapport stat)</summary>
    Cadence,

    /// <summary>Cards - Information/options (Insight stat)</summary>
    Cards
}