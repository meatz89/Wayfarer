using System;
using System.Collections.Generic;
using System.Linq;

public static class LocationTraitsParser
{
    /// <summary>
    /// Parse Venue traits from environmental properties and domain tags
    /// Location is the gameplay entity with all mechanical properties
    /// </summary>
    public static List<string> ParseLocationTraits(Location location, TimeBlocks currentTime)
    {
        List<string> traits = new List<string>();

        if (location == null)
            return traits;

        // Parse time-specific LocationPropertyType enums from TimeSpecificProperties list
        if (location.TimeSpecificProperties.Any(t => t.TimeBlock == currentTime))
        {
            List<LocationPropertyType> properties = location.TimeSpecificProperties.First(t => t.TimeBlock == currentTime).Properties;
            foreach (LocationPropertyType prop in properties)
            {
                string trait = prop.ToString();
                if (!string.IsNullOrEmpty(trait) && !traits.Contains(trait))
                {
                    traits.Add(trait);
                }
            }
        }

        // Parse domain tags from Location
        if (location.DomainTags != null)
        {
            foreach (string tag in location.DomainTags)
            {
                string trait = tag?.ToUpper() switch
                {
                    "COMMERCE" => "Diplomacy Hub",
                    "SOCIAL" => "Social Gathering",
                    "PUBLIC" => "Public Square",
                    "NOBLE" => "Noble District",
                    "WEALTH" => "Affluent Area",
                    "EXCLUSIVE" => "Exclusive Access",
                    "HOME" => "Home Base",
                    "REST" => "Rest Area",
                    "PRIVATE" => "Private Space",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(trait) && !traits.Contains(trait))
                {
                    traits.Add(trait);
                }
            }
        }

        // Add location-type specific traits from Location
        string locationTypeTrait = location.LocationType.ToString();
        if (!string.IsNullOrEmpty(locationTypeTrait) && !traits.Contains(locationTypeTrait))
        {
            traits.Add(locationTypeTrait);
        }

        // Limit to 3-4 most relevant traits (as per mockup)
        return traits.Take(4).ToList();
    }

}