public interface IEncounterTag
{
    string NarrativeName { get; }
    bool IsActive(EncounterTagSystem tagSystem);
    void ApplyEffect(EncounterState state);
}
