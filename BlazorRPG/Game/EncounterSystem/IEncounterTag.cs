

public interface IEncounterTag
{
    string Name { get; }
    bool IsActive(BaseTagSystem tagSystem);
    void ApplyEffect(EncounterState state);
}
