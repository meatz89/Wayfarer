public class SocialPropertiesBuilder
{
    private Engagement legality = Engagement.Idle;
    private Engagement pressure = Engagement.Idle;

    public SocialPropertiesBuilder WithLegality(Engagement legality)
    {
        this.legality = legality;
        return this;
    }

    public SocialPropertiesBuilder WithPressure(Engagement pressure)
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
