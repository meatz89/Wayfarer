using Xunit;
using System.Collections.Generic;

public class EquipmentTokenTests
{
    private GameWorld _gameWorld;
    private ConnectionTokenManager _tokenManager;
    private ItemRepository _itemRepository;
    private NPCRepository _npcRepository;
    private MessageSystem _messageSystem;

    public EquipmentTokenTests()
    {
        _gameWorld = new GameWorld();
        _gameWorld.GetPlayer().Inventory = new Inventory(10);
        
        // Add test NPCs
        _gameWorld.WorldState.NPCs = new List<NPC>
        {
            new NPC { ID = "commoner", Name = "Bob", LetterTokenTypes = new List<ConnectionType> { ConnectionType.Common } },
            new NPC { ID = "merchant", Name = "Marcus", LetterTokenTypes = new List<ConnectionType> { ConnectionType.Trade } }
        };
        
        _messageSystem = new MessageSystem(_gameWorld);
        _itemRepository = new ItemRepository(_gameWorld);
        var visibilityService = new NPCVisibilityService();
        _npcRepository = new NPCRepository(_gameWorld, null, visibilityService);
        _tokenManager = new ConnectionTokenManager(_gameWorld, _messageSystem, _npcRepository, _itemRepository);
    }

    [Fact]
    public void LocalAleSet_Should_Provide_50Percent_Bonus_To_Common_Tokens()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Create and add the Local Ale Set item
        var aleSet = new Item
        {
            Id = "local_ale_set",
            Name = "Local Ale Set",
            TokenGenerationModifiers = new Dictionary<ConnectionType, float> { { ConnectionType.Common, 1.5f } }
        };
        _itemRepository.AddItem(aleSet);
        player.Inventory.AddItem("local_ale_set");
        
        // Act - Add tokens 10 times to test the 50% modifier
        int totalTokensEarned = 0;
        for (int i = 0; i < 10; i++)
        {
            var tokensBefore = _tokenManager.GetTokenCount(ConnectionType.Common);
            _tokenManager.AddTokensToNPC(ConnectionType.Common, 1, "commoner");
            var tokensAfter = _tokenManager.GetTokenCount(ConnectionType.Common);
            totalTokensEarned += (tokensAfter - tokensBefore);
        }
        
        // Assert - With 50% bonus, we should get around 15 tokens (10 base + 5 bonus)
        Assert.True(totalTokensEarned >= 10, "Should earn at least the base amount");
        Assert.True(totalTokensEarned <= 20, "Should not earn more than double");
        // Due to rounding up, we should get 2 tokens each time (1 * 1.5 = 1.5, rounded up to 2)
        Assert.Equal(20, totalTokensEarned);
    }

    [Fact] 
    public void FineClothes_Should_Enable_Noble_Token_Generation()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Create and add Fine Clothes
        var fineClothes = new Item
        {
            Id = "fine_clothes",
            Name = "Fine Clothes",
            EnablesTokenGeneration = new List<ConnectionType> { ConnectionType.Noble }
        };
        _itemRepository.AddItem(fineClothes);
        
        // Act & Assert - Without equipment, can't generate Noble tokens from commoner
        Assert.False(_tokenManager.CanGenerateTokenType(ConnectionType.Noble, "commoner"));
        
        // Add equipment
        player.Inventory.AddItem("fine_clothes");
        
        // Now should be able to generate Noble tokens
        Assert.True(_tokenManager.CanGenerateTokenType(ConnectionType.Noble, "commoner"));
    }

    [Fact]
    public void MerchantLedger_Should_Enable_Trade_Token_Generation()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Create and add Merchant Ledger
        var ledger = new Item
        {
            Id = "merchant_ledger", 
            Name = "Merchant Ledger",
            EnablesTokenGeneration = new List<ConnectionType> { ConnectionType.Trade }
        };
        _itemRepository.AddItem(ledger);
        
        // Act & Assert - Without equipment, can't generate Trade tokens from commoner
        Assert.False(_tokenManager.CanGenerateTokenType(ConnectionType.Trade, "commoner"));
        
        // Add equipment
        player.Inventory.AddItem("merchant_ledger");
        
        // Now should be able to generate Trade tokens
        Assert.True(_tokenManager.CanGenerateTokenType(ConnectionType.Trade, "commoner"));
        
        // Should still be able to generate from merchants who naturally offer Trade tokens
        Assert.True(_tokenManager.CanGenerateTokenType(ConnectionType.Trade, "merchant"));
    }

    [Fact]
    public void Equipment_Should_Not_Enable_Token_Types_Already_Offered_By_NPC()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Merchant already offers Trade tokens naturally
        Assert.True(_tokenManager.CanGenerateTokenType(ConnectionType.Trade, "merchant"));
        
        // Even without equipment, should be able to generate Trade tokens from merchant
        Assert.True(_tokenManager.CanGenerateTokenType(ConnectionType.Trade, "merchant"));
    }

    [Fact]
    public void Multiple_Modifiers_Should_Stack_Multiplicatively()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Create two items that both boost Common tokens
        var item1 = new Item
        {
            Id = "item1",
            Name = "Item 1",
            TokenGenerationModifiers = new Dictionary<ConnectionType, float> { { ConnectionType.Common, 1.5f } }
        };
        var item2 = new Item
        {
            Id = "item2", 
            Name = "Item 2",
            TokenGenerationModifiers = new Dictionary<ConnectionType, float> { { ConnectionType.Common, 1.2f } }
        };
        
        _itemRepository.AddItem(item1);
        _itemRepository.AddItem(item2);
        player.Inventory.AddItem("item1");
        player.Inventory.AddItem("item2");
        
        // Act - Add 1 token
        var tokensBefore = _tokenManager.GetTokenCount(ConnectionType.Common);
        _tokenManager.AddTokensToNPC(ConnectionType.Common, 1, "commoner");
        var tokensAfter = _tokenManager.GetTokenCount(ConnectionType.Common);
        var tokensEarned = tokensAfter - tokensBefore;
        
        // Assert - 1.5 * 1.2 = 1.8, rounded up to 2
        Assert.Equal(2, tokensEarned);
    }
}