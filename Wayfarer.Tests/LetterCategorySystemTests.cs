using Xunit;
using System.Collections.Generic;
using System.Linq;

public class LetterCategorySystemTests
{
    private ConnectionTokenManager _tokenManager;
    private LetterCategoryService _categoryService;
    private LetterTemplateRepository _letterTemplateRepository;
    private NPCRepository _npcRepository;
    private MessageSystem _messageSystem;
    private GameWorld _gameWorld;
    private GameWorldManager _gameWorldManager;
    
    public LetterCategorySystemTests()
    {
        _gameWorldManager = TestGameWorldFactory.CreateCompleteGameWorldManager();
        _gameWorld = _gameWorldManager.GameWorld;
        
        _messageSystem = new MessageSystem(_gameWorld);
        _npcRepository = new NPCRepository(_gameWorld);
        _tokenManager = new ConnectionTokenManager(_gameWorld, _messageSystem, _npcRepository);
        _categoryService = new LetterCategoryService(_gameWorld, _tokenManager, _npcRepository, _messageSystem);
        _letterTemplateRepository = new LetterTemplateRepository(_gameWorld);
        
        CreateTestNPCsAndTemplates();
    }
    
    private void CreateTestNPCsAndTemplates()
    {
        // Create test NPC with Trust letters
        var elena = new NPC
        {
            ID = "elena_test",
            Name = "Elena",
            Profession = Professions.Scholar,
            LetterTokenTypes = new List<ConnectionType> { ConnectionType.Trust },
            Location = "town_square"
        };
        _gameWorld.NPCs.Add(elena.ID, elena);
        
        // Create test letter templates for each category
        var basicTemplate = new LetterTemplate
        {
            Id = "trust_basic",
            TokenType = ConnectionType.Trust,
            Category = LetterCategory.Basic,
            MinTokensRequired = 3,
            MinPayment = 3,
            MaxPayment = 5,
            MinDeadline = 3,
            MaxDeadline = 5,
            Description = "Basic trust letter"
        };
        
        var qualityTemplate = new LetterTemplate
        {
            Id = "trust_quality",
            TokenType = ConnectionType.Trust,
            Category = LetterCategory.Quality,
            MinTokensRequired = 5,
            MinPayment = 8,
            MaxPayment = 12,
            MinDeadline = 3,
            MaxDeadline = 5,
            Description = "Quality trust letter"
        };
        
        var premiumTemplate = new LetterTemplate
        {
            Id = "trust_premium",
            TokenType = ConnectionType.Trust,
            Category = LetterCategory.Premium,
            MinTokensRequired = 8,
            MinPayment = 15,
            MaxPayment = 20,
            MinDeadline = 3,
            MaxDeadline = 5,
            Description = "Premium trust letter"
        };
        
        _gameWorld.LetterTemplates.Add(basicTemplate.Id, basicTemplate);
        _gameWorld.LetterTemplates.Add(qualityTemplate.Id, qualityTemplate);
        _gameWorld.LetterTemplates.Add(premiumTemplate.Id, premiumTemplate);
    }
    
    [Fact]
    public void CanNPCOfferLetters_WithNoTokens_ReturnsFalse()
    {
        // Arrange
        var npcId = "elena_test";
        
        // Act
        var canOffer = _categoryService.CanNPCOfferLetters(npcId);
        
        // Assert
        Assert.False(canOffer, "NPC should not offer letters with 0 tokens");
    }
    
    [Fact]
    public void CanNPCOfferLetters_WithBasicThreshold_ReturnsTrue()
    {
        // Arrange
        var npcId = "elena_test";
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId);
        
        // Act
        var canOffer = _categoryService.CanNPCOfferLetters(npcId);
        
        // Assert
        Assert.True(canOffer, "NPC should offer letters with 3+ tokens");
    }
    
    [Fact]
    public void GetAvailableCategory_ReturnsCorrectCategory()
    {
        // Arrange
        var npcId = "elena_test";
        
        // Test Basic threshold
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId);
        var basicCategory = _categoryService.GetAvailableCategory(npcId, ConnectionType.Trust);
        Assert.Equal(LetterCategory.Basic, basicCategory);
        
        // Test Quality threshold
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 2, npcId); // Total: 5
        var qualityCategory = _categoryService.GetAvailableCategory(npcId, ConnectionType.Trust);
        Assert.Equal(LetterCategory.Quality, qualityCategory);
        
        // Test Premium threshold
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId); // Total: 8
        var premiumCategory = _categoryService.GetAvailableCategory(npcId, ConnectionType.Trust);
        Assert.Equal(LetterCategory.Premium, premiumCategory);
    }
    
    [Fact]
    public void GetAvailableTemplates_RespectsTokenThresholds()
    {
        // Arrange
        var npcId = "elena_test";
        
        // With 0 tokens - no templates
        var noTokenTemplates = _categoryService.GetAvailableTemplates(npcId, ConnectionType.Trust);
        Assert.Equal(0, noTokenTemplates.Count);
        
        // With 3 tokens - only basic templates
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId);
        var basicTemplates = _categoryService.GetAvailableTemplates(npcId, ConnectionType.Trust);
        Assert.Equal(1, basicTemplates.Count);
        Assert.Equal(LetterCategory.Basic, basicTemplates[0].Category);
        
        // With 5 tokens - basic and quality templates
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 2, npcId);
        var qualityTemplates = _categoryService.GetAvailableTemplates(npcId, ConnectionType.Trust);
        Assert.Equal(2, qualityTemplates.Count);
        Assert.True(qualityTemplates.Any(t => t.Category == LetterCategory.Basic));
        Assert.True(qualityTemplates.Any(t => t.Category == LetterCategory.Quality));
        
        // With 8 tokens - all templates
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId);
        var allTemplates = _categoryService.GetAvailableTemplates(npcId, ConnectionType.Trust);
        Assert.Equal(3, allTemplates.Count);
    }
    
    [Fact]
    public void CheckCategoryUnlock_ShowsCorrectMessages()
    {
        // Arrange
        var npcId = "elena_test";
        
        // Test Basic unlock
        _categoryService.CheckCategoryUnlock(npcId, ConnectionType.Trust, 2, 3);
        var messages = _gameWorld.SystemMessages;
        Assert.True(messages.Any(m => m.Message.Contains("now trusts you enough to offer Trust letters")));
        _gameWorld.SystemMessages.Clear();
        
        // Test Quality unlock
        _categoryService.CheckCategoryUnlock(npcId, ConnectionType.Trust, 4, 5);
        messages = _gameWorld.SystemMessages;
        Assert.True(messages.Any(m => m.Message.Contains("relationship") && m.Message.Contains("stronger")));
        _gameWorld.SystemMessages.Clear();
        
        // Test Premium unlock
        _categoryService.CheckCategoryUnlock(npcId, ConnectionType.Trust, 7, 8);
        messages = _gameWorld.SystemMessages;
        Assert.True(messages.Any(m => m.Message.Contains("most trusted associates")));
    }
    
    [Fact]
    public void GetCategoryPaymentRange_ReturnsCorrectRanges()
    {
        // Test payment ranges
        var (basicMin, basicMax) = _categoryService.GetCategoryPaymentRange(LetterCategory.Basic);
        Assert.Equal(3, basicMin);
        Assert.Equal(5, basicMax);
        
        var (qualityMin, qualityMax) = _categoryService.GetCategoryPaymentRange(LetterCategory.Quality);
        Assert.Equal(8, qualityMin);
        Assert.Equal(12, qualityMax);
        
        var (premiumMin, premiumMax) = _categoryService.GetCategoryPaymentRange(LetterCategory.Premium);
        Assert.Equal(15, premiumMin);
        Assert.Equal(20, premiumMax);
    }
    
    [Fact]
    public void LetterGeneration_RespectsCategories()
    {
        // Arrange
        var npcId = "elena_test";
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 5, npcId); // Quality threshold
        
        // Act
        var letter = _letterTemplateRepository.GenerateLetterFromNPC(npcId, "Elena", ConnectionType.Trust);
        
        // Assert
        Assert.NotNull(letter);
        Assert.True(letter.Payment >= 3); // Should be at least basic payment
        Assert.True(letter.Payment <= 12); // Should not exceed quality payment
    }
}