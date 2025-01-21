public class ChoiceTemplateBuilder
{
    private ChoiceArchetypes archetype;
    private ChoiceApproaches approach;
    private EnergyTypes energyType;

    public ChoiceTemplateBuilder WithArchetype(ChoiceArchetypes archetype)
    {
        this.archetype = archetype;
        this.energyType = archetype switch
        {
            ChoiceArchetypes.Physical => EnergyTypes.Physical,
            ChoiceArchetypes.Focus => EnergyTypes.Focus,
            ChoiceArchetypes.Social => EnergyTypes.Social,
            _ => throw new ArgumentOutOfRangeException(nameof(archetype),
                "All choice archetypes must map to an energy type")
        };
        return this;
    }
    public ChoiceTemplateBuilder WithApproach(ChoiceApproaches approach)
    {
        this.approach = approach;
        return this;
    }

    public ChoiceTemplate Build()
    {
        return new ChoiceTemplate
        {
            ChoiceArchetype = archetype,
            ChoiceApproach = approach,
            EnergyType = energyType,
        };
    }
}