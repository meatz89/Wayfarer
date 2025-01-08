public class SocialContext
{
    public LegalityTypes Legality { get; set; }
    public PressureStateTypes Pressure { get; set; }
}

public enum LegalityTypes
{
    Legal, Gray, Illegal
}

public enum PressureStateTypes
{
    Relaxed, Alert, Hostile,
}

public enum GroupSize
{
    Solo, Small, Large
}

