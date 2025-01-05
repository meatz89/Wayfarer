public class ActivityPropertiesBuilder
{
    private ComplexityTypes complexity = ComplexityTypes.Simple;
    private DurationTypes duration = DurationTypes.Brief;
    private IntensityTypes intensity = IntensityTypes.Low;
    private NoiseTypes noise = NoiseTypes.Quiet;

    public ActivityPropertiesBuilder WithComplexity(ComplexityTypes complexity)
    {
        this.complexity = complexity;
        return this;
    }

    public ActivityPropertiesBuilder WithDuration(DurationTypes duration)
    {
        this.duration = duration;
        return this;
    }

    public ActivityPropertiesBuilder WithIntensity(IntensityTypes intensity)
    {
        this.intensity = intensity;
        return this;
    }

    public ActivityPropertiesBuilder WithNoise(NoiseTypes noise)
    {
        this.noise = noise;
        return this;
    }

    public ActivityProperties Build()
    {
        return new ActivityProperties
        {
            Complexity = complexity,
            Duration = duration,
            Intensity = intensity,
            Noise = noise
        };
    }
}