using System;
using System.Collections;

/// <summary>
/// 
/// ### Example Location Combinations
/// 
/// **Market Square: **
/// -**Morning * *: Bright + Crowded + Commercial + Chaotic
/// - **Afternoon * *: Bright + Crowded + Commercial + Tense
/// - **Evening * *: Shadowy + Crowded + Commercial + Chaotic
/// - **Night * *: Dark + Quiet + Commercial + Tense
/// 
/// **Forest Path: **
/// -**Day * *: Bright + Isolated + Expansive + Hazardous
/// - **Dusk * *: Shadowy + Isolated + Expansive + Tense
/// - **Night * *: Dark + Isolated + Expansive + Hazardous
/// 
/// </summary>
public interface IEnvironmentalProperty
{
    string GetPropertyType();
    string GetPropertyValue();
}

public class Illumination : IEnvironmentalProperty, IEquatable<Illumination>
{
    public static Illumination Bright = new Illumination("Bright");
    public static Illumination Shadowy = new Illumination("Shadowy");
    public static Illumination Dark = new Illumination("Dark");

    private string Value;

    public bool Equals(Illumination? other)
    {
        return Equals(other);
    }

    public override bool Equals(object? obj)
    {
        Illumination? other = (Illumination)obj;
        return this.Value == other.Value;
    }
    public Illumination(string value)
    {
        this.Value = value;
    }

    public string GetPropertyType()
    {
        return typeof(Illumination).Name;
    }

    public string GetPropertyValue()
    {
        return Value;
    }

    public string ToString()
    {
        return $"{GetPropertyValue()}";
    }
}

public class Population : IEnvironmentalProperty, IEquatable<Population>
{
    public static Population Crowded = new Population("Crowded");
    public static Population Quiet = new Population("Quiet");
    public static Population Isolated = new Population("Isolated");

    private string Value;

    public bool Equals(Population? other)
    {
        return Equals(other);
    }

    public override bool Equals(object? obj)
    {
        Population? other = (Population)obj;
        return this.Value == other.Value;
    }

    public Population(string value)
    {
        this.Value = value;
    }

    public string GetPropertyType()
    {
        return typeof(Population).Name;
    }

    public string GetPropertyValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"{GetPropertyValue()}";
    }
}

public class Economic : IEnvironmentalProperty, IEquatable<Economic>
{
    public static Economic Wealthy = new Economic("Wealthy");
    public static Economic Commercial = new Economic("Commercial");
    public static Economic Humble = new Economic("Humble");

    private string Value;

    public bool Equals(Economic? other)
    {
        return Equals(other);
    }

    public override bool Equals(object? obj)
    {
        Economic? other = (Economic)obj;
        return this.Value == other.Value;
    }

    public Economic(string value)
    {
        this.Value = value;
    }

    public string GetPropertyType()
    {
        return typeof(Economic).Name;
    }

    public string GetPropertyValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"{GetPropertyValue()}";
    }
}


public class Physical : IEnvironmentalProperty, IEquatable<Physical>
{
    public static Physical Confined = new Physical("Confined");
    public static Physical Expansive = new Physical("Expansive");
    public static Physical Hazardous = new Physical("Hazardous");

    private string Value;

    public bool Equals(Physical? other)
    {
        return Equals(other);
    }

    public override bool Equals(object? obj)
    {
        Physical? other = (Physical)obj;
        return this.Value == other.Value;
    }

    public Physical(string value)
    {
        this.Value = value;
    }

    public string GetPropertyType()
    {
        return typeof(Physical).Name;
    }

    public string GetPropertyValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"{GetPropertyValue()}";
    }
}

public class Atmosphere : IEnvironmentalProperty, IEquatable<Atmosphere>
{
    public static Atmosphere Tense = new Atmosphere("Tense");
    public static Atmosphere Formal = new Atmosphere("Formal");
    public static Atmosphere Chaotic = new Atmosphere("Chaotic");

    private string Value;

    public bool Equals(Atmosphere? other)
    {
        return Equals(other);
    }

    public override bool Equals(object? obj)
    {
        Atmosphere? other = (Atmosphere)obj;
        return this.Value == other.Value;
    }

    public Atmosphere(string value)
    {
        this.Value = value;
    }

    public string GetPropertyType()
    {
        return typeof(Atmosphere).Name;
    }

    public string GetPropertyValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"{GetPropertyValue()}";
    }
}
