public class SocialContextBuilder
{
    private AuthorityTypes authority = AuthorityTypes.Official;
    private FormalityTypes formality = FormalityTypes.Formal;
    private LegalityTypes legality = LegalityTypes.Illegal;
    private TensionState tension = TensionState.Relaxed;

    public SocialContextBuilder WithAuthority(AuthorityTypes authority)
    {
        this.authority = authority;
        return this;
    }

    public SocialContextBuilder WithFormality(FormalityTypes formality)
    {
        this.formality = formality;
        return this;
    }

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
            Authority = authority,
            Formality = formality,
            Legality = legality,
            Tension = tension
        };
    }
}
