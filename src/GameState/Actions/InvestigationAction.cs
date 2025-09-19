using System;

namespace Wayfarer.GameState.Actions
{
    /// <summary>
    /// Investigation action allows players to spend 1 segment to gain familiarity with a location.
    /// Familiarity gain depends on spot properties: Quiet spots give +2, Busy spots give +1, others give +1.
    /// Used to unlock observation rewards at higher familiarity levels.
    /// </summary>
    public class InvestigationAction
    {

        /// <summary>
        /// Time required for investigation in segments (always 1)
        /// </summary>
        public int TimeSegments => 1;

        /// <summary>
        /// Calculate familiarity gain based on spot properties.
        /// Quiet spots: +2 familiarity
        /// Busy spots: +1 familiarity  
        /// All other spots: +1 familiarity
        /// </summary>
        /// <param name="spot">The location spot being investigated</param>
        /// <param name="currentTime">Current time block for time-specific properties</param>
        /// <returns>Amount of familiarity to gain (1 or 2)</returns>
        public int GetFamiliarityGain(LocationSpot spot, TimeBlocks currentTime)
        {
            if (spot == null) return 1;

            List<SpotPropertyType> activeProperties = spot.GetActiveProperties(currentTime);

            // Check for Quiet property first (higher priority)
            if (activeProperties.Contains(SpotPropertyType.Quiet))
            {
                return 2;
            }

            // Check for Busy property
            if (activeProperties.Contains(SpotPropertyType.Busy))
            {
                return 1;
            }

            // Default for all other spots
            return 1;
        }

        /// <summary>
        /// Check if investigation is possible at this location.
        /// Returns true if player can investigate (has attention and location isn't at max familiarity).
        /// </summary>
        /// <param name="player">Player performing the investigation</param>
        /// <param name="location">Location being investigated</param>
        /// <returns>True if investigation is possible</returns>
        public bool CanInvestigate(Player player, Location location)
        {
            if (player == null || location == null) return false;


            // Check if location familiarity is already at maximum
            int currentFamiliarity = player.GetLocationFamiliarity(location.Id);
            if (currentFamiliarity >= location.MaxFamiliarity) return false;

            return true;
        }
    }
}