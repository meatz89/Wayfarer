/// <summary>
/// Categorizes conversation choices for UI styling and behavior
/// </summary>
public enum ConversationChoiceType
{
    // Letter-related choices
    AcceptLetterOffer,
    DeclineLetterOffer,
    RequestTrustLetter,
    RequestCommerceLetter,
    RequestStatusLetter,
    RequestShadowLetter,
    PurgeLetter,
    KeepLetter,
    Deliver,  // Deliver a letter from position 1 to the recipient

    // Special letter requests (Epic 7)  
    IntroductionLetter,             // Request introduction letter (Trust tokens)
    AccessPermit,                   // Request access permit (Commerce tokens)

    // Queue manipulation choices
    SkipAndDeliver,
    RespectQueueOrder,

    // Social choices
    Introduction,

    // Discovery choices
    DiscoverRoute,

    // Travel choices
    TravelCautious,
    TravelUseEquipment,
    TravelForceThrough,
    TravelSlowProgress,
    TravelTradeHelp,
    TravelExchangeInfo,

    // Default/generic
    Default
}