public class SocialContext
{
    public LegalityTypes Legality { get; set; }
    public TensionStateTypes Tension { get; set; }
}

public enum LegalityTypes
{
    Legal,
    Illegal
}

public enum TensionStateTypes
{
    Relaxed,
    Alert,
    Hostile,
}

