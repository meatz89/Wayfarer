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

        try
        {
            // PHASE 1: Load comprehensive deck system (POC architecture)
            var deckLoader = new CardDeckLoader(context.GetContentPath());
            deckLoader.LoadAllDecks();
            
            // Store all deck configurations in GameWorld (single source of truth)
            context.GameWorld.AllCardDefinitions = deckLoader.GetAllCards();
            context.GameWorld.NPCConversationDeckMappings = deckLoader.GetNPCConversationDecks();
            context.GameWorld.NPCGoalDecks = deckLoader.GetNPCGoalDecks();
            context.GameWorld.NPCExchangeDecks = deckLoader.GetNPCExchangeDecks();
            context.GameWorld.PlayerObservationCards = deckLoader.GetPlayerObservationCards();
            
            // Initialize static factories to read from GameWorld
            GoalCardFactory.Initialize(context.GameWorld);
            CardDeck.InitializeGameWorld(context.GameWorld);
            
            // Report loading statistics
            Console.WriteLine($"[Phase0] Card system initialized:");
            Console.WriteLine($"  - Total cards: {context.GameWorld.AllCardDefinitions.Count}");
            Console.WriteLine($"  - NPC conversation decks: {context.GameWorld.NPCConversationDeckMappings.Count}");
            Console.WriteLine($"  - NPC goal decks: {context.GameWorld.NPCGoalDecks.Count}");
            Console.WriteLine($"  - NPC exchange decks: {context.GameWorld.NPCExchangeDecks.Count}");
            Console.WriteLine($"  - Player observation cards: {context.GameWorld.PlayerObservationCards.Count}");
        }
        catch (Exception ex)
        {
            context.Errors.Add($"Failed to initialize card system: {ex.Message}");
            throw;
        }
    }
}