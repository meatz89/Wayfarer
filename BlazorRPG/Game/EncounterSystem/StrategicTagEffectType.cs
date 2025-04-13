public enum StrategicTagEffectType
{
    // Beneficial effects
    IncreaseMomentum,    // Adds momentum proportional to approach value
    DecreasePressure,    // Reduces pressure proportional to approach value

    // Detrimental effects
    DecreaseMomentum,    // Reduces momentum proportional to approach value
    IncreasePressure,    // Adds pressure proportional to approach value

    IncreaseInjury       // Reduces Health/Concentration/Confidence proportional to approach value
}
