using System.Collections.Generic;

namespace Wayfarer.Core.Repositories
{
    /// <summary>
    /// Repository interface for NPC entities
    /// </summary>
    public interface INPCRepository : IRepository<NPC>
    {
        /// <summary>
        /// Get NPCs for a specific location
        /// </summary>
        IEnumerable<NPC> GetNPCsForLocation(string locationId);

        /// <summary>
        /// Get NPCs available at a specific time
        /// </summary>
        IEnumerable<NPC> GetAvailableNPCs(TimeBlocks currentTime);

        /// <summary>
        /// Get NPCs by profession
        /// </summary>
        IEnumerable<NPC> GetNPCsByProfession(Professions profession);

        /// <summary>
        /// Get NPCs that provide a specific service
        /// </summary>
        IEnumerable<NPC> GetNPCsProvidingService(ServiceTypes service);

        /// <summary>
        /// Get NPCs for a specific location and time
        /// </summary>
        IEnumerable<NPC> GetNPCsForLocationAndTime(string locationId, TimeBlocks currentTime);

        /// <summary>
        /// Get NPCs for a specific location spot and time
        /// </summary>
        IEnumerable<NPC> GetNPCsForLocationSpotAndTime(string locationSpotId, TimeBlocks currentTime);

        /// <summary>
        /// Get the primary NPC for a specific location spot if available at the current time
        /// </summary>
        NPC GetPrimaryNPCForSpot(string locationSpotId, TimeBlocks currentTime);
    }
}