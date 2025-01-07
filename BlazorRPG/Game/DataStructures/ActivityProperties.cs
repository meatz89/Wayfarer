public class ActivityProperties
{
    public ComplexityTypes Complexity { get; set; }
}

public enum ComplexityTypes
{
    Complex, Simple
}

public enum Intensity
{
    Low, Medium, High
}

public enum Tools
{
    None, Simple, Complex
}
