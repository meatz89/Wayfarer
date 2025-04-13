public interface IEncounterTag
{
    string NarrativeName { get; }
    bool IsActive(BaseTagSystem tagSystem);
    void ApplyEffect(EncounterState state);
}
