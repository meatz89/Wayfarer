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
        // Since LetterQueueManager is a service, we'll just create the letters here
        // and they'll be picked up when the game starts

        // Create the 5 letters from the mockup
        List<Letter> letters = new System.Collections.Generic.List<Letter>();

        // 1. Elena's marriage refusal (URGENT - 2h 15m)
        Letter elenaLetter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "elena",
            SenderName = "Elena",
            RecipientId = "lord_aldwin",
            RecipientName = "Lord Aldwin",
            Description = "Elena's refusal of Lord Aldwin's marriage proposal",
            TokenType = ConnectionType.Trust,
            Stakes = StakeType.SAFETY, // Changed to SAFETY to trigger DESPERATE state
            Size = SizeCategory.Small,
            DeadlineInHours = 1, // URGENT! Less than 2 hours makes Elena DESPERATE
            QueuePosition = 1,
            State = LetterState.Collected,
            Payment = 0
        };
        letters.Add(elenaLetter);

        // 2. Lord Blackwood's urgent letter (position 2)
        Letter lordBLetter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "lord_b",
            SenderName = "Lord Blackwood",
            RecipientId = "noble_district",
            RecipientName = "Noble District",
            Description = "Lord Blackwood's urgent correspondence",
            TokenType = ConnectionType.Status,
            Stakes = StakeType.REPUTATION,
            Size = SizeCategory.Small,
            DeadlineInHours = 48, // ~2 days (the one with deadline warning)
            QueuePosition = 2,
            State = LetterState.Collected,
            Payment = 10
        };
        letters.Add(lordBLetter);

        // 3. Marcus's trade deal (position 3)
        Letter marcusLetter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "marcus",
            SenderName = "Marcus",
            RecipientId = "merchant_row",
            RecipientName = "Merchant Row",
            Description = "Marcus's urgent trade correspondence",
            TokenType = ConnectionType.Commerce,
            Stakes = StakeType.WEALTH,
            Size = SizeCategory.Medium,
            DeadlineInHours = 72, // ~3 days
            QueuePosition = 3,
            State = LetterState.Collected,
            Payment = 5
        };
        letters.Add(marcusLetter);

        // 4. Viktor's report (position 5 - after Marcus's 2-slot letter)
        Letter viktorLetter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "viktor",
            SenderName = "Viktor",
            RecipientId = "noble_district",
            RecipientName = "Noble District",
            Description = "Guard Captain Viktor's security report",
            TokenType = ConnectionType.Status,
            Stakes = StakeType.SAFETY,
            Size = SizeCategory.Small,
            DeadlineInHours = 144, // ~6 days
            QueuePosition = 5,
            State = LetterState.Collected,
            Payment = 3
        };
        letters.Add(viktorLetter);

        // 5. Garrett's package (position 6)
        Letter garrettLetter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "garrett",
            SenderName = "Garrett",
            RecipientId = "riverside",
            RecipientName = "Riverside",
            Description = "Garrett's mysterious package",
            TokenType = ConnectionType.Shadow,
            Stakes = StakeType.SECRET,
            Size = SizeCategory.Large,
            DeadlineInHours = 288, // ~12 days
            QueuePosition = 6,
            State = LetterState.Collected,
            Payment = 15
        };
        letters.Add(garrettLetter);

        // Add letters directly to the player's queue
        Player player = gameWorld.GetPlayer();
        if (player != null && player.LetterQueue != null)
        {
            // Add letters to their positions (queue already initialized in Player constructor)
            foreach (Letter letter in letters)
            {
                if (letter.QueuePosition > 0 && letter.QueuePosition <= 8)
                {
                    player.LetterQueue[letter.QueuePosition - 1] = letter;
                }
            }

            Console.WriteLine($"  Added {letters.Count} initial letters to player's queue");
            Console.WriteLine($"  - Elena's marriage refusal (pos 1)");
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