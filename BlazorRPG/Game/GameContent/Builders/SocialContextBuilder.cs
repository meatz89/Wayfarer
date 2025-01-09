public class SocialPropertiesBuilder
{
    private LegalityTypes legality = LegalityTypes.Legal;
    private PressureStateTypes pressure = PressureStateTypes.Relaxed;

    public SocialPropertiesBuilder WithLegality(LegalityTypes legality)
    {
        this.legality = legality;
        return this;
    }

    public SocialPropertiesBuilder WithPressure(PressureStateTypes pressure)
    {
        this.pressure = pressure;
        return this;
    }

    public SocialProperties Build()
    {
        return new SocialProperties
        {
            Legality = legality,
            Pressure = pressure
        };
    }
}
