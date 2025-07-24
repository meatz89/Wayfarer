/// <summary>
/// Categorizes conversation choices for UI styling and behavior
/// </summary>
public enum ConversationChoiceType
{
    // Letter-related choices
    AcceptLetterOffer,
    DeclineLetterOffer,
    PurgeLetter,
    KeepLetter,
    
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