public enum CurrentViews
{
    // CORE MODAL STATES (Four-Modal Focus System)
    LocationScreen,         // MAP MODE - City overview with NPC locations (default)
    ConversationScreen,     // CONVERSATION MODE - NPC interactions
    LetterQueueScreen,      // QUEUE MODE - DeliveryObligation management interface
    TravelScreen,           // ROUTE PLANNING MODE - Travel decision interface
    
    // ADDITIONAL SCREENS (Not part of core modal system)
    LetterBoardScreen,      // Morning letter selection and acceptance
    RelationshipScreen,     // Character relationships and connection tokens
    ObligationsScreen,      // Standing obligations and permanent modifiers
    MarketScreen,           // Trading items for letter delivery needs
    RestScreen,             // Rest and recovery management
    SealProgressionScreen,  // Seal and endorsement management
    NarrativeScreen,        // Story events and conversations
    CharacterScreen,        // Character sheet and inventory
    PlayerStatusScreen,     // Player stats and condition
    InformationDiscoveryScreen, // Discovered information and secrets
    EventLogScreen,         // Event log showing all system messages
    MissingReferences       // Error screen for missing content
}