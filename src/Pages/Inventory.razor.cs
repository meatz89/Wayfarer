using Microsoft.AspNetCore.Components;
using Wayfarer.GameState.Constants;

namespace Wayfarer.Pages
{
    public class InventoryBase : ComponentBase
    {
        [Inject] public TravelManager TravelManager { get; set; }
        [Inject] public ItemRepository ItemRepository { get; set; }

        public void ShowInventory(GameWorld gameWorld)
        {
            Console.WriteLine("=== Inventory ===");

            int totalWeight = TravelManager.CalculateCurrentWeight(gameWorld);
            string weightStatus = totalWeight <= GameConstants.LoadWeight.LIGHT_LOAD_MAX ? "Light load" :
                                 (totalWeight <= GameConstants.LoadWeight.MEDIUM_LOAD_MAX ? "Medium load (+1 stamina cost)" : "Heavy load (+2 stamina cost)");

            Console.WriteLine($"Current load: {weightStatus} ({totalWeight} weight units)");

            // Show coin weight
            int coinWeight = gameWorld.PlayerCoins / GameConstants.Inventory.COINS_PER_WEIGHT_UNIT;
            Console.WriteLine($"Coins: {gameWorld.PlayerCoins} ({coinWeight} weight units)");

            Console.WriteLine();
            Console.WriteLine("Items:");

            // Display each inventory slot with weight
            for (int i = 0; i < gameWorld.PlayerInventory.ItemSlots.Length; i++)
            {
                string itemName = gameWorld.PlayerInventory.ItemSlots[i];
                if (itemName != null)
                {
                    Item item = ItemRepository.GetItemByName(itemName);
                    string weightInfo = item != null ? $" (weight: {item.Weight})" : "";
                    string specialProperties = "";

                    if (item != null)
                    {
                        if (item.Categories.Count > 0)
                        {
                            specialProperties += $" - {string.Join(", ", item.Categories.Select(c => c.ToString().Replace('_', ' ')))}";
                        }
                    }

                    Console.WriteLine($"[{i + 1}] {itemName}{weightInfo}{specialProperties} [Drop]");
                }
                else
                {
                    Console.WriteLine($"[{i + 1}] Empty");
                }
            }
        }
    }
}
