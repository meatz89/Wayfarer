using System;
using System.Collections.Generic;
using System.Linq;

public static class LocationTraitsParser
{
    /// <summary>
    /// Parse location traits from environmental properties and domain tags
    /// LocationSpot is the gameplay entity with all mechanical properties
    /// </summary>
    public static List<string> ParseLocationTraits(LocationSpot spot, TimeBlocks currentTime)
    {
        List<string> traits = new List<string>();

        if (spot == null)
            return traits;

        // Parse time-specific SpotPropertyType enums from TimeSpecificProperties dictionary
        if (spot.TimeSpecificProperties.ContainsKey(currentTime))
        {
            List<SpotPropertyType> properties = spot.TimeSpecificProperties[currentTime];
            foreach (SpotPropertyType prop in properties)
            {
                string trait = prop.ToString();
                if (!string.IsNullOrEmpty(trait) && !traits.Contains(trait))
                {
                    traits.Add(trait);
                }
            }
        }

        // Parse domain tags from LocationSpot
        if (spot.DomainTags != null)
        {
            foreach (string tag in spot.DomainTags)
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

        // Add location-type specific traits from LocationSpot
        string locationTypeTrait = spot.LocationType.ToString();
        if (!string.IsNullOrEmpty(locationTypeTrait) && !traits.Contains(locationTypeTrait))
        {
            traits.Add(locationTypeTrait);
        }

        // Limit to 3-4 most relevant traits (as per mockup)
        return traits.Take(4).ToList();
    }




}