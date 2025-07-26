using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Constants;
/// <summary>
/// Service for managing patron letters - mysterious gold-sealed letters that jump to queue positions 1-3.
/// These represent the core tension: are you an agent with purpose or just a pawn being used?
/// </summary>
public class PatronLetterService
{
    private readonly GameWorld _gameWorld;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly MessageSystem _messageSystem;
    private readonly Random _random = new Random();

    // Track last patron letter generation
    private int _lastPatronLetterDay = -7; // Allow immediate generation on game start
    private readonly int _minDaysBetweenPatronLetters = 5;
    private readonly int _maxDaysBetweenPatronLetters = 10;

    // Patron letter template IDs
    private readonly string[] _patronLetterTemplateIds = new[]
    {
        "patron_letter_resources",
        "patron_letter_instructions",
        "forced_patron_resources",
        "forced_patron_instructions",
        "forced_patron_summons"
    };

    private readonly ConnectionTokenManager _tokenManager;

    public PatronLetterService(
        GameWorld gameWorld,
        LetterQueueManager letterQueueManager,
        LetterTemplateRepository letterTemplateRepository,
        MessageSystem messageSystem,
        ConnectionTokenManager tokenManager)
    {
        _gameWorld = gameWorld;
        _letterQueueManager = letterQueueManager;
        _letterTemplateRepository = letterTemplateRepository;
        _messageSystem = messageSystem;
        _tokenManager = tokenManager;
    }

    /// <summary>
    /// Check if it's time to generate a patron letter.
    /// Called during morning phase or specific time blocks.
    /// </summary>
    public bool ShouldGeneratePatronLetter()
    {
        int currentDay = _gameWorld.CurrentDay;
        int daysSinceLastLetter = currentDay - _lastPatronLetterDay;

        // Check if enough time has passed
        if (daysSinceLastLetter < _minDaysBetweenPatronLetters)
            return false;

        // Increasing chance as more days pass
        int chancePer100 = Math.Min(90, 10 + (daysSinceLastLetter - _minDaysBetweenPatronLetters) * 15);

        return _random.Next(100) < chancePer100;
    }

    /// <summary>
    /// Initialize patron relationship with extreme debt for leverage.
    /// Called when patron obligation is applied (e.g., Day 10 tutorial).
    /// </summary>
    public void InitializePatronDebt()
    {
        // Check if patron debt already exists
        var patronTokens = _tokenManager.GetTokensWithNPC("patron");
        if (patronTokens[ConnectionType.Noble] >= GameConstants.Patron.PATRON_DEBT_THRESHOLD)
        {
            // Set extreme debt to create natural leverage
            // This debt makes patron letters naturally reach positions 1-3
            _tokenManager.AddTokensToNPC(ConnectionType.Noble, GameConstants.Patron.INITIAL_PATRON_DEBT, "patron");
            
            _messageSystem.AddSystemMessage(
                "Your patron has saved you from destitution - but at a steep price.",
                SystemMessageTypes.Warning
            );
            _messageSystem.AddSystemMessage(
                $"Your debt of {Math.Abs(GameConstants.Patron.INITIAL_PATRON_DEBT)} Noble tokens gives them extreme leverage over your priorities.",
                SystemMessageTypes.Info
            );
            _messageSystem.AddSystemMessage(
                "Their letters will naturally demand top positions in your queue.",
                SystemMessageTypes.Info
            );
        }
    }

    /// <summary>
    /// Generate a patron letter that uses leverage for queue position.
    /// These letters create immediate priority crises through debt.
    /// </summary>
    public Letter GeneratePatronLetter()
    {
        // Select a patron letter template
        string templateId = _patronLetterTemplateIds[_random.Next(_patronLetterTemplateIds.Length)];
        LetterTemplate template = _letterTemplateRepository.GetTemplateById(templateId);

        if (template == null)
        {
            // Fallback to generic patron letter if template missing
            template = new LetterTemplate
            {
                Id = "patron_generic",
                Description = "Urgent instructions from your patron",
                TokenType = ConnectionType.Noble,
                Category = LetterCategory.Premium,
                MinDeadline = 2,
                MaxDeadline = 4,
                MinPayment = 60,
                MaxPayment = 100,
                MinTokensRequired = 1
            };
        }

        // Generate narrative names for patron letters
        string[] patronSenders = new[] { "Your Patron", "Patron's Secretary", "House Steward", "Patron's Voice" };
        string[] patronRecipients = new[] { "Field Agent", "Local Contact", "Resource Master", "Field Commander", "Supply Coordinator" };

        string sender = patronSenders[_random.Next(patronSenders.Length)];
        string recipient = patronRecipients[_random.Next(patronRecipients.Length)];

        Letter letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "patron", // Special ID for patron
            IsFromPatron = true, // Mark as patron letter for special messaging
            SenderName = sender,
            RecipientId = $"patron_contact_{_random.Next(1, 10)}", // Various patron contacts
            RecipientName = recipient,
            Description = template.Description,
            TokenType = template.TokenType,
            Payment = _random.Next(template.MinPayment, template.MaxPayment + 1),
            Deadline = _random.Next(template.MinDeadline, template.MaxDeadline + 1),
            DaysInQueue = 0,
            IsGenerated = true,
            GenerationReason = "Patron Directive"
        };

        // Update tracking
        _lastPatronLetterDay = _gameWorld.CurrentDay;

        return letter;
    }

    /// <summary>
    /// Add a patron letter to the queue with special handling.
    /// </summary>
    public bool AddPatronLetterToQueue(Letter patronLetter)
    {
        if (!patronLetter.IsFromPatron)
            return false;

        // Show dramatic arrival message
        _messageSystem.AddSystemMessage(
            "📜 A gold-sealed letter arrives from your patron!",
            SystemMessageTypes.Warning
        );

        _messageSystem.AddSystemMessage(
            "Your extreme debt gives this letter overwhelming leverage in your queue.",
            SystemMessageTypes.Warning
        );

        // Add mysterious patron messages
        string[] messages = new[]
        {
            "You wonder about your patron's true intentions...",
            "The weight of your patron's expectations bears down on you.",
            "Another test? Or genuine need? You can never tell.",
            "Your patron's timing is, as always, inconvenient.",
            "The gold seal gleams with unspoken authority."
        };

        _messageSystem.AddSystemMessage(
            messages[_random.Next(messages.Length)],
            SystemMessageTypes.Info
        );

        // Use leverage-based queue positioning for patron letters
        int position = _letterQueueManager.AddLetterWithObligationEffects(patronLetter);
        return position > 0;
    }

    /// <summary>
    /// Generate periodic patron letters based on game state.
    /// </summary>
    public Letter CheckForPatronLetter()
    {
        // Ensure patron debt is initialized for leverage
        InitializePatronDebt();
        
        if (ShouldGeneratePatronLetter())
        {
            Letter patronLetter = GeneratePatronLetter();
            AddPatronLetterToQueue(patronLetter);
            return patronLetter;
        }
        return null;
    }

    /// <summary>
    /// Force generate a patron letter (for testing or specific events).
    /// </summary>
    public void ForcePatronLetter()
    {
        Letter patronLetter = GeneratePatronLetter();
        AddPatronLetterToQueue(patronLetter);
    }

    /// <summary>
    /// Get narrative description for patron relationships.
    /// </summary>
    public string GetPatronNarrative()
    {
        string[] narratives = new[]
        {
            "Your patron remains a mystery, communicating only through sealed letters.",
            "Gold-sealed letters arrive without warning, disrupting your carefully planned routes.",
            "You've never met your patron, only their representatives and contacts.",
            "Each patron letter brings resources, but at what cost to your other obligations?",
            "Are you a trusted agent, or merely a convenient pawn in a larger game?"
        };

        return narratives[_random.Next(narratives.Length)];
    }
}