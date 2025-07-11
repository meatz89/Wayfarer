using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests;

/// <summary>
/// Tests that validate the "only check completion actions" principle for contracts.
/// Contracts should complete based on specific completion actions, NOT on process requirements.
/// </summary>
public class ContractCompletionTests
{
    private GameWorld CreateTestGameWorld()
    {
        GameWorldInitializer initializer = new GameWorldInitializer("Content");
        return initializer.LoadGame();
    }

    private ContractProgressionService CreateContractProgressionService(GameWorld gameWorld)
    {
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        return new ContractProgressionService(contractRepository, itemRepository, locationRepository);
    }

    [Fact]
    public void DeliverToolsContract_CompletesOnSellAction_NotOnHavingTools()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();
        
        // Add the deliver_tools contract to active contracts
        Contract deliverToolsContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "deliver_tools");
        Assert.NotNull(deliverToolsContract);
        gameWorld.ActiveContracts.Add(deliverToolsContract);
        
        // CRITICAL: Player doesn't need to have tools beforehand
        player.Inventory.Clear();
        Assert.False(player.Inventory.HasItem("tools"));
        
        // Act: Player sells tools at town_square (completion action)
        contractProgressionService.CheckMarketProgression("tools", "town_square", TransactionType.Sell, 1, 10, player);
        
        // Assert: Contract should complete based on the sell action alone
        Assert.True(deliverToolsContract.IsCompleted);
    }

    [Fact]
    public void DeliverToolsContract_DoesNotComplete_OnJustHavingTools()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();
        
        Contract deliverToolsContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "deliver_tools");
        Assert.NotNull(deliverToolsContract);
        gameWorld.ActiveContracts.Add(deliverToolsContract);
        
        // Act: Player has tools but doesn't perform completion action
        player.Inventory.AddItem("tools");
        
        // Assert: Contract should NOT complete just from having tools
        Assert.False(deliverToolsContract.IsCompleted);
    }

    [Fact]
    public void MountainExplorationContract_CompletesOnArrival_NotOnHavingEquipment()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();
        
        Contract mountainContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "mountain_exploration");
        Assert.NotNull(mountainContract);
        gameWorld.ActiveContracts.Add(mountainContract);
        
        // CRITICAL: Player doesn't need to have equipment beforehand
        player.Inventory.Clear();
        Assert.False(player.Inventory.HasItem("climbing_gear"));
        Assert.False(player.Inventory.HasItem("weather_protection"));
        
        // Act: Player arrives at mountain_summit (completion action)
        contractProgressionService.CheckTravelProgression("mountain_summit", player);
        
        // Assert: Contract should complete based on arrival alone
        Assert.True(mountainContract.IsCompleted);
    }

    [Fact]
    public void SimpleMessageDeliveryContract_CompletesOnConversation_NotOnTravel()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();
        
        Contract messageContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "simple_message_delivery");
        Assert.NotNull(messageContract);
        gameWorld.ActiveContracts.Add(messageContract);
        
        // Act: Player talks to blacksmith (completion action)
        contractProgressionService.CheckNPCConversationProgression("blacksmith", player);
        
        // Assert: Contract should complete based on conversation alone
        Assert.True(messageContract.IsCompleted);
    }

    [Fact]
    public void CarpentersSpecialContract_CompletesOnLocationAction_NotOnHavingToolsOrLocation()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();
        
        Contract carpenterContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "carpenter_special");
        Assert.NotNull(carpenterContract);
        gameWorld.ActiveContracts.Add(carpenterContract);
        
        // CRITICAL: Player doesn't need tools or specific location beforehand
        player.Inventory.Clear();
        Assert.False(player.Inventory.HasItem("tools"));
        Assert.False(player.Inventory.HasItem("rope"));
        
        // Create a mock location action for testing
        LocationAction carpenterAction = new LocationAction
        {
            ActionId = "complete_carpenter_job",
            Name = "Complete Carpenter Job",
            LocationId = "town_square",
            LocationSpotId = "workshop"
        };
        
        // Act: Player performs the completion action
        contractProgressionService.CheckLocationActionProgression(carpenterAction, player);
        
        // Assert: Contract should complete based on action alone
        Assert.True(carpenterContract.IsCompleted);
    }

    [Fact]
    public void UrgentTradeNegotiationContract_CompletesOnGoldTransaction_NotOnHavingGold()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();
        
        Contract tradeContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "urgent_trade_negotiation");
        Assert.NotNull(tradeContract);
        gameWorld.ActiveContracts.Add(tradeContract);
        
        // CRITICAL: Player doesn't need to have gold beforehand
        player.ModifyCoins(-player.Coins); // Remove all coins
        Assert.Equal(0, player.Coins);
        
        // Act: Player sells gold_coins at merchant_guild (completion action)
        contractProgressionService.CheckMarketProgression("gold_coins", "merchant_guild", TransactionType.Sell, 1, 50, player);
        
        // Assert: Contract should complete based on the transaction alone
        Assert.True(tradeContract.IsCompleted);
    }

    [Fact]
    public void MultipleContracts_CompleteIndependently_BasedOnTheirSpecificActions()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();
        
        // Add multiple contracts
        Contract toolsContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "deliver_tools");
        Contract messageContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "simple_message_delivery");
        Contract mountainContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "mountain_exploration");
        
        Assert.NotNull(toolsContract);
        Assert.NotNull(messageContract);
        Assert.NotNull(mountainContract);
        
        gameWorld.ActiveContracts.AddRange(new[] { toolsContract, messageContract, mountainContract });
        
        // Act 1: Complete message delivery
        contractProgressionService.CheckNPCConversationProgression("blacksmith", player);
        
        // Assert 1: Only message contract completes
        Assert.True(messageContract.IsCompleted);
        Assert.False(toolsContract.IsCompleted);
        Assert.False(mountainContract.IsCompleted);
        
        // Act 2: Complete mountain exploration
        contractProgressionService.CheckTravelProgression("mountain_summit", player);
        
        // Assert 2: Mountain contract also completes, tools still incomplete
        Assert.True(messageContract.IsCompleted);
        Assert.True(mountainContract.IsCompleted);
        Assert.False(toolsContract.IsCompleted);
        
        // Act 3: Complete tools delivery
        contractProgressionService.CheckMarketProgression("tools", "town_square", TransactionType.Sell, 1, 10, player);
        
        // Assert 3: All contracts now complete
        Assert.True(messageContract.IsCompleted);
        Assert.True(mountainContract.IsCompleted);
        Assert.True(toolsContract.IsCompleted);
    }

    [Fact]
    public void ContractProgression_TracksPartialCompletion_ForMultiRequirementContracts()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();
        
        // Find a contract with multiple completion requirements (like artisan_masterwork)
        Contract artisanContract = gameWorld.WorldState.Contracts?.FirstOrDefault(c => c.Id == "artisan_masterwork");
        Assert.NotNull(artisanContract);
        gameWorld.ActiveContracts.Add(artisanContract);
        
        // Act 1: Complete the transaction requirement
        contractProgressionService.CheckMarketProgression("rare_materials", "workshop", TransactionType.Sell, 1, 100, player);
        
        // Assert 1: Contract not yet complete (still needs location action)
        Assert.False(artisanContract.IsCompleted);
        Assert.Single(artisanContract.CompletedTransactions);
        
        // Act 2: Complete the location action requirement
        LocationAction masterworkAction = new LocationAction
        {
            ActionId = "create_masterwork",
            Name = "Create Masterwork",
            LocationId = "workshop",
            LocationSpotId = "crafting_area"
        };
        contractProgressionService.CheckLocationActionProgression(masterworkAction, player);
        
        // Assert 2: Contract now complete (both requirements met)
        Assert.True(artisanContract.IsCompleted);
        Assert.Single(artisanContract.CompletedTransactions);
        Assert.Single(artisanContract.CompletedLocationActions);
    }
}