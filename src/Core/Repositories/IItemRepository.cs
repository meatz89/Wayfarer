using System.Collections.Generic;

namespace Wayfarer.Core.Repositories
{
    /// <summary>
    /// Repository interface for Item entities
    /// </summary>
    public interface IItemRepository : IRepository<Item>
    {
        /// <summary>
        /// Get items for a specific location
        /// </summary>
        /// <param name="locationId">The location identifier</param>
        /// <param name="spotId">Optional spot identifier within the location</param>
        IEnumerable<Item> GetItemsForLocation(string locationId, string spotId = null);

        /// <summary>
        /// Get an item by its name
        /// </summary>
        Item GetByName(string name);

        /// <summary>
        /// Get items by category
        /// </summary>
        IEnumerable<Item> GetByCategory(ItemCategory category);

        /// <summary>
        /// Get items by size category
        /// </summary>
        IEnumerable<Item> GetBySizeCategory(SizeCategory sizeCategory);

        /// <summary>
        /// Get all equipment items
        /// </summary>
        IEnumerable<Item> GetEquipmentItems();
    }
}