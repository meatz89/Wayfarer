
/// <summary>
/// Classification of what happens when player selects an NPC action
/// Determines routing to appropriate subsystem
/// </summary>
public enum NPCActionType
{
/// <summary>
/// Start a Social challenge (conversation session)
/// Routes to SocialFacade.StartConversation()
/// Launches tactical conversation system with card play
/// </summary>
StartConversation,

/// <summary>
/// Start a ConversationTree (branching dialogue)
/// Routes to ConversationTreeFacade.CreateConversationTreeContext()
/// Launches narrative dialogue system with choice nodes
/// </summary>
StartConversationTree,

/// <summary>
/// Start an Exchange session (resource trading)
/// Routes to ExchangeFacade.CreateExchangeSession()
/// Launches transaction screen for buying/selling/services
/// </summary>
StartExchange,

/// <summary>
/// Initiate a Situation (strategic action with consequences)
/// Routes to SituationFacade.SelectAndExecuteSituation()
/// May lead to challenge or instant resolution
/// </summary>
InitiateSituation,

/// <summary>
/// Instant effect (apply consequences immediately)
/// No subsystem routing, direct consequence application
/// Used for simple NPC interactions (gift giving, information sharing)
/// </summary>
Instant
}
