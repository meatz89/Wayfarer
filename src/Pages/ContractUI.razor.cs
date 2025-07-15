using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages
{
    public class ContractUIBase : ComponentBase
    {
        [Inject] public GameWorld GameWorld { get; set; }
        [Inject] public GameWorldManager GameManager { get; set; }
        [Inject] public ContractRepository ContractRepository { get; set; }
        [Inject] ContractSystem ContractSystem { get; set; }
        [Inject] ItemRepository ItemRepository { get; set; }

        public List<Contract> AvailableContracts
        {
            get
            {
                return ContractRepository.GetAvailableContracts(GameWorld.CurrentDay, GameWorld.CurrentTimeBlock);
            }
        }

        public void CompleteContract(Contract contract)
        {
            GameManager.CompleteContract(contract);

            // Refresh UI
            StateHasChanged();
        }

        /// <summary>
        /// Get current equipment categories owned by player
        /// </summary>
        public List<EquipmentCategory> GetCurrentEquipmentCategories()
        {
            List<EquipmentCategory> ownedCategories = new List<EquipmentCategory>();

            foreach (string itemName in GameWorld.GetPlayer().Inventory.ItemSlots)
            {
                if (itemName != null)
                {
                    Item item = ItemRepository.GetItemByName(itemName);
                    if (item != null)
                    {
                        ownedCategories.AddRange(item.Categories);
                    }
                }
            }

            return ownedCategories.Distinct().ToList();
        }

        /// <summary>
        /// Get available items for a specific equipment category
        /// </summary>
        public List<Item> GetAvailableItemsForCategory(EquipmentCategory category)
        {
            Location currentLocation = GameWorld.CurrentLocation;
            List<Item> allMarketItems = GameManager.GetAvailableMarketItems(currentLocation.Id);
            return allMarketItems.Where(i => i.Categories.Contains(category)).ToList();
        }

        /// <summary>
        /// Get benefit description for equipment category
        /// </summary>
        public string GetEquipmentBenefit(EquipmentCategory category)
        {
            return category switch
            {
                EquipmentCategory.Climbing_Equipment => "Enables mountain terrain access",
                EquipmentCategory.Navigation_Tools => "Enables wilderness navigation",
                EquipmentCategory.Weather_Protection => "Enables all-weather travel",
                EquipmentCategory.Water_Transport => "Enables river route access",
                EquipmentCategory.Special_Access => "Enables restricted area access",
                _ => "Enhances travel capabilities"
            };
        }
    }
}