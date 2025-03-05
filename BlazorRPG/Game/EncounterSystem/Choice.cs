public class Choice
{
    public string Id { get; }
    public string Name { get; }
    public ApproachTypes ApproachType { get; }
    public FocusTypes FocusType { get; }
    public EffectTypes EffectType { get; }

    public Choice(string id, string name, ApproachTypes approach, FocusTypes focus, EffectTypes effect)
    {
        Id = id;
        Name = name;
        ApproachType = approach;
        FocusType = focus;
        EffectType = effect;
    }

    public override string ToString()
    {
        return $"{Name} ({EffectType}, {ApproachType}, {FocusType})";
    }
}