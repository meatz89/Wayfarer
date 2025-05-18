/// <summary>
/// 
/// ### Example Location Combinations
/// 
/// **Market Square: **
/// -**Morning * *: Bright + Crowded + Commercial + Chaotic
/// - **Afternoon * *: Bright + Crowded + Commercial + Tense
/// - **Evening * *: Roguey + Crowded + Commercial + Chaotic
/// - **Night * *: Dark + Quiet + Commercial + Tense
/// 
/// **Forest Path: **
/// -**Day * *: Bright + Isolated + Expansive + Hazardous
/// - **Dusk * *: Roguey + Isolated + Expansive + Tense
/// - **Night * *: Dark + Isolated + Expansive + Hazardous
/// 
/// Illumination (Bright, Roguey, Dark)
/// Population (Crowded, Quiet, Scholarly)
/// Physical (Confined, Expansive, Hazardous)
/// Atmosphere (Tense, Formal, Chaotic)
/// 
/// </summary>
public interface ILocationProperty
{
    string GetPropertyType();
    string GetPropertyValue();
}
public class Illumination : ILocationProperty, IEquatable<Illumination>
{
    public static ILocationProperty Any
    {
        get
        {
            return new LocationPropertyAny(nameof(Illumination));
        }
    }

    public static Illumination Bright = new Illumination("Bright");
    public static Illumination Roguey = new Illumination("Roguey");
    public static Illumination Dark = new Illumination("Dark");

    private string Value;

    public Illumination(string value)
    {
        this.Value = value;
    }

    public bool Equals(Illumination other)
    {
        if (other == null)
            return false;
        return this.Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        // Handle EnvironmentalPropertyAny - let it handle the comparison
        if (obj is LocationPropertyAny any)
            return any.Equals(this);

        // Handle same type comparison
        if (obj is Illumination other)
            return Equals(other);

        return false;
    }

    public string GetPropertyType()
    {
        return nameof(Illumination);
    }

    public string GetPropertyValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"{GetPropertyValue()}";
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

public class Population : ILocationProperty, IEquatable<Population>
{
    public static ILocationProperty Any
    {
        get
        {
            return new LocationPropertyAny(nameof(Population));
        }
    }

    public static Population Crowded = new Population("Crowded");
    public static Population Quiet = new Population("Quiet");
    public static Population Scholarly = new Population("Scholarly");

    private string Value;

    public Population(string value)
    {
        this.Value = value;
    }

    public bool Equals(Population other)
    {
        if (other == null)
            return false;
        return this.Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        // Handle EnvironmentalPropertyAny - let it handle the comparison
        if (obj is LocationPropertyAny any)
            return any.Equals(this);

        // Handle same type comparison
        if (obj is Population other)
            return Equals(other);

        return false;
    }

    public string GetPropertyType()
    {
        return nameof(Population);
    }

    public string GetPropertyValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"{GetPropertyValue()}";
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

public class Physical : ILocationProperty, IEquatable<Physical>
{
    public static ILocationProperty Any
    {
        get
        {
            return new LocationPropertyAny(nameof(Physical));
        }
    }

    public static Physical Confined = new Physical("Confined");
    public static Physical Expansive = new Physical("Expansive");
    public static Physical Hazardous = new Physical("Hazardous");

    private string Value;

    public Physical(string value)
    {
        this.Value = value;
    }

    public bool Equals(Physical other)
    {
        if (other == null)
            return false;
        return this.Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        // Handle EnvironmentalPropertyAny - let it handle the comparison
        if (obj is LocationPropertyAny any)
            return any.Equals(this);

        // Handle same type comparison
        if (obj is Physical other)
            return Equals(other);

        return false;
    }

    public string GetPropertyType()
    {
        return nameof(Physical);
    }

    public string GetPropertyValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"{GetPropertyValue()}";
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

public class Atmosphere : ILocationProperty, IEquatable<Atmosphere>
{
    public static ILocationProperty Any
    {
        get
        {
            return new LocationPropertyAny(nameof(Atmosphere));
        }
    }

    public static Atmosphere Formal = new Atmosphere("Formal");
    public static Atmosphere Calm = new Atmosphere("Calm");
    public static Atmosphere Rough = new Atmosphere("Rough");
    public static Atmosphere Tense = new Atmosphere("Tense");
    public static Atmosphere Chaotic = new Atmosphere("Chaotic");

    private string Value;

    public Atmosphere(string value)
    {
        this.Value = value;
    }

    public bool Equals(Atmosphere other)
    {
        if (other == null)
            return false;
        return this.Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        // Handle EnvironmentalPropertyAny - let it handle the comparison
        if (obj is LocationPropertyAny any)
            return any.Equals(this);

        // Handle same type comparison
        if (obj is Atmosphere other)
            return Equals(other);

        return false;
    }

    public string GetPropertyType()
    {
        return nameof(Atmosphere);
    }

    public string GetPropertyValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"{GetPropertyValue()}";
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}