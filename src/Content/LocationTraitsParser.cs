using System;
using System.Collections.Generic;
using System.Linq;

public static class LocationTraitsParser
{
    /// <summary>
    /// Parse location traits from environmental properties and domain tags
    /// </summary>
    public static List<string> ParseLocationTraits(Location location, TimeBlocks currentTime)
    {
        List<string> traits = new List<string>();

        if (location == null)
            return traits;

        // Parse environmental properties for current time based on TimeBlocks
        List<string> properties = currentTime switch
        {
            TimeBlocks.Dawn => location.MorningProperties ?? new List<string>(),
            TimeBlocks.Morning => location.MorningProperties ?? new List<string>(),
            TimeBlocks.Midday => location.AfternoonProperties ?? new List<string>(),
            TimeBlocks.Afternoon => location.EveningProperties ?? new List<string>(),
            TimeBlocks.Evening => location.NightProperties ?? new List<string>(),
            TimeBlocks.Night => location.NightProperties ?? new List<string>(),
            _ => location.AfternoonProperties ?? new List<string>()
        };

        foreach (string prop in properties)
        {
            string trait = prop.ToString(); // Direct conversion
            if (!string.IsNullOrEmpty(trait) && !traits.Contains(trait))
            {
                traits.Add(trait);
            }
        }

        // Parse domain tags (these are strings in Location)
        if (location.DomainTags != null)
        {
            foreach (string tag in location.DomainTags)
            {
                string trait = tag?.ToUpper() switch
                {
                    "COMMERCE" => "Commerce Hub",
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

        // Add location-type specific traits
        string locationTypeTrait = !string.IsNullOrEmpty(location.LocationTypeString) ? location.LocationTypeString : "";
        if (!string.IsNullOrEmpty(locationTypeTrait) && !traits.Contains(locationTypeTrait))
        {
            traits.Add(locationTypeTrait);
        }

        // Limit to 3-4 most relevant traits (as per mockup)
        return traits.Take(4).ToList();
    }




}