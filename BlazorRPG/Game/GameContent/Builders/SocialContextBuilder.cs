public class SocialContextBuilder
{
    private LegalityTypes legality = LegalityTypes.Legal;
    private PressureStateTypes pressure = PressureStateTypes.Relaxed;

    public SocialContextBuilder WithLegality(LegalityTypes legality)
    {
        this.legality = legality;
        return this;
    }

    public SocialContextBuilder WithPressure(PressureStateTypes pressure)
    {
        this.pressure = pressure;
        return this;
    }

    public SocialContext Build()
    {
        return new SocialContext
        {
            Legality = legality,
            Pressure = pressure
        };
    }
}
