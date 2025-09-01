using System;
using System.Linq;

/// <summary>
/// Phase 8: Initialize the letter queue with starting letters for the mockup UI
/// </summary>
public class Phase8_InitialLetters : IInitializationPhase
{
    public int PhaseNumber => 8;
    public string Name => "Initial Letters";
    public bool IsCritical => false;

    public void Execute(InitializationContext context)
    {
        Console.WriteLine("Initializing letter queue from NPC data...");

        GameWorld gameWorld = context.GameWorld;
        List<DeliveryObligation> obligations = new System.Collections.Generic.List<DeliveryObligation>();

        // Load initial obligations from NPCs with letter cards
        // NPCs with letter cards in their deck should have them offered initially
        // This is now handled through the conversation system when player talks to them
        // We don't pre-load letters into the queue anymore - they must be negotiated first

        // Additional letters can be defined in letters.json and referenced from NPCs
        // This ensures all content is data-driven, not hardcoded

        // Add letters directly to the player's queue
        Player player = gameWorld.GetPlayer();
        if (player != null && player.ObligationQueue != null)
        {
            // Add obligations to their positions (queue already initialized in Player constructor)
            foreach (DeliveryObligation obligation in obligations)
            {
                if (obligation.QueuePosition > 0 && obligation.QueuePosition <= 8)
                {
                    player.ObligationQueue[obligation.QueuePosition - 1] = obligation;
                }
            }

            Console.WriteLine($"  Added {obligations.Count} initial obligations to player's queue from NPC data");
        }
        else
        {
            context.Warnings.Add("Could not add initial letters - player or queue not initialized");
            Console.WriteLine("  WARNING: Could not add initial letters - player or queue not initialized");
        }
    }
}