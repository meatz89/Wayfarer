public class TagManager
{
    public EncounterTagSystem EncounterTagSystem { get; }
    public List<IEncounterTag> EncounterTags { get; }

    private readonly Dictionary<FocusTags, int> _focusMomentumBonuses = new Dictionary<FocusTags, int>();
    private readonly Dictionary<FocusTags, int> _focusPressureModifiers = new Dictionary<FocusTags, int>();

    public TagManager()
    {
        EncounterTagSystem = new EncounterTagSystem();
        EncounterTags = new List<IEncounterTag>();
        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
        {
            _focusMomentumBonuses[focus] = 0;
            _focusPressureModifiers[focus] = 0;
        }
    }

    public void CreateEncounterTags(List<IEncounterTag> locationTags)
    {
        HashSet<string> previouslyActive = new(EncounterTags.Select(t => t.NarrativeName));

        EncounterTags.Clear();
        ResetTagEffects();

        foreach (IEncounterTag tag in locationTags)
        {
            bool shouldActivate = tag is StrategicTag || (tag is NarrativeTag);

            if (shouldActivate)
            {
                EncounterTags.Add(tag);
            }
        }
    }

    public void ResetTagEffects()
    {
        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
            _focusMomentumBonuses[focus] = 0;

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
            _focusPressureModifiers[focus] = 0;

    }

    public int GetTotalMomentum(CardDefinition choice, int baseMomentum)
    {
        int total = baseMomentum;

        if (_focusMomentumBonuses.ContainsKey(choice.Focus))
            total += _focusMomentumBonuses[choice.Focus];

        return total;
    }

    public int GetTotalPressure(CardDefinition choice, int basePressure)
    {
        int total = basePressure;

        if (_focusPressureModifiers.ContainsKey(choice.Focus))
            total += _focusPressureModifiers[choice.Focus];

        return Math.Max(0, total);
    }

    public void AddFocusMomentumBonus(FocusTags focus, int bonus)
    {
        if (!_focusMomentumBonuses.ContainsKey(focus))
            _focusMomentumBonuses[focus] = 0;

        _focusMomentumBonuses[focus] += bonus;
    }

    public void AddFocusPressureModifier(FocusTags focus, int modifier)
    {
        if (!_focusPressureModifiers.ContainsKey(focus))
            _focusPressureModifiers[focus] = 0;

        _focusPressureModifiers[focus] += modifier;
    }

    public EncounterTagSystem CloneTagSystem() => EncounterTagSystem.Clone();

    public List<StrategicTag> GetStrategicActiveTags()
    {
        List<StrategicTag> list = EncounterTags
            .Where(x => x is StrategicTag strategicTag)
            .Select(x => (StrategicTag)x)
            .ToList();

        return list;
    }

    public List<NarrativeTag> GetNarrativeActiveTags()
    {
        List<NarrativeTag> list = EncounterTags
            .Where(x => x is NarrativeTag narrativeTag)
            .Select(x => (NarrativeTag)x)
            .ToList();

        return list;
    }
}
