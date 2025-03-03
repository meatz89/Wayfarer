
/// <summary>
/// Represents the strategic signature of an encounter
/// </summary>
public class StrategicSignature
{
    public int Dominance { get; set; }
    public int Rapport { get; set; }
    public int Analysis { get; set; }
    public int Precision { get; set; }
    public int Concealment { get; set; }

    public StrategicSignature()
    {
        Dominance = 0;
        Rapport = 0;
        Analysis = 0;
        Precision = 0;
        Concealment = 0;
    }

    public StrategicSignature(StrategicSignature source)
    {
        Dominance = source.Dominance;
        Rapport = source.Rapport;
        Analysis = source.Analysis;
        Precision = source.Precision;
        Concealment = source.Concealment;
    }

    /// <summary>
    /// Get element value by type
    /// </summary>
    public int GetElementValue(SignatureElementTypes elementType)
    {
        switch (elementType)
        {
            case SignatureElementTypes.Dominance:
                return Dominance;
            case SignatureElementTypes.Rapport:
                return Rapport;
            case SignatureElementTypes.Analysis:
                return Analysis;
            case SignatureElementTypes.Precision:
                return Precision;
            case SignatureElementTypes.Concealment:
                return Concealment;
            default:
                throw new ArgumentException("Invalid element type");
        }
    }

    /// <summary>
    /// Set element value by type (clamped between 0-5)
    /// </summary>
    public void SetElementValue(SignatureElementTypes elementType, int value)
    {
        // Ensure value is between 0 and 5
        value = Math.Max(0, Math.Min(5, value));

        switch (elementType)
        {
            case SignatureElementTypes.Dominance:
                Dominance = value;
                break;
            case SignatureElementTypes.Rapport:
                Rapport = value;
                break;
            case SignatureElementTypes.Analysis:
                Analysis = value;
                break;
            case SignatureElementTypes.Precision:
                Precision = value;
                break;
            case SignatureElementTypes.Concealment:
                Concealment = value;
                break;
            default:
                throw new ArgumentException("Invalid element type");
        }
    }

    /// <summary>
    /// Increment element value by type
    /// </summary>
    public void IncrementElement(SignatureElementTypes elementType, int amount = 1)
    {
        int currentValue = GetElementValue(elementType);
        SetElementValue(elementType, currentValue + amount);
    }

    /// <summary>
    /// Map approach type to signature element type
    /// </summary>
    public static SignatureElementTypes ApproachToElement(ApproachTypes approach)
    {
        switch (approach)
        {
            case ApproachTypes.Force:
                return SignatureElementTypes.Dominance;
            case ApproachTypes.Charm:
                return SignatureElementTypes.Rapport;
            case ApproachTypes.Wit:
                return SignatureElementTypes.Analysis;
            case ApproachTypes.Finesse:
                return SignatureElementTypes.Precision;
            case ApproachTypes.Stealth:
                return SignatureElementTypes.Concealment;
            default:
                throw new ArgumentException("Invalid approach type");
        }
    }
}

