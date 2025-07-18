using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class RelationshipDamageTests
    {
        private readonly GameWorld _gameWorld;
        private readonly LetterQueueManager _letterQueueManager;
        private readonly ConnectionTokenManager _tokenManager;
        private readonly MessageSystem _messageSystem;
        
        public RelationshipDamageTests()
        {
            // Create test world
            // Create factories needed for GameWorldInitializer
            var locationFactory = new LocationFactory();
            var locationSpotFactory = new LocationSpotFactory();
            var npcFactory = new NPCFactory();
            var itemFactory = new ItemFactory();
            var routeFactory = new RouteFactory();
            var routeDiscoveryFactory = new RouteDiscoveryFactory();
            var networkUnlockFactory = new NetworkUnlockFactory();
            var letterTemplateFactory = new LetterTemplateFactory();
            var standingObligationFactory = new StandingObligationFactory();
            var actionDefinitionFactory = new ActionDefinitionFactory();
            
            var contentDirectory = new ContentDirectory { Path = "Content" };
            var gameWorldInitializer = new GameWorldInitializer(
                contentDirectory,
                locationFactory,
                locationSpotFactory,
                npcFactory,
                itemFactory,
                routeFactory,
                routeDiscoveryFactory,
                networkUnlockFactory,
                letterTemplateFactory,
                standingObligationFactory,
                actionDefinitionFactory);
            _gameWorld = gameWorldInitializer.LoadGame();
            
            // Create repositories
            var letterTemplateRepository = new LetterTemplateRepository(_gameWorld);
            var npcRepository = new NPCRepository(_gameWorld);
            _messageSystem = new MessageSystem();
            
            // Create managers
            var narrativeService = new NarrativeService(npcRepository);
            _tokenManager = new ConnectionTokenManager(_gameWorld, _messageSystem, npcRepository);
            var obligationManager = new StandingObligationManager(_gameWorld, _messageSystem, letterTemplateRepository, _tokenManager);
            _letterQueueManager = new LetterQueueManager(_gameWorld, letterTemplateRepository, npcRepository, _messageSystem, obligationManager, _tokenManager);
        }
        
        [Fact]
        public void ExpiredLetter_RemovesTokensFromNPC()
        {
            // Arrange
            var player = _gameWorld.GetPlayer();
            
            // Create test NPCs
            var elena = new NPC { ID = "elena_test", Name = "Elena" };
            _gameWorld.WorldState.NPCs.Add(elena);
            
            // Give player some tokens with Elena
            _tokenManager.AddTokens(ConnectionType.Trust, 5, elena.ID);
            
            // Add a letter that will expire
            var letter = new Letter
            {
                SenderName = "Elena",
                RecipientName = "Marcus",
                Deadline = 1,  // Will expire on next day
                Payment = 5,
                TokenType = ConnectionType.Trust
            };
            _letterQueueManager.AddLetterToFirstEmpty(letter);
            
            // Act - Process daily deadlines (letter expires)
            _letterQueueManager.ProcessDailyDeadlines();
            
            // Assert
            var npcTokens = _tokenManager.GetTokensWithNPC(elena.ID);
            Assert.Equal(3, npcTokens[ConnectionType.Trust]); // Started with 5, lost 2
            Assert.Equal(3, _tokenManager.GetTokenCount(ConnectionType.Trust)); // Player total also reduced
        }
        
        [Fact]
        public void ExpiredLetter_CanGoNegativeWithNPC()
        {
            // Arrange
            var player = _gameWorld.GetPlayer();
            
            // Create test NPC
            var merchantGuild = new NPC { ID = "merchant_guild_test", Name = "Merchant Guild" };
            _gameWorld.WorldState.NPCs.Add(merchantGuild);
            
            // Give player only 1 token with the NPC
            _tokenManager.AddTokens(ConnectionType.Trade, 1, merchantGuild.ID);
            
            // Add a letter that will expire
            var letter = new Letter
            {
                SenderName = "Merchant Guild",
                RecipientName = "Dock Master",
                Deadline = 1,
                Payment = 8,
                TokenType = ConnectionType.Trade
            };
            _letterQueueManager.AddLetterToFirstEmpty(letter);
            
            // Act - Process daily deadlines (letter expires)
            _letterQueueManager.ProcessDailyDeadlines();
            
            // Assert
            var npcTokens = _tokenManager.GetTokensWithNPC(merchantGuild.ID);
            Assert.Equal(-1, npcTokens[ConnectionType.Trade]); // Started with 1, lost 2 = -1
            Assert.Equal(0, _tokenManager.GetTokenCount(ConnectionType.Trade)); // Player total can't go below 0
        }
        
        [Fact]
        public void ExpiredLetter_GeneratesWarningMessage()
        {
            // Arrange
            // Create test NPC
            var lordAshford = new NPC { ID = "lord_ashford_test", Name = "Lord Ashford" };
            _gameWorld.WorldState.NPCs.Add(lordAshford);
            
            var letter = new Letter
            {
                SenderName = "Lord Ashford",
                RecipientName = "Lady Winters",
                Deadline = 1,
                Payment = 12,
                TokenType = ConnectionType.Noble
            };
            _letterQueueManager.AddLetterToFirstEmpty(letter);
            
            // Act
            _letterQueueManager.ProcessDailyDeadlines();
            
            // Assert
            var messages = _messageSystem.GetAndClearChanges();
            Assert.Contains(messages.SystemMessages, m => 
                m.Message.Contains("Letter from Lord Ashford expired") && 
                m.Type == SystemMessageTypes.Danger);
        }
        
        [Fact]
        public void NonExpiredLetter_NoRelationshipDamage()
        {
            // Arrange
            var player = _gameWorld.GetPlayer();
            
            // Create test NPC
            var anonymous = new NPC { ID = "anonymous_test", Name = "Anonymous" };
            _gameWorld.WorldState.NPCs.Add(anonymous);
            
            // Give player some tokens
            _tokenManager.AddTokens(ConnectionType.Shadow, 3, anonymous.ID);
            
            // Add a letter with 3 days deadline
            var letter = new Letter
            {
                SenderName = "Anonymous",
                RecipientName = "The Fence",
                Deadline = 3,
                Payment = 15,
                TokenType = ConnectionType.Shadow
            };
            _letterQueueManager.AddLetterToFirstEmpty(letter);
            
            // Act - Process one day
            _letterQueueManager.ProcessDailyDeadlines();
            
            // Assert
            var npcTokens = _tokenManager.GetTokensWithNPC(anonymous.ID);
            Assert.Equal(3, npcTokens[ConnectionType.Shadow]); // No tokens lost
            Assert.Equal(2, letter.Deadline); // Deadline decreased but not expired
        }
    }