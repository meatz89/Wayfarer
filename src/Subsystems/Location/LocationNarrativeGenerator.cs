using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Strongly typed context for narrative generation
/// </summary>
public class NarrativeContext
{
    public string Weather { get; set; }
    public int NpcCount { get; set; }
    public int UrgentLetters { get; set; }
    public TimeBlocks? CurrentTimeBlock { get; set; }
    public string LocationName { get; set; }
    public bool IsIndoor { get; set; }
}

namespace Wayfarer.Subsystems.LocationSubsystem
{
    /// <summary>
    /// Generates narrative text and atmospheric descriptions for locations and spots.
    /// Creates immersive descriptions based on time, properties, and context.
    /// </summary>
    public class LocationNarrativeGenerator
    {
        private readonly GameWorld _gameWorld;
        private readonly TimeManager _timeManager;

        public LocationNarrativeGenerator(GameWorld gameWorld, TimeManager timeManager)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
            _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        }

        /// <summary>
        /// Generate atmosphere text for a location spot.
        /// </summary>
        public string GenerateAtmosphereText(
            LocationSpot spot,
            Location location,
            TimeBlocks currentTime,
            int urgentObligations,
            int npcsPresent)
        {
            if (spot != null)
            {
                return GenerateSpotAtmosphere(spot, currentTime, urgentObligations, npcsPresent);
            }
            else if (location != null)
            {
                return GenerateLocationAtmosphere(location, currentTime);
            }

            return "An undefined location.";
        }

        /// <summary>
        /// Generate atmosphere text specific to a spot.
        /// </summary>
        private string GenerateSpotAtmosphere(
            LocationSpot spot,
            TimeBlocks currentTime,
            int urgentObligations,
            int npcsPresent)
        {
            SpotDescriptionGenerator descGenerator = new SpotDescriptionGenerator();
            List<SpotPropertyType> activeProperties = spot.GetActiveProperties(currentTime);

            // Debug log
            Console.WriteLine($"[LocationNarrativeGenerator] Generating atmosphere for: {spot.SpotID}");
            Console.WriteLine($"  Properties: {string.Join(", ", activeProperties)}");
            Console.WriteLine($"  Time: {currentTime}, NPCs: {npcsPresent}, Urgent: {urgentObligations}");

            return descGenerator.GenerateDescription(
                activeProperties,
                currentTime,
                urgentObligations,
                npcsPresent
            );
        }

        /// <summary>
        /// Generate atmosphere text for a general location.
        /// </summary>
        private string GenerateLocationAtmosphere(Location location, TimeBlocks currentTime)
        {
            if (string.IsNullOrEmpty(location.Description))
            {
                return GenerateDefaultLocationDescription(location, currentTime);
            }

            // Add time-specific modifiers to the base description
            string timeModifier = GetTimeModifier(currentTime);
            if (!string.IsNullOrEmpty(timeModifier))
            {
                return $"{location.Description} {timeModifier}";
            }

            return location.Description;
        }

        /// <summary>
        /// Generate a default description when none is provided.
        /// </summary>
        private string GenerateDefaultLocationDescription(Location location, TimeBlocks currentTime)
        {
            string timeDesc = GetTimeDescription(currentTime);
            string locationType = DetermineLocationType(location);

            return $"{timeDesc} at {location.Name}, {locationType}.";
        }

        /// <summary>
        /// Get a time-specific modifier for descriptions.
        /// </summary>
        private string GetTimeModifier(TimeBlocks timeBlock)
        {
            return timeBlock switch
            {
                TimeBlocks.Dawn => "The early morning light casts long shadows.",
                TimeBlocks.Morning => "The morning bustle is beginning to pick up.",
                TimeBlocks.Afternoon => "The afternoon sun warms the area.",
                TimeBlocks.Evening => "Evening approaches, bringing a change of pace.",
                TimeBlocks.Night => "Night has fallen, bringing quiet to most areas.",
                TimeBlocks.LateNight => "The deep of night brings stillness.",
                _ => ""
            };
        }

        /// <summary>
        /// Get a general time description.
        /// </summary>
        private string GetTimeDescription(TimeBlocks timeBlock)
        {
            return timeBlock switch
            {
                TimeBlocks.Dawn => "It's early dawn",
                TimeBlocks.Morning => "It's morning",
                TimeBlocks.Afternoon => "It's afternoon",
                TimeBlocks.Evening => "It's evening",
                TimeBlocks.Night => "It's nighttime",
                TimeBlocks.LateNight => "It's late at night",
                _ => "The time is uncertain"
            };
        }

        /// <summary>
        /// Determine the type of location for generic descriptions.
        /// </summary>
        private string DetermineLocationType(Location location)
        {
            // Check location tags or properties to determine type
            if (location.DomainTags?.Contains("Market") == true)
                return "a bustling marketplace";
            if (location.DomainTags?.Contains("Inn") == true)
                return "a welcoming inn";
            if (location.DomainTags?.Contains("Noble") == true)
                return "an upscale district";
            if (location.DomainTags?.Contains("Dock") == true)
                return "a busy port area";

            return "an interesting location";
        }

        /// <summary>
        /// Generate a brief description for entering a new location.
        /// </summary>
        public string GenerateArrivalText(Location location, LocationSpot entrySpot)
        {
            if (location == null) return "You arrive at an unknown location.";

            string spotDesc = "";
            if (entrySpot != null)
            {
                spotDesc = $" at {entrySpot.Name}";
            }

            return $"You arrive at {location.Name}{spotDesc}.";
        }

        /// <summary>
        /// Generate text for leaving a location.
        /// </summary>
        public string GenerateDepartureText(Location location, LocationSpot exitSpot)
        {
            if (location == null) return "You depart from your current location.";

            string spotDesc = "";
            if (exitSpot != null)
            {
                spotDesc = $" from {exitSpot.Name}";
            }

            return $"You leave {location.Name}{spotDesc}.";
        }

        /// <summary>
        /// Generate a description for movement between spots.
        /// </summary>
        public string GenerateMovementText(LocationSpot fromSpot, LocationSpot toSpot)
        {
            if (fromSpot == null || toSpot == null)
                return "You move to a new area.";

            return $"You move from {fromSpot.Name} to {toSpot.Name}.";
        }

        /// <summary>
        /// Generate contextual flavor text based on current conditions.
        /// </summary>
        public string GenerateContextualFlavor(LocationSpot spot, NarrativeContext context)
        {
            List<string> flavorParts = new List<string>();

            // Check for weather effects
            if (!string.IsNullOrEmpty(context?.Weather))
            {
                string weatherFlavor = GetWeatherFlavor(context.Weather, spot);
                if (!string.IsNullOrEmpty(weatherFlavor))
                    flavorParts.Add(weatherFlavor);
            }

            // Check for crowding
            if (context?.NpcCount > 3)
            {
                flavorParts.Add("The area is quite crowded.");
            }

            // Check for urgent business
            if (context?.UrgentLetters > 0)
            {
                flavorParts.Add("You feel the focus of urgent obligations.");
            }

            return string.Join(" ", flavorParts);
        }

        /// <summary>
        /// Get weather-specific flavor text.
        /// </summary>
        private string GetWeatherFlavor(string weather, LocationSpot spot)
        {
            // Assume we're outdoors unless spot has warm/shaded properties (indicating shelter)
            bool isIndoor = spot?.SpotProperties?.Contains(SpotPropertyType.Warm) == true ||
                           spot?.SpotProperties?.Contains(SpotPropertyType.Shaded) == true;

            return weather switch
            {
                "Rain" when !isIndoor => "Rain patters steadily around you.",
                "Rain" when isIndoor => "You can hear rain against the windows.",
                "Snow" when !isIndoor => "Snow drifts gently through the air.",
                "Snow" when isIndoor => "Snow accumulates outside the windows.",
                "Wind" when !isIndoor => "A strong wind tugs at your clothes.",
                "Wind" when isIndoor => "Wind rattles the shutters.",
                _ => null
            };
        }

        /// <summary>
        /// Generate observation prompt text.
        /// </summary>
        public string GenerateObservationPrompt(Location location, LocationSpot spot)
        {
            if (spot != null)
            {
                List<SpotPropertyType> properties = spot.GetActiveProperties(_timeManager.GetCurrentTimeBlock());
                if (properties.Contains(SpotPropertyType.ViewsMarket))
                    return "The area has several interesting details worth observing.";
                if (properties.Contains(SpotPropertyType.Commercial))
                    return "The bustling commerce provides many things to notice.";
            }

            return "There might be something worth observing here.";
        }
    }
}