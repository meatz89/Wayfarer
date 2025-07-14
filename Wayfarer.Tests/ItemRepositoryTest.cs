using Wayfarer.Game.MainSystem;
using Xunit;

namespace Wayfarer.Tests
{
    public class ItemRepositoryTest
    {
        [Fact]
        public void ItemRepository_GetItemById_Should_Find_Items()
        {
            // Create test world with standard setup
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Get all items
            List<Item> allItems = itemRepository.GetAllItems();
            Assert.True(allItems.Count > 0, $"Should have items, found {allItems.Count}");

            // Test GetItemById for each item
            foreach (Item item in allItems)
            {
                Item foundItem = itemRepository.GetItemById(item.Id);
                Assert.NotNull(foundItem);
                Assert.Equal(item.Id, foundItem.Id);
                Assert.Equal(item.Name, foundItem.Name);
            }

            // Test specific known items
            Item herbsItem = itemRepository.GetItemById("herbs");
            Assert.NotNull(herbsItem);
            Assert.Equal("herbs", herbsItem.Id);

            // Test non-existent item
            Item nonExistent = itemRepository.GetItemById("non_existent_item");
            Assert.Null(nonExistent);
        }
    }
}