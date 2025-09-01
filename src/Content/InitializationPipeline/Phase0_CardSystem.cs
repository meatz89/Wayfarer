using System;
using System.IO;

/// <summary>
/// Phase 0: Initialize the card system from JSON
/// Must run before any other phases that create cards
/// </summary>
public class Phase0_CardSystem : IInitializationPhase
{
    public int PhaseNumber => 0;
    public string Name => "Card System Initialization";
    public bool IsCritical => true;

    public void Execute(InitializationContext context)
    {
        Console.WriteLine("Initializing card system from JSON...");

        // Load all card data using the new CardDatabase system
        var cardDatabase = CardDatabase.LoadFromJson(context.GetContentPath());
        
        // Store card database reference in context for other phases
        context.CardDatabase = cardDatabase;
        
        // Convert to legacy format for backwards compatibility (temporary)
        // This will be removed when all systems are updated to use CardDatabase directly
        context.GameWorld.AllCardDefinitions = new System.Collections.Generic.Dictionary<string, ConversationCard>();
        
        // Add all base deck cards to AllCardDefinitions
        foreach (var category in cardDatabase.BaseDeck)
        {
            foreach (var card in category.Value)
            {
                context.GameWorld.AllCardDefinitions[card.Id] = card;
            }
        }
        
        // Add NPC special cards
        foreach (var npcCards in cardDatabase.NPCSpecialCards)
        {
            foreach (var card in npcCards.Value)
            {
                context.GameWorld.AllCardDefinitions[card.Id] = card;
            }
        }
        
        // Initialize empty legacy collections (will be populated in Phase3)
        context.GameWorld.NPCConversationDeckMappings = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
        context.GameWorld.NPCGoalDecks = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ConversationCard>>();
        context.GameWorld.NPCExchangeDecks = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ConversationCard>>();
        context.GameWorld.PlayerObservationCards = new System.Collections.Generic.List<ConversationCard>();
        context.GameWorld.TravelCards = new System.Collections.Generic.List<TravelCard>();
        
        // Add observation cards to PlayerObservationCards
        foreach (var location in cardDatabase.LocationObservations)
        {
            foreach (var contextCards in location.Value)
            {
                foreach (var obsCard in contextCards.Value)
                {
                    // Convert ObservationCard to ConversationCard for legacy compatibility
                    var convCard = new ConversationCard
                    {
                        Id = obsCard.Id,
                        Name = obsCard.Name,
                        Description = obsCard.Description,
                        DialogueFragment = obsCard.DialogueFragment,
                        TokenType = obsCard.TokenType,
                        Weight = obsCard.Weight,
                        Difficulty = obsCard.Difficulty,
                        Type = CardType.Observation,
                        IsObservation = true,
                        Persistence = PersistenceType.Persistent
                    };
                    context.GameWorld.PlayerObservationCards.Add(convCard);
                    context.GameWorld.AllCardDefinitions[obsCard.Id] = convCard;
                }
            }
        }

        // Initialize static factories to read from GameWorld
        GoalCardFactory.Initialize(context.GameWorld);
        CardDeck.InitializeGameWorld(context.GameWorld);

        // Report loading statistics
        Console.WriteLine($"[Phase0] Card system initialized from JSON database:");
        Console.WriteLine($"  - Total cards loaded: {context.GameWorld.AllCardDefinitions.Count}");
        Console.WriteLine($"  - Base deck categories: {cardDatabase.BaseDeck.Count}");
        Console.WriteLine($"  - NPCs with special cards: {cardDatabase.NPCSpecialCards.Count}");
        Console.WriteLine($"  - Locations with observations: {cardDatabase.LocationObservations.Count}");
        Console.WriteLine($"  - NPCs with goals: {cardDatabase.NPCGoals.Count}");
    }
}