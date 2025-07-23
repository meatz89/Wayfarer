using System.Collections.Generic;

namespace Wayfarer.Core.Repositories
{
    /// <summary>
    /// Interface to abstract access to the game world state
    /// This allows repositories to access world state without direct dependency on GameWorld
    /// </summary>
    public interface IWorldStateAccessor
    {
        /// <summary>
        /// Get the collection of items
        /// </summary>
        List<Item> Items { get; }

        /// <summary>
        /// Get the collection of NPCs
        /// </summary>
        List<NPC> NPCs { get; }

        /// <summary>
        /// Get the collection of letter templates
        /// </summary>
        List<LetterTemplate> LetterTemplates { get; }

        /// <summary>
        /// Get the collection of locations
        /// </summary>
        List<Location> Locations { get; }

        /// <summary>
        /// Get the collection of location spots
        /// </summary>
        List<LocationSpot> LocationSpots { get; }

        /// <summary>
        /// Get the collection of route options
        /// </summary>
        List<RouteOption> Routes { get; }

        /// <summary>
        /// Get the current time manager
        /// </summary>
        ITimeManager TimeManager { get; }
    }
}