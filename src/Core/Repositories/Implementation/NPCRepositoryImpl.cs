using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Wayfarer.Core.Repositories.Implementation
{
    /// <summary>
    /// Concrete implementation of INPCRepository
    /// </summary>
    public class NPCRepositoryImpl : BaseRepository<NPC>, INPCRepository
    {
        public NPCRepositoryImpl(IWorldStateAccessor worldState, ILogger<NPCRepositoryImpl> logger) 
            : base(worldState, logger)
        {
        }

        protected override List<NPC> GetCollection()
        {
            return _worldState.NPCs;
        }

        protected override string GetEntityId(NPC entity)
        {
            return entity?.ID;
        }

        protected override string EntityTypeName => "NPC";

        public IEnumerable<NPC> GetNPCsForLocation(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return Enumerable.Empty<NPC>();
            }

            return GetAll().Where(n => n.Location == locationId);
        }

        public IEnumerable<NPC> GetAvailableNPCs(TimeBlocks currentTime)
        {
            return GetAll().Where(n => n.IsAvailable(currentTime));
        }

        public IEnumerable<NPC> GetNPCsByProfession(Professions profession)
        {
            return GetAll().Where(n => n.Profession == profession);
        }

        public IEnumerable<NPC> GetNPCsProvidingService(ServiceTypes service)
        {
            return GetAll().Where(n => n.ProvidedServices.Contains(service));
        }

        public IEnumerable<NPC> GetNPCsForLocationAndTime(string locationId, TimeBlocks currentTime)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return Enumerable.Empty<NPC>();
            }

            // Return all NPCs at location, regardless of availability
            // UI will handle whether they're interactable based on availability
            return GetAll().Where(n => n.Location == locationId);
        }

        public IEnumerable<NPC> GetNPCsForLocationSpotAndTime(string locationSpotId, TimeBlocks currentTime)
        {
            if (string.IsNullOrWhiteSpace(locationSpotId))
            {
                return Enumerable.Empty<NPC>();
            }

            _logger.LogDebug($"Looking for NPCs at spot '{locationSpotId}' during {currentTime}");
            
            var npcsAtSpot = GetAll().Where(n => n.SpotId == locationSpotId).ToList();
            
            _logger.LogDebug($"Found {npcsAtSpot.Count} NPCs at spot '{locationSpotId}': " + 
                string.Join(", ", npcsAtSpot.Select(n => $"{n.Name} ({n.ID}) - Available: {n.IsAvailable(currentTime)}")));
            
            return npcsAtSpot;
        }

        public NPC GetPrimaryNPCForSpot(string locationSpotId, TimeBlocks currentTime)
        {
            if (string.IsNullOrWhiteSpace(locationSpotId))
            {
                return null;
            }

            return GetAll().FirstOrDefault(n => n.SpotId == locationSpotId && n.IsAvailable(currentTime));
        }
    }
}