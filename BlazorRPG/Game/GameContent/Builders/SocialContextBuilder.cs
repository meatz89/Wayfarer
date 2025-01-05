public class SocialContextBuilder
{
    private LegalityTypes legality = LegalityTypes.Illegal;
    private TensionState tension = TensionState.Relaxed;

    public SocialContextBuilder WithLegality(LegalityTypes legality)
    {
        this.legality = legality;
        return this;
    }

    public SocialContextBuilder WithTension(TensionState tension)
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
