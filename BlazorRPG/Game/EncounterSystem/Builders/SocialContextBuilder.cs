public class SocialPropertiesBuilder
{
    private SupervisionTypes legality = SupervisionTypes.Unsupervised;
    private SupervisionTypes pressure = SupervisionTypes.Unsupervised;

    public SocialPropertiesBuilder WithLegality(SupervisionTypes legality)
    {
        this.legality = legality;
        return this;
    }

    public SocialPropertiesBuilder WithPressure(SupervisionTypes pressure)
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
