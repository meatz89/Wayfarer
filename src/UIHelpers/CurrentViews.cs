public enum CurrentViews
{
    // PRIMARY GAMEPLAY SCREENS (Letter Queue Focus)
    LetterQueueScreen,      // Primary interface - 8-slot queue management
    LetterBoardScreen,      // Morning letter selection and acceptance
    RelationshipScreen,     // Character relationships and connection tokens
    ObligationsScreen,      // Standing obligations and permanent modifiers
    
    // LOCATION AND NAVIGATION SCREENS
    LocationScreen,         // Current location with NPCs and facilities
    MapScreen,              // World map for route planning
    TravelScreen,           // Active travel between locations
    
    // ACTIVITY SCREENS (Supporting Letter Delivery)
    MarketScreen,           // Trading items for letter delivery needs
    RestScreen,             // Rest and recovery management
    
    // EVENT AND NARRATIVE SCREENS
    ConversationScreen,     // Conversations with NPCs
    NarrativeScreen,        // Story events and conversations
    
    // CHARACTER MANAGEMENT SCREENS
    CharacterScreen,        // Character sheet and inventory
    PlayerStatusScreen,     // Player stats and condition
    
    // SYSTEM SCREENS
    EventLogScreen,         // Event log showing all system messages
    MissingReferences       // Error screen for missing content
}