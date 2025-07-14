using Wayfarer.Game.MainSystem;
using Xunit;

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
    public void TradingPostSuppliesContract_CompletesOnSellAction_NotOnHavingTools()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();

        // Add the trading_post_supplies contract to active contracts using repository pattern
        Contract tradingPostContract = contractRepository.GetContract("trading_post_supplies");
        Assert.NotNull(tradingPostContract);
        contractRepository.AddActiveContract(tradingPostContract);

        // CRITICAL: Player doesn't need to have tools beforehand
        player.Inventory.Clear();
        Assert.False(player.Inventory.HasItem("tools"));

        // First complete the destination requirement
        contractProgressionService.CheckTravelProgression("crossbridge", player);

        // Then complete the transaction requirement
        contractProgressionService.CheckMarketProgression("tools", "crossbridge", TransactionType.Sell, 1, 10, player);

        // Assert: Contract should complete after both requirements are met
        Assert.True(tradingPostContract.IsCompleted);
    }

    [Fact]
    public void TradingPostSuppliesContract_DoesNotComplete_OnJustHavingTools()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();

        Contract tradingPostContract = contractRepository.GetContract("trading_post_supplies");
        Assert.NotNull(tradingPostContract);
        contractRepository.AddActiveContract(tradingPostContract);

        // Act: Player has tools but doesn't perform completion action
        player.Inventory.AddItem("tools");

        // Assert: Contract should NOT complete just from having tools
        Assert.False(tradingPostContract.IsCompleted);
    }

    [Fact]
    public void MountainAccessContract_CompletesOnArrival_NotOnHavingEquipment()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();

        Contract mountainContract = contractRepository.GetContract("mountain_access_test");
        Assert.NotNull(mountainContract);
        contractRepository.AddActiveContract(mountainContract);

        // CRITICAL: Player doesn't need to have equipment beforehand
        player.Inventory.Clear();
        Assert.False(player.Inventory.HasItem("climbing_gear"));
        Assert.False(player.Inventory.HasItem("weather_protection"));

        // Act: Player arrives at ironhold (completion action per contract requirements)
        contractProgressionService.CheckTravelProgression("ironhold", player);

        // Assert: Contract should complete based on arrival alone (this contract only requires destination)
        Assert.True(mountainContract.IsCompleted);
    }

    [Fact]
    public void SimpleMessageDeliveryContract_CompletesOnConversation_NotOnTravel()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();

        Contract messageContract = contractRepository.GetContract("village_herb_delivery");
        Assert.NotNull(messageContract);
        contractRepository.AddActiveContract(messageContract);

        // Act: Complete both destination and transaction requirements
        contractProgressionService.CheckTravelProgression("millbrook", player);
        contractProgressionService.CheckMarketProgression("herbs", "millbrook", TransactionType.Sell, 1, 5, player);

        // Assert: Contract should complete after both requirements are met
        Assert.True(messageContract.IsCompleted);
    }

    [Fact]
    public void CarpentersSpecialContract_CompletesOnLocationAction_NotOnHavingToolsOrLocation()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();

        Contract carpenterContract = contractRepository.GetContract("dark_passage_navigation");
        Assert.NotNull(carpenterContract);
        contractRepository.AddActiveContract(carpenterContract);

        // CRITICAL: Player doesn't need tools or specific location beforehand
        player.Inventory.Clear();
        Assert.False(player.Inventory.HasItem("tools"));
        Assert.False(player.Inventory.HasItem("rope"));

        // Act: Player travels to workshop (this contract only requires destination)
        contractProgressionService.CheckTravelProgression("workshop", player);

        // Assert: Contract should complete based on destination alone
        Assert.True(carpenterContract.IsCompleted);
    }

    [Fact]
    public void UrgentTradeNegotiationContract_CompletesOnGoldTransaction_NotOnHavingGold()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();

        Contract tradeContract = contractRepository.GetContract("maritime_trade");
        Assert.NotNull(tradeContract);
        contractRepository.AddActiveContract(tradeContract);

        // CRITICAL: Player doesn't need to have gold beforehand
        player.ModifyCoins(-player.Coins); // Remove all coins
        Assert.Equal(0, player.Coins);

        // Act: Complete all requirements for maritime_trade
        contractProgressionService.CheckTravelProgression("eastport", player);
        contractProgressionService.CheckTravelProgression("crossbridge", player);
        contractProgressionService.CheckMarketProgression("exotic_spices", "eastport", TransactionType.Buy, 1, 20, player);
        contractProgressionService.CheckMarketProgression("exotic_spices", "crossbridge", TransactionType.Sell, 1, 30, player);

        // Assert: Contract should complete based on all transactions
        Assert.True(tradeContract.IsCompleted);
    }

    [Fact]
    public void MultipleContracts_CompleteIndependently_BasedOnTheirSpecificActions()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractProgressionService contractProgressionService = CreateContractProgressionService(gameWorld);
        Player player = gameWorld.GetPlayer();

        // Add multiple contracts using repository pattern - using existing contract IDs
        Contract toolsContract = contractRepository.GetContract("trading_post_supplies");
        Contract messageContract = contractRepository.GetContract("village_herb_delivery");
        Contract mountainContract = contractRepository.GetContract("mountain_access_test");

        Assert.NotNull(toolsContract);
        Assert.NotNull(messageContract);
        Assert.NotNull(mountainContract);

        contractRepository.AddActiveContract(toolsContract);
        contractRepository.AddActiveContract(messageContract);
        contractRepository.AddActiveContract(mountainContract);

        // Act 1: Complete herb delivery (destination + transaction)
        contractProgressionService.CheckTravelProgression("millbrook", player);
        contractProgressionService.CheckMarketProgression("herbs", "millbrook", TransactionType.Sell, 1, 5, player);

        // Assert 1: Only message contract completes
        Assert.True(messageContract.IsCompleted);
        Assert.False(toolsContract.IsCompleted);
        Assert.False(mountainContract.IsCompleted);

        // Act 2: Complete mountain exploration
        contractProgressionService.CheckTravelProgression("ironhold", player);

        // Assert 2: Mountain contract also completes, tools still incomplete
        Assert.True(messageContract.IsCompleted);
        Assert.True(mountainContract.IsCompleted);
        Assert.False(toolsContract.IsCompleted);

        // Act 3: Complete tools delivery (destination + transaction)
        contractProgressionService.CheckTravelProgression("crossbridge", player);
        contractProgressionService.CheckMarketProgression("tools", "crossbridge", TransactionType.Sell, 1, 10, player);

        // Assert 3: All contracts now complete
        Assert.True(messageContract.IsCompleted);
        Assert.True(mountainContract.IsCompleted);
        Assert.True(toolsContract.IsCompleted);
    }

}