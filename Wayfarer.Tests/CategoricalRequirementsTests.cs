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
    public void ToolCategoryRequirement_Should_Allow_None_Category()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ToolCategoryRequirement requirement = new ToolCategoryRequirement(ToolCategory.None, itemRepository);

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
        ToolCategoryRequirement requirement = new ToolCategoryRequirement(ToolCategory.Basic_Tools, itemRepository);

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
        ToolCategoryRequirement requirement = new ToolCategoryRequirement(ToolCategory.Basic_Tools, itemRepository);

        // Add tools to player inventory
        Player player = gameWorld.GetPlayer();
        player.Inventory.ItemSlots[0] = "basic_tools";

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.True(result); // Should be allowed with tools
    }

    [Fact]
    public void KnowledgeLevelRequirement_Should_Allow_None_Level()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        KnowledgeLevelRequirement requirement = new KnowledgeLevelRequirement(KnowledgeRequirement.None);

        // Act
        bool result = requirement.IsMet(gameWorld);

        // Assert
        Assert.True(result); // None level should always be met
    }

    [Fact]
    public void ToolCategoryRequirement_Should_Provide_Clear_Description()
    {
        // Arrange
        ItemRepository itemRepository = new ItemRepository(CreateTestGameWorld());
        EquipmentCategoryRequirement requirement = new EquipmentCategoryRequirement(EquipmentCategory.Climbing_Equipment, itemRepository);

        // Act
        string description = requirement.GetDescription();

        // Assert
        Assert.Contains("climbing equipment", description.ToLower());
        Assert.Contains("requires", description.ToLower());
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
        ActionDefinition actionDef = new ActionDefinition("test_action", "Test Action", "test_spot")
        {
            ToolRequirements = new List<ToolCategory> { ToolCategory.Basic_Tools },
            ActionPointCost = 1
        };

        // Act
        LocationAction action = actionFactory.CreateActionFromTemplate(
            actionDef, "test_location", "test_spot", ActionExecutionTypes.Instant);

        // Assert
        Assert.NotNull(action.Requirements);
        Assert.True(action.Requirements.Count > 4); // Should have categorical + numerical requirements

        // Check for specific requirement types
        Assert.Contains(action.Requirements, r => r is ToolCategoryRequirement);
        Assert.Contains(action.Requirements, r => r is KnowledgeLevelRequirement);
        Assert.Contains(action.Requirements, r => r is ActionPointRequirement);
    }
}