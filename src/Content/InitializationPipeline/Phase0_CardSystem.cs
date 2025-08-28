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
            // Create parser as a temporary tool - it will be discarded after loading
            var cardParser = new ConversationCardParser(context.GetContentPath());
            
            // Load all card definitions from conversations.json
            cardParser.LoadConversationCards();
            
            // CRITICAL: Store loaded card data directly in GameWorld, not the parser
            // GameWorld is the single source of truth for all state
            context.GameWorld.CardTemplates = cardParser.GetCardTemplates();
            context.GameWorld.PersonalityMappings = cardParser.GetPersonalityMappings();
            context.GameWorld.TokenUnlocks = cardParser.GetTokenUnlocks();
            
            // Initialize static factories to read from GameWorld
            GoalCardFactory.Initialize(context.GameWorld);
            CardDeck.InitializeGameWorld(context.GameWorld);
            
            // Parser is now discarded - it was just a tool for loading
            Console.WriteLine($"  Card system initialized: {context.GameWorld.CardTemplates.Count} templates loaded into GameWorld");
        }
        catch (Exception ex)
        {
            context.Errors.Add($"Failed to initialize card system: {ex.Message}");
            throw;
        }
    }
}