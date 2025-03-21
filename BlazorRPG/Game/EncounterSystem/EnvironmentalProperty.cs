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
    string ForDisplay();
}

public class Illumination : IEnvironmentalProperty
{
    public static Illumination Bright = new Illumination("Bright");
    public static Illumination Shadowy = new Illumination("Shadowy");
    public static Illumination Dark = new Illumination("Dark");

    private string Value;

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

    public string ForDisplay()
    {
        return $"{GetPropertyType()}: {GetPropertyValue()}";
    }
}

public class Population : IEnvironmentalProperty
{
    public static Population Crowded = new Population("Crowded");
    public static Population Quiet = new Population("Quiet");
    public static Population Isolated = new Population("Isolated");

    private string Value;

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

    public string ForDisplay()
    {
        return $"{GetPropertyType()}: {GetPropertyValue()}";
    }
}


public class Economic : IEnvironmentalProperty
{
    public static Economic Wealthy = new Economic("Wealthy");
    public static Economic Commercial = new Economic("Commercial");
    public static Economic Humble = new Economic("Humble");

    private string Value;

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

    public string ForDisplay()
    {
        return $"{GetPropertyType()}: {GetPropertyValue()}";
    }
}


public class Physical : IEnvironmentalProperty
{
    public static Physical Confined = new Physical("Confined");
    public static Physical Expansive = new Physical("Expansive");
    public static Physical Hazardous = new Physical("Hazardous");

    private string Value;

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

    public string ForDisplay()
    {
        return $"{GetPropertyType()}: {GetPropertyValue()}";
    }
}

public class Atmosphere : IEnvironmentalProperty
{
    public static Atmosphere Tense = new Atmosphere("Tense");
    public static Atmosphere Formal = new Atmosphere("Formal");
    public static Atmosphere Chaotic = new Atmosphere("Chaotic");

    private string Value;

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

    public string ForDisplay()
    {
        return $"{GetPropertyType()}: {GetPropertyValue()}";
    }
}
