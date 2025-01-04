
public class PersonalityBuilder
{
    private bool prefersDirect;
    private bool valuesTrust;
    private bool sharesKnowledge;

    public PersonalityBuilder PrefersDirect(bool prefersDirect)
    {
        this.prefersDirect = prefersDirect;
        return this;
    }

    public PersonalityBuilder ValuesTrust(bool valuesTrust)
    {
        this.valuesTrust = valuesTrust;
        return this;
    }

    public PersonalityBuilder SharesKnowledge(bool sharesKnowledge)
    {
        this.sharesKnowledge = sharesKnowledge;
        return this;
    }

    public CharacterPersonality Build()
    {
        return new CharacterPersonality();
    }

}