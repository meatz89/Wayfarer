namespace BlazorRPG.Pages;

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

    public static string GetIconForTimeWindow(TimeWindowTypes time)
    {
        return time switch
        {
            TimeWindowTypes.Night => "🌙",
            TimeWindowTypes.Morning => "🌄",
            TimeWindowTypes.Afternoon => "☀️",
            TimeWindowTypes.Evening => "🌆",
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

    public static string GetItemIcon(ItemTypes itemType)
    {
        return itemType switch
        {
            ItemTypes.Sword => "⚔️",
            ItemTypes.Shield => "🛡️",
            ItemTypes.Bow => "🏹",
            ItemTypes.Snares => "🪶",
            ItemTypes.Dagger => "🔪",
            ItemTypes.Lockpicks => "🗝️",
            ItemTypes.Journal => "📚",
            ItemTypes.Spectacles => "📜",
            ItemTypes.WaxSealKit => "🎵",
            ItemTypes.GrapplingHook => "🧶",
            ItemTypes.FlintAndSteel => "🍖",
            ItemTypes.Chainmail => "👕",
            ItemTypes.QuillAndInk => "✒️",
            ItemTypes.SkinningKnife => "🔪",
            ItemTypes.HerbPouch => "🍃",
            ItemTypes.FineClothes => "👘",
            ItemTypes.WineFlask => "🍷",
            ItemTypes.DarkCloak => "⛏️",
            _ => "📦"
        };
    }

    public static string GetItemDescription(ItemTypes itemType)
    {
        return itemType switch
        {
            ItemTypes.Sword => "A sturdy steel sword",
            ItemTypes.Shield => "A wooden shield with metal binding",
            ItemTypes.Bow => "A hunting bow made of yew",
            ItemTypes.Snares => "Sharp arrows with fletching",
            ItemTypes.Dagger => "A small but sharp blade",
            ItemTypes.Lockpicks => "Tools for picking locks",
            ItemTypes.Journal => "A tome of knowledge",
            ItemTypes.Spectacles => "A rolled parchment with writing",
            ItemTypes.WaxSealKit => "A stringed musical instrument",
            ItemTypes.GrapplingHook => "Strong hemp rope",
            ItemTypes.FlintAndSteel => "Dried food for travel",
            ItemTypes.Chainmail => "Protective leather garments",
            ItemTypes.QuillAndInk => "Quill, ink and parchment",
            ItemTypes.SkinningKnife => "A knife for skinning game",
            ItemTypes.HerbPouch => "Medicinal plants",
            ItemTypes.FineClothes => "Well-made attire suitable for performance",
            ItemTypes.WineFlask => "A bottle of reasonably good wine",
            ItemTypes.DarkCloak => "Tools for scaling walls",
            _ => "A common item"
        };
    }
    public static string GetTimeOfDayStyle(TimeWindowTypes currentTime)
    {
        return currentTime switch
        {
            TimeWindowTypes.Morning => "time-morning",
            TimeWindowTypes.Afternoon => "time-afternoon",
            TimeWindowTypes.Evening => "time-evening",
            TimeWindowTypes.Night => "time-night",
            _ => ""
        };
    }
}