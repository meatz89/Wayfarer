public class ActivityPropertiesBuilder
{
    private ComplexityTypes complexity = ComplexityTypes.Simple;

    public ActivityPropertiesBuilder WithComplexity(ComplexityTypes complexity)
    {
        this.complexity = complexity;
        return this;
    }

    public ActivityProperties Build()
    {
        return new ActivityProperties
        {
            Complexity = complexity,
        };
    }
}