
/// <summary>
/// Builder for creating encounter tags
/// </summary>
public class EncounterTagBuilder
{
    private string _id;
    private string _name;
    private string _description;
    private SignatureElementTypes _sourceElement;
    private int _thresholdValue;
    private TagEffect _effect;

    public EncounterTagBuilder()
    {
        _id = string.Empty;
        _name = string.Empty;
        _description = string.Empty;
        _sourceElement = SignatureElementTypes.Dominance;
        _thresholdValue = 3;
        _effect = new TagEffect();
    }

    public EncounterTagBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public EncounterTagBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public EncounterTagBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public EncounterTagBuilder WithSourceElement(SignatureElementTypes element)
    {
        _sourceElement = element;
        return this;
    }

    public EncounterTagBuilder WithThreshold(int threshold)
    {
        _thresholdValue = threshold;
        return this;
    }

    public EncounterTagBuilder WithEffect(TagEffect effect)
    {
        _effect = effect;
        return this;
    }

    public EncounterTag Build()
    {
        return new EncounterTag(_id, _name, _description, _sourceElement, _thresholdValue, _effect);
    }
}
