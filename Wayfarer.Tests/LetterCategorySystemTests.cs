using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class LetterCategorySystemTests : TestFixtureBase
{
    private ConnectionTokenManager _tokenManager;
    private LetterCategoryService _categoryService;
    private LetterTemplateRepository _letterTemplateRepository;
    private NPCRepository _npcRepository;
    private MessageSystem _messageSystem;
    
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        
        _npcRepository = new NPCRepository(TestGameWorld);
        _messageSystem = new MessageSystem();
        _tokenManager = new ConnectionTokenManager(TestGameWorld, _messageSystem, _npcRepository);
        _categoryService = new LetterCategoryService(TestGameWorld, _tokenManager, _npcRepository, _messageSystem);
        _letterTemplateRepository = new LetterTemplateRepository(TestGameWorld);
        
        // Wire up services
        _tokenManager.SetCategoryService(_categoryService);
        _letterTemplateRepository.SetCategoryService(_categoryService);
        
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
        TestGameWorld.WorldState.NPCs.Add(elena);
        
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
        
        TestGameWorld.WorldState.LetterTemplates.Add(basicTemplate);
        TestGameWorld.WorldState.LetterTemplates.Add(qualityTemplate);
        TestGameWorld.WorldState.LetterTemplates.Add(premiumTemplate);
    }
    
    [Test]
    public void CanNPCOfferLetters_WithNoTokens_ReturnsFalse()
    {
        // Arrange
        var npcId = "elena_test";
        
        // Act
        var canOffer = _categoryService.CanNPCOfferLetters(npcId);
        
        // Assert
        Assert.IsFalse(canOffer, "NPC should not offer letters with 0 tokens");
    }
    
    [Test]
    public void CanNPCOfferLetters_WithBasicThreshold_ReturnsTrue()
    {
        // Arrange
        var npcId = "elena_test";
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId);
        
        // Act
        var canOffer = _categoryService.CanNPCOfferLetters(npcId);
        
        // Assert
        Assert.IsTrue(canOffer, "NPC should offer letters with 3+ tokens");
    }
    
    [Test]
    public void GetAvailableCategory_ReturnsCorrectCategory()
    {
        // Arrange
        var npcId = "elena_test";
        
        // Test Basic threshold
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId);
        var basicCategory = _categoryService.GetAvailableCategory(npcId, ConnectionType.Trust);
        Assert.AreEqual(LetterCategory.Basic, basicCategory);
        
        // Test Quality threshold
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 2, npcId); // Total: 5
        var qualityCategory = _categoryService.GetAvailableCategory(npcId, ConnectionType.Trust);
        Assert.AreEqual(LetterCategory.Quality, qualityCategory);
        
        // Test Premium threshold
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId); // Total: 8
        var premiumCategory = _categoryService.GetAvailableCategory(npcId, ConnectionType.Trust);
        Assert.AreEqual(LetterCategory.Premium, premiumCategory);
    }
    
    [Test]
    public void GetAvailableTemplates_RespectsTokenThresholds()
    {
        // Arrange
        var npcId = "elena_test";
        
        // With 0 tokens - no templates
        var noTokenTemplates = _categoryService.GetAvailableTemplates(npcId, ConnectionType.Trust);
        Assert.AreEqual(0, noTokenTemplates.Count);
        
        // With 3 tokens - only basic templates
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId);
        var basicTemplates = _categoryService.GetAvailableTemplates(npcId, ConnectionType.Trust);
        Assert.AreEqual(1, basicTemplates.Count);
        Assert.AreEqual(LetterCategory.Basic, basicTemplates[0].Category);
        
        // With 5 tokens - basic and quality templates
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 2, npcId);
        var qualityTemplates = _categoryService.GetAvailableTemplates(npcId, ConnectionType.Trust);
        Assert.AreEqual(2, qualityTemplates.Count);
        Assert.IsTrue(qualityTemplates.Any(t => t.Category == LetterCategory.Basic));
        Assert.IsTrue(qualityTemplates.Any(t => t.Category == LetterCategory.Quality));
        
        // With 8 tokens - all templates
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 3, npcId);
        var allTemplates = _categoryService.GetAvailableTemplates(npcId, ConnectionType.Trust);
        Assert.AreEqual(3, allTemplates.Count);
    }
    
    [Test]
    public void CheckCategoryUnlock_ShowsCorrectMessages()
    {
        // Arrange
        var npcId = "elena_test";
        var messages = new List<string>();
        _messageSystem.OnSystemMessage += (msg, type) => messages.Add(msg);
        
        // Test Basic unlock
        _categoryService.CheckCategoryUnlock(npcId, ConnectionType.Trust, 2, 3);
        Assert.IsTrue(messages.Any(m => m.Contains("now trusts you enough to offer Trust letters")));
        
        // Test Quality unlock
        messages.Clear();
        _categoryService.CheckCategoryUnlock(npcId, ConnectionType.Trust, 4, 5);
        Assert.IsTrue(messages.Any(m => m.Contains("relationship") && m.Contains("stronger")));
        
        // Test Premium unlock
        messages.Clear();
        _categoryService.CheckCategoryUnlock(npcId, ConnectionType.Trust, 7, 8);
        Assert.IsTrue(messages.Any(m => m.Contains("most trusted associates")));
    }
    
    [Test]
    public void GetCategoryPaymentRange_ReturnsCorrectRanges()
    {
        // Test payment ranges
        var (basicMin, basicMax) = _categoryService.GetCategoryPaymentRange(LetterCategory.Basic);
        Assert.AreEqual(3, basicMin);
        Assert.AreEqual(5, basicMax);
        
        var (qualityMin, qualityMax) = _categoryService.GetCategoryPaymentRange(LetterCategory.Quality);
        Assert.AreEqual(8, qualityMin);
        Assert.AreEqual(12, qualityMax);
        
        var (premiumMin, premiumMax) = _categoryService.GetCategoryPaymentRange(LetterCategory.Premium);
        Assert.AreEqual(15, premiumMin);
        Assert.AreEqual(20, premiumMax);
    }
    
    [Test]
    public void LetterGeneration_RespectsCategories()
    {
        // Arrange
        var npcId = "elena_test";
        _tokenManager.AddTokensToNPC(ConnectionType.Trust, 5, npcId); // Quality threshold
        
        // Act
        var letter = _letterTemplateRepository.GenerateLetterFromNPC(npcId, "Elena", ConnectionType.Trust);
        
        // Assert
        Assert.IsNotNull(letter);
        Assert.GreaterOrEqual(letter.Payment, 3); // Should be at least basic payment
        Assert.LessOrEqual(letter.Payment, 12); // Should not exceed quality payment
    }
}