using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    public class InventoryContentBase : ComponentBase
    {
        [Inject] protected GameWorld GameWorld { get; set; }
        [Inject] protected ItemRepository ItemRepository { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        protected int CurrentWeight { get; set; }
        protected int MaxWeight { get; set; }
        protected List<InventoryItemGroup> InventoryItems { get; set; } = new List<InventoryItemGroup>();

        protected override void OnInitialized()
        {
            LoadInventoryData();
        }

        private void LoadInventoryData()
        {
            Player player = GameWorld.GetPlayer();

            // Get weight status
            CurrentWeight = player.GetCurrentWeight(ItemRepository);
            MaxWeight = player.Inventory.GetCapacity();

            // Get all items from inventory and group by item
            List<Item> items = player.Inventory.GetAllItems();

            // Group items by identity and count
            InventoryItems = items
                .GroupBy(item => item)
                .Select(group => new InventoryItemGroup
                {
                    Item = group.Key,
                    Count = group.Count()
                })
                .OrderBy(group => group.Item.Name)
                .ToList();
        }

        protected double GetWeightPercent()
        {
            if (MaxWeight == 0) return 0;
            return (double)CurrentWeight / MaxWeight * 100.0;
        }

        protected async Task CloseInventory()
        {
            await OnClose.InvokeAsync();
        }
    }

    /// <summary>
    /// Helper class for grouping inventory items
    /// </summary>
    public class InventoryItemGroup
    {
        public Item Item { get; set; }
        public int Count { get; set; }
    }
}
