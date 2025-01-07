public class SocialContextBuilder
{
    private LegalityTypes legality = LegalityTypes.Legal;
    private TensionStateTypes tension = TensionStateTypes.Relaxed;

    public SocialContextBuilder WithLegality(LegalityTypes legality)
    {
        this.legality = legality;
        return this;
    }

    public SocialContextBuilder WithTension(TensionStateTypes tension)
    {
        this.tension = tension;
        return this;
    }

    public SocialContext Build()
    {
        return new SocialContext
        {
            Legality = legality,
            Tension = tension
        };
    }
}
