using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Wayfarer.Core.Repositories.Implementation
{
    /// <summary>
    /// Concrete implementation of IItemRepository
    /// </summary>
    public class ItemRepositoryImpl : BaseRepository<Item>, IItemRepository
    {
        public ItemRepositoryImpl(IWorldStateAccessor worldState, ILogger<ItemRepositoryImpl> logger) 
            : base(worldState, logger)
        {
        }

        protected override List<Item> GetCollection()
        {
            return _worldState.Items;
        }

        protected override string GetEntityId(Item entity)
        {
            return entity?.Id;
        }

        public Item GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return GetCollection()?.FirstOrDefault(i => i.Name == name);
        }

        public IEnumerable<Item> GetItemsForLocation(string locationId, string spotId = null)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return Enumerable.Empty<Item>();
            }

            var items = GetAll();
            
            if (!string.IsNullOrWhiteSpace(spotId))
            {
                return items.Where(i => i.LocationId == locationId && i.SpotId == spotId);
            }
            
            return items.Where(i => i.LocationId == locationId);
        }

        public IEnumerable<Item> GetByCategory(ItemCategory category)
        {
            return GetAll().Where(i => i.Categories.Contains(category));
        }

        public IEnumerable<Item> GetBySizeCategory(SizeCategory sizeCategory)
        {
            return GetAll().Where(i => i.Size == sizeCategory);
        }

        public IEnumerable<Item> GetEquipmentItems()
        {
            return GetAll().Where(i => i.IsEquipment);
        }
    }
}