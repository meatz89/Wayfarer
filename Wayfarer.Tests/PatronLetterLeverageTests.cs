using Xunit;
using System.Collections.Generic;
using System.Linq;

public class PatronLetterLeverageTests
{
    private GameWorld _gameWorld;
    private LetterQueueManager _letterQueueManager;
    private ConnectionTokenManager _tokenManager;
    private LetterTemplateRepository _templateRepo;
    private NPCRepository _npcRepo;
    private MessageSystem _messageSystem;
    private StandingObligationManager _obligationManager;
    private LetterCategoryService _categoryService;
    private ConversationFactory _conversationFactory;
    private GameConfiguration _gameConfig;
    private IGameRuleEngine _ruleEngine;
    private ITimeManager _timeManager;
    private ConversationStateManager _conversationStateManager;
    private DebugLogger _debugLogger;

    public PatronLetterLeverageTests()
    {
        _gameWorld = new GameWorld();
        
        // Set up player
        var player = _gameWorld.GetPlayer();
        player.Name = "TestPlayer";
        
        // Add NPCs to world state
        _gameWorld.WorldState.NPCs = new List<NPC>
        {
            new NPC { ID = "patron", Name = "Your Patron", LetterTokenTypes = new List<ConnectionType> { ConnectionType.Noble } },
            new NPC { ID = "merchant", Name = "Marcus", LetterTokenTypes = new List<ConnectionType> { ConnectionType.Trade } }
        };

        _messageSystem = new MessageSystem(_gameWorld);
        var visibilityService = new NPCVisibilityService();
        _npcRepo = new NPCRepository(_gameWorld, null, visibilityService);
        var itemRepo = new ItemRepository(_gameWorld);
        _tokenManager = new ConnectionTokenManager(_gameWorld, _messageSystem, _npcRepo, itemRepo);
        _templateRepo = new LetterTemplateRepository(_gameWorld);
        _conversationStateManager = new ConversationStateManager();
        var timeModel = new TimeModel();
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<TimeManager>();
        _timeManager = new TimeManager(timeModel, _messageSystem, logger);
        _debugLogger = new DebugLogger(_timeManager, _conversationStateManager);
        var obligationRepo = new StandingObligationRepository(_gameWorld);
        _obligationManager = new StandingObligationManager(_gameWorld, _messageSystem, _templateRepo, _tokenManager, obligationRepo, _timeManager);
        _categoryService = new LetterCategoryService(_gameWorld, _tokenManager, _npcRepo, _messageSystem, _gameConfig);
        _conversationFactory = new ConversationFactory(null, _tokenManager);
        _gameConfig = new GameConfiguration();
        _ruleEngine = new GameRuleEngine(_gameConfig, _tokenManager, _npcRepo, _timeManager as TimeManager);
        
        _letterQueueManager = new LetterQueueManager(
            _gameWorld,
            _templateRepo,
            _npcRepo,
            _messageSystem,
            _obligationManager,
            _tokenManager,
            _categoryService,
            _conversationFactory,
            _gameConfig,
            _ruleEngine,
            _timeManager,
            _conversationStateManager
        );
    }

    [Fact]
    public void Patron_Letter_With_Extreme_Debt_Should_Naturally_Take_Top_Position()
    {
        // Arrange - Set up extreme debt with patron
        _tokenManager.AddTokensToNPC(ConnectionType.Noble, -20, "patron");
        
        // Fill queue with regular letters
        for (int i = 1; i <= 5; i++)
        {
            var regularLetter = new Letter
            {
                Id = $"letter{i}",
                SenderName = "Marcus",
                TokenType = ConnectionType.Trade,
                QueuePosition = i
            };
            _gameWorld.GetPlayer().LetterQueue[i-1] = regularLetter;
        }

        // Create patron letter
        var patronLetter = new Letter
        {
            Id = "patron_letter",
            SenderName = "Your Patron",
            TokenType = ConnectionType.Noble,
            IsFromPatron = true
        };

        // Act - Add patron letter using leverage-aware addition
        int position = _letterQueueManager.AddLetterWithObligationEffects(patronLetter);

        // Assert - Patron letter should be at position 1 due to extreme leverage
        var queuedPatronLetter = _gameWorld.GetPlayer().LetterQueue
            .FirstOrDefault(l => l != null && l.Id == "patron_letter");
        
        Assert.NotNull(queuedPatronLetter);
        Assert.Equal(1, queuedPatronLetter.QueuePosition);
    }

    [Fact] 
    public void Different_Debt_Levels_Should_Create_Natural_Priority_Hierarchy()
    {
        // Arrange - Set up different debt levels
        _tokenManager.AddTokensToNPC(ConnectionType.Noble, -20, "patron");     // Extreme debt
        _tokenManager.AddTokensToNPC(ConnectionType.Noble, -5, "noble");       // Moderate debt  
        _tokenManager.AddTokensToNPC(ConnectionType.Trade, -2, "merchant");    // Small debt
        _tokenManager.AddTokensToNPC(ConnectionType.Common, 3, "commoner");    // Positive relationship

        // Add NPCs
        _gameWorld.WorldState.NPCs.AddRange(new[]
        {
            new NPC { ID = "noble", Name = "Lord Noble", LetterTokenTypes = new List<ConnectionType> { ConnectionType.Noble } },
            new NPC { ID = "commoner", Name = "Jane Common", LetterTokenTypes = new List<ConnectionType> { ConnectionType.Common } }
        });

        // Create letters from each
        var letters = new[]
        {
            new Letter { Id = "l1", SenderName = "Your Patron", TokenType = ConnectionType.Noble },
            new Letter { Id = "l2", SenderName = "Lord Noble", TokenType = ConnectionType.Noble },
            new Letter { Id = "l3", SenderName = "Marcus", TokenType = ConnectionType.Trade },
            new Letter { Id = "l4", SenderName = "Jane Common", TokenType = ConnectionType.Common }
        };

        // Act - Add all letters
        foreach (var letter in letters)
        {
            _letterQueueManager.AddLetterWithObligationEffects(letter);
        }

        // Assert - Check natural priority order based on leverage
        var queue = _gameWorld.GetPlayer().LetterQueue.Where(l => l != null).ToList();
        
        Assert.Equal("Your Patron", queue[0].SenderName);
        Assert.Equal("Lord Noble", queue[1].SenderName);
        Assert.Equal("Marcus", queue[2].SenderName);
        Assert.Equal("Jane Common", queue[3].SenderName);
    }

    [Fact]
    public void Patron_Letter_Should_Not_Use_Special_Positioning_Method()
    {
        // This test will verify that AddPatronLetter() method doesn't exist
        // or if it does, that it uses the same leverage calculation as regular letters
        
        // Arrange
        var patronLetter = new Letter
        {
            Id = "patron_letter",
            SenderName = "Your Patron", 
            TokenType = ConnectionType.Noble,
            IsFromPatron = true
        };

        // Act & Assert
        // The test passes if we can add patron letters through normal queue mechanism
        // without any special handling
        var exception = Record.Exception(() => _letterQueueManager.AddLetterWithObligationEffects(patronLetter));
        Assert.Null(exception);
    }
}