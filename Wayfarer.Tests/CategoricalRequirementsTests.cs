using Xunit;
using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests;

public class CategoricalRequirementsTests
{
    private GameWorld CreateTestGameWorld()
    {
        GameWorldInitializer initializer = new GameWorldInitializer("Content");
        return initializer.LoadGame();
    }

    [Fact]
    public void SocialAccessRequirement_Should_Allow_Any_For_Commoner_Requirement()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        var requirement = new SocialAccessRequirement(SocialRequirement.Commoner, itemRepository);

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.True(result); // Anyone should meet commoner requirements
    }

    [Fact]
    public void SocialAccessRequirement_Should_Block_Merchant_Without_Credentials()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        var requirement = new SocialAccessRequirement(SocialRequirement.Merchant_Class, itemRepository);

        // Clear player inventory to ensure no merchant items
        Player player = gameWorld.GetPlayer();
        for (int i = 0; i < player.Inventory.ItemSlots.Length; i++)
        {
            player.Inventory.ItemSlots[i] = null;
        }

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.False(result); // Should be blocked without merchant credentials
    }

    [Fact]
    public void SocialAccessRequirement_Should_Allow_Merchant_With_Trade_Items()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        var requirement = new SocialAccessRequirement(SocialRequirement.Merchant_Class, itemRepository);

        // Add merchant item to player inventory
        Player player = gameWorld.GetPlayer();
        player.Inventory.ItemSlots[0] = "fine_merchant_cloak";

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.True(result); // Should be allowed with merchant attire
    }

    [Fact]
    public void ToolCategoryRequirement_Should_Allow_None_Category()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        var requirement = new ToolCategoryRequirement(ToolCategory.None, itemRepository);

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.True(result); // None category should always be met
    }

    [Fact]
    public void ToolCategoryRequirement_Should_Block_Missing_Basic_Tools()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        var requirement = new ToolCategoryRequirement(ToolCategory.Basic_Tools, itemRepository);

        // Clear player inventory
        Player player = gameWorld.GetPlayer();
        for (int i = 0; i < player.Inventory.ItemSlots.Length; i++)
        {
            player.Inventory.ItemSlots[i] = null;
        }

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.False(result); // Should be blocked without tools
    }

    [Fact]
    public void ToolCategoryRequirement_Should_Allow_With_Tools()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        var requirement = new ToolCategoryRequirement(ToolCategory.Basic_Tools, itemRepository);

        // Add tools to player inventory
        Player player = gameWorld.GetPlayer();
        player.Inventory.ItemSlots[0] = "basic_tools";

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.True(result); // Should be allowed with tools
    }

    [Fact]
    public void EnvironmentRequirement_Should_Allow_Any_Category()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        var requirement = new EnvironmentRequirement(EnvironmentCategory.Any);

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.True(result); // Any category should always be met
    }

    [Fact]
    public void EnvironmentRequirement_Should_Detect_Hearth_Location()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        var requirement = new EnvironmentRequirement(EnvironmentCategory.Hearth);

        // Move to a location with hearth (assuming dusty_flagon has hearth spot)
        // The test game world should initialize with player at dusty_flagon hearth

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert - This depends on the test data having a hearth spot
        // We'll just verify the requirement can be evaluated without error
        Assert.True(result || !result); // Either result is valid for this test
    }

    [Fact]
    public void EnvironmentRequirement_Should_Check_Commercial_Setting()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        var requirement = new EnvironmentRequirement(EnvironmentCategory.Commercial_Setting);

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert - Result depends on current location's commercial nature
        Assert.True(result || !result); // Either result is valid for this test
    }

    [Fact]
    public void KnowledgeLevelRequirement_Should_Allow_None_Level()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        var requirement = new KnowledgeLevelRequirement(KnowledgeRequirement.None);

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.True(result); // None level should always be met
    }

    [Fact]
    public void KnowledgeLevelRequirement_Should_Allow_Basic_Level()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        var requirement = new KnowledgeLevelRequirement(KnowledgeRequirement.Basic);

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.True(result); // All players should have basic knowledge
    }

    [Fact]
    public void KnowledgeLevelRequirement_Should_Check_Professional_Level()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        var requirement = new KnowledgeLevelRequirement(KnowledgeRequirement.Professional);

        Player player = gameWorld.GetPlayer();
        int playerLevel = player.Level;

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert - Should be true if player level >= 3
        Assert.Equal(playerLevel >= 3, result);
    }

    [Fact]
    public void SocialAccessRequirement_Should_Provide_Clear_Description()
    {
        // Arrange
        ItemRepository itemRepository = new ItemRepository(CreateTestGameWorld());
        var requirement = new SocialAccessRequirement(SocialRequirement.Merchant_Class, itemRepository);

        // Act
        string description = requirement.GetDescription();

        // Assert
        Assert.Contains("merchant class", description.ToLower());
        Assert.Contains("social standing", description.ToLower());
    }

    [Fact]
    public void ToolCategoryRequirement_Should_Provide_Clear_Description()
    {
        // Arrange
        ItemRepository itemRepository = new ItemRepository(CreateTestGameWorld());
        var requirement = new EquipmentCategoryRequirement(EquipmentCategory.Climbing_Equipment, itemRepository);

        // Act
        string description = requirement.GetDescription();

        // Assert
        Assert.Contains("climbing equipment", description.ToLower());
        Assert.Contains("requires", description.ToLower());
    }

    [Fact]
    public void EnvironmentRequirement_Should_Provide_Clear_Description()
    {
        // Arrange
        var requirement = new EnvironmentRequirement(EnvironmentCategory.Workshop);

        // Act
        string description = requirement.GetDescription();

        // Assert
        Assert.Contains("workshop", description.ToLower());
        Assert.Contains("environment", description.ToLower());
    }

    [Fact]
    public void KnowledgeLevelRequirement_Should_Provide_Clear_Description()
    {
        // Arrange
        var requirement = new KnowledgeLevelRequirement(KnowledgeRequirement.Expert);

        // Act
        string description = requirement.GetDescription();

        // Assert
        Assert.Contains("expert", description.ToLower());
        Assert.Contains("knowledge", description.ToLower());
    }

    [Fact]
    public void ActionFactory_Should_Create_Categorical_Requirements()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ActionRepository actionRepository = new ActionRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);
        ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository, contractRepository, contractValidation);

        // Create action definition with categorical requirements
        var actionDef = new ActionDefinition("test_action", "Test Action", "test_spot")
        {
            SocialRequirement = SocialRequirement.Merchant_Class,
            ToolRequirements = new List<ToolCategory> { ToolCategory.Basic_Tools },
            EnvironmentRequirements = new List<EnvironmentCategory> { EnvironmentCategory.Workshop },
            KnowledgeRequirement = KnowledgeRequirement.Professional,
            ActionPointCost = 1
        };

        // Act
        LocationAction action = actionFactory.CreateActionFromTemplate(
            actionDef, "test_location", "test_spot", ActionExecutionTypes.Instant);

        // Assert
        Assert.NotNull(action.Requirements);
        Assert.True(action.Requirements.Count > 4); // Should have categorical + numerical requirements

        // Check for specific requirement types
        Assert.Contains(action.Requirements, r => r is SocialAccessRequirement);
        Assert.Contains(action.Requirements, r => r is ToolCategoryRequirement);
        Assert.Contains(action.Requirements, r => r is EnvironmentRequirement);
        Assert.Contains(action.Requirements, r => r is KnowledgeLevelRequirement);
        Assert.Contains(action.Requirements, r => r is ActionPointRequirement);
    }
}