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
            // Create and initialize the card parser
            var cardParser = new ConversationCardParser(context.GetContentPath());
            
            // Load all card definitions from conversations.json
            cardParser.LoadConversationCards();
            
            // Initialize static factories with the parser
            GoalCardFactory.Initialize(cardParser);
            CardDeck.InitializeCardParser(cardParser);
            
            // Store parser in shared data for other phases
            context.SharedData["CardParser"] = cardParser;
            
            Console.WriteLine("  Card system initialized successfully");
        }
        catch (Exception ex)
        {
            context.Errors.Add($"Failed to initialize card system: {ex.Message}");
            throw;
        }
    }
}