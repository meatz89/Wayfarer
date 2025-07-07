namespace BlazorRPG.UIHelpers;

public static class StyleHelper
{
    public static List<PropertyDisplay> GetSpotProperties(Location location, LocationSpot spot)
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        if (location.Population != null)
        {
            properties.Add(new(
            "📐",
            FormatEnumString(location.Population.ToString()),
            "",
            "",
            ""
            ));
        }

        if (location.Physical != null)
        {
            properties.Add(new(
            "🧩",
            FormatEnumString(location.Physical.ToString()),
            "",
            "",
            ""
            ));
        }

        if (location.Illumination != null)
        {
            properties.Add(new(
            "☀️",
            FormatEnumString(location.Illumination.ToString()),
            "",
            "",
            ""
            ));
        }

        return properties;
    }

    public static string FormatEnumString(string value)
    {
        return string.Concat(value
            .Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()))
            .Replace("Type", "")
            .Replace("Types", "");
    }

    public static string GetIconForTimeWindow(TimeBlocks time)
    {
        return time switch
        {
            TimeBlocks.Night => "🌙",
            TimeBlocks.Morning => "🌄",
            TimeBlocks.Afternoon => "☀️",
            TimeBlocks.Evening => "🌆",
            _ => "❓"
        };
    }
    public static string GetArchetypeIcon(Professions archetype)
    {
        return archetype switch
        {
            Professions.Warrior => "⚔️",
            Professions.Scholar => "📚",
            Professions.Thief => "🏹",
            Professions.Merchant => "🎵",
            Professions.Ranger => "🗝️",
            _ => "❓"
        };
    }

    public static string GetTimeBlocksStyle(TimeBlocks currentTime)
    {
        return currentTime switch
        {
            TimeBlocks.Morning => "time-morning",
            TimeBlocks.Afternoon => "time-afternoon",
            TimeBlocks.Evening => "time-evening",
            TimeBlocks.Night => "time-night",
            _ => ""
        };
    }
}