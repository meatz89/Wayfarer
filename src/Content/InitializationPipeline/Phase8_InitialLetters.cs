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
        Console.WriteLine("Initializing letter queue with mockup letters...");

        GameWorld gameWorld = context.GameWorld;

        // Store initial letters in shared data for later initialization
        // Since ObligationQueueManager is a service, we'll just create the letters here
        // and they'll be picked up when the game starts

        // Create the 5 delivery obligations from the mockup
        List<DeliveryObligation> obligations = new System.Collections.Generic.List<DeliveryObligation>();

        // 1. Elena has a MEETING OBLIGATION instead of a delivery obligation
        // She summons the player urgently, and will give them the letter during conversation
        MeetingObligation elenaMeeting = new MeetingObligation
        {
            Id = "elena_urgent_meeting",
            RequesterId = "elena",
            RequesterName = "Elena",
            DeadlineInMinutes = 120,  // 2 hours - URGENT!
            Stakes = StakeType.SAFETY,
            Reason = "Family safety matter - urgent letter delivery needed"
        };
        
        // Add Elena's meeting obligation to player
        if (gameWorld.GetPlayer() != null)
        {
            gameWorld.GetPlayer().MeetingObligations.Add(elenaMeeting);
            Console.WriteLine($"  Added Elena's urgent meeting request (2 hour deadline)");
            
            // Initialize Elena's crisis deck for the urgent meeting
            var elena = gameWorld.WorldState.NPCs.FirstOrDefault(n => n.ID == "elena");
            if (elena != null)
            {
                elena.InitializeCrisisDeck();
                if (elena.CrisisDeck != null)
                {
                    // Add crisis cards for the urgent meeting
                    var crisisCard = new ConversationCard
                    {
                        Id = "elena_crisis_urgent",
                        Template = CardTemplateType.UrgentPlea,
                        Context = new CardContext 
                        { 
                            Personality = PersonalityType.DEVOTED,
                            EmotionalState = EmotionalState.DESPERATE,
                            UrgencyLevel = 3,
                            HasDeadline = true,
                            MinutesUntilDeadline = 120
                        },
                        Type = CardType.Trust,
                        Persistence = PersistenceType.Crisis,
                        Weight = 5,
                        BaseComfort = 8,
                        Category = CardCategory.CRISIS,
                        IsObservation = false,
                        CanDeliverLetter = false,
                        ManipulatesObligations = false
                    };
                    elena.CrisisDeck.AddCard(crisisCard);
                    
                    // Add another crisis card
                    var crisisCard2 = new ConversationCard
                    {
                        Id = "elena_crisis_plea",
                        Template = CardTemplateType.DesperateRequest,
                        Context = new CardContext 
                        { 
                            Personality = PersonalityType.DEVOTED,
                            EmotionalState = EmotionalState.DESPERATE,
                            UrgencyLevel = 3,
                            HasDeadline = true,
                            MinutesUntilDeadline = 120
                        },
                        Type = CardType.Trust,
                        Persistence = PersistenceType.Crisis,
                        Weight = 5,
                        BaseComfort = 6,
                        Category = CardCategory.CRISIS,
                        IsObservation = false,
                        CanDeliverLetter = false,
                        ManipulatesObligations = false
                    };
                    elena.CrisisDeck.AddCard(crisisCard2);
                    
                    Console.WriteLine($"  Added crisis cards to Elena for urgent meeting");
                }
            }
        }
        
        // Elena's actual letter will be given during conversation, not in queue initially

        // 2. Lord Blackwood's urgent letter (position 2)
        DeliveryObligation lordBDeliveryObligation = new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "lord_b",
            SenderName = "Lord Blackwood",
            RecipientId = "noble_district",
            RecipientName = "Noble District",
            Description = "Lord Blackwood's urgent correspondence",
            TokenType = ConnectionType.Status,
            Stakes = StakeType.REPUTATION,
            DeadlineInMinutes = 48, // ~2 days (the one with deadline warning)
            QueuePosition = 2,
            // State is only for physical Letters, not abstract DeliveryObligations
            Payment = 10
        };
        obligations.Add(lordBDeliveryObligation);

        // 3. Marcus's trade deal (position 3)
        DeliveryObligation marcusDeliveryObligation = new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "marcus",
            SenderName = "Marcus",
            RecipientId = "merchant_row",
            RecipientName = "Merchant Row",
            Description = "Marcus's urgent trade correspondence",
            TokenType = ConnectionType.Commerce,
            Stakes = StakeType.WEALTH,
            DeadlineInMinutes = 72, // ~3 days
            QueuePosition = 3,
            // State is only for physical Letters, not abstract DeliveryObligations
            Payment = 5
        };
        obligations.Add(marcusDeliveryObligation);

        // 4. Viktor's report (position 5 - after Marcus's 2-slot letter)
        DeliveryObligation viktorDeliveryObligation = new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "viktor",
            SenderName = "Viktor",
            RecipientId = "noble_district",
            RecipientName = "Noble District",
            Description = "Guard Captain Viktor's security report",
            TokenType = ConnectionType.Status,
            Stakes = StakeType.SAFETY,
            DeadlineInMinutes = 144, // ~6 days
            QueuePosition = 5,
            // State is only for physical Letters, not abstract DeliveryObligations
            Payment = 3
        };
        obligations.Add(viktorDeliveryObligation);

        // 5. Garrett's package (position 6)
        DeliveryObligation garrettDeliveryObligation = new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "garrett",
            SenderName = "Garrett",
            RecipientId = "riverside",
            RecipientName = "Riverside",
            Description = "Garrett's mysterious package",
            TokenType = ConnectionType.Shadow,
            Stakes = StakeType.SECRET,
            DeadlineInMinutes = 288, // ~12 days
            QueuePosition = 6,
            // State is only for physical Letters, not abstract DeliveryObligations
            Payment = 15
        };
        obligations.Add(garrettDeliveryObligation);

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

            Console.WriteLine($"  Added {obligations.Count} initial obligations to player's queue");
            Console.WriteLine($"  - Elena's meeting request (urgent - not in queue)");
            Console.WriteLine($"  - Lord Blackwood's urgent letter (pos 2)");
            Console.WriteLine($"  - Marcus's trade deal (pos 3-4)");
            Console.WriteLine($"  - Viktor's security report (pos 5)");
            Console.WriteLine($"  - Garrett's mysterious package (pos 6-8)");
        }
        else
        {
            context.Warnings.Add("Could not add initial letters - player or queue not initialized");
            Console.WriteLine("  WARNING: Could not add initial letters - player or queue not initialized");
        }
    }
}