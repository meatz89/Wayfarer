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

        // Check for NPCs in crisis state who might have urgent meetings
        var urgentNpc = gameWorld.WorldState.NPCs.FirstOrDefault(n => n.IsInCrisis());
        if (urgentNpc != null && gameWorld.GetPlayer() != null)
        {
            // Create meeting obligation for urgent NPC
            MeetingObligation urgentMeeting = new MeetingObligation
            {
                Id = $"{urgentNpc.ID}_urgent_meeting",
                RequesterId = urgentNpc.ID,
                RequesterName = urgentNpc.Name,
                DeadlineInMinutes = 73,  // 1h 13m - DESPERATE state (< 2 hours with SAFETY stakes)
                Stakes = StakeType.SAFETY,
                Reason = "Family safety matter - urgent letter delivery needed"
            };
            
            gameWorld.GetPlayer().MeetingObligations.Add(urgentMeeting);
            Console.WriteLine($"  Added {urgentNpc.Name}'s urgent meeting request (2 hour deadline)");
        }
        
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