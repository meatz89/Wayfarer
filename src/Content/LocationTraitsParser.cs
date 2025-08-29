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
        var traits = new List<string>();
        
        if (location == null)
            return traits;
        
        // Parse environmental properties for current time based on TimeBlocks
        var properties = GetPropertiesForTime(location, currentTime);
        foreach (var prop in properties)
        {
            string trait = MapEnvironmentalPropertyToTrait(prop);
            if (!string.IsNullOrEmpty(trait) && !traits.Contains(trait))
            {
                traits.Add(trait);
            }
        }
        
        // Parse domain tags (these are strings in Location)
        if (location.DomainTags != null)
        {
            foreach (var tag in location.DomainTags)
            {
                string trait = MapDomainTagToTrait(tag);
                if (!string.IsNullOrEmpty(trait) && !traits.Contains(trait))
                {
                    traits.Add(trait);
                }
            }
        }
        
        // Add location-type specific traits
        string locationTypeTrait = GetLocationTypeTrait(location);
        if (!string.IsNullOrEmpty(locationTypeTrait) && !traits.Contains(locationTypeTrait))
        {
            traits.Add(locationTypeTrait);
        }
        
        // Limit to 3-4 most relevant traits (as per mockup)
        return traits.Take(4).ToList();
    }
    
    private static List<string> GetPropertiesForTime(Location location, TimeBlocks time)
    {
        return time switch
        {
            TimeBlocks.Dawn => location.MorningProperties ?? new List<string>(),
            TimeBlocks.Morning => location.MorningProperties ?? new List<string>(),
            TimeBlocks.Afternoon => location.AfternoonProperties ?? new List<string>(),
            TimeBlocks.Evening => location.EveningProperties ?? new List<string>(),
            TimeBlocks.Night => location.NightProperties ?? new List<string>(),
            TimeBlocks.LateNight => location.NightProperties ?? new List<string>(),
            _ => location.AfternoonProperties ?? new List<string>()
        };
    }
    
    private static string MapEnvironmentalPropertyToTrait(string prop)
    {
        return prop?.ToUpper() switch
        {
            "WELL-LIT" => "Well-Lit",
            "DIM" => "Dimly Lit",
            "DARK" => "Dark",
            "SHADOWY" => "Shadowy",
            "WARM" => "Warm",
            "COZY" => "Cozy",
            "BUSY" => "Busy",
            "CROWDED" => "Crowded",
            "QUIET" => "Quiet",
            "SOCIAL" => "Social",
            "URBAN" => "Urban",
            "COMMERCIAL" => "Commercial",
            "WEALTHY" => "Wealthy",
            "GUARDED" => "Guarded",
            "PRIVATE" => "Private",
            _ => ""
        };
    }
    
    private static string MapDomainTagToTrait(string tag)
    {
        return tag?.ToUpper() switch
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
    }
    
    private static string GetLocationTypeTrait(Location location)
    {
        // Use mechanical property instead of hardcoded checks
        if (!string.IsNullOrEmpty(location.LocationTypeString))
            return location.LocationTypeString;
            
        return "";
    }
}