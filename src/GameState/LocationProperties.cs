using System.Collections.Generic;

// Location property enums for categorical mechanics

public interface ILocationProperty
{
    string GetDescription();
}

public enum Population
{
    Quiet,
    Crowded,
    Scholarly
}

public enum Atmosphere
{
    Calm,
    Guarded,
    Formal,
    Chaotic
}

public enum Physical
{
    Confined,
    Expansive,
    Hazardous
}

public enum Illumination
{
    Bright,
    Thiefy,  // Shadows for thieves
    Dark
}

// Extension methods for property values
public static class LocationPropertyExtensions
{
    public static string GetPropertyValue(this Population pop)
    {
        return pop switch
        {
            Population.Quiet => "quiet",
            Population.Crowded => "crowded",
            Population.Scholarly => "scholarly",
            _ => "normal"
        };
    }

    public static string GetPropertyValue(this Atmosphere atm)
    {
        return atm switch
        {
            Atmosphere.Calm => "calm",
            Atmosphere.Guarded => "guarded",
            Atmosphere.Formal => "formal",
            Atmosphere.Chaotic => "chaotic",
            _ => "normal"
        };
    }

    public static string GetPropertyValue(this Physical phys)
    {
        return phys switch
        {
            Physical.Confined => "confined",
            Physical.Expansive => "expansive",
            Physical.Hazardous => "hazardous",
            _ => "normal"
        };
    }

    public static string GetPropertyValue(this Illumination ill)
    {
        return ill switch
        {
            Illumination.Bright => "bright",
            Illumination.Thiefy => "shadowy",
            Illumination.Dark => "dark",
            _ => "normal"
        };
    }
}