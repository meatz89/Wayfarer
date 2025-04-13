public class TagManager
{
    public EncounterTagSystem EncounterTagSystem { get; }
    public List<IEncounterTag> ActiveTags { get; }

    private readonly Dictionary<FocusTags, int> _focusMomentumBonuses = new Dictionary<FocusTags, int>();
    private readonly Dictionary<FocusTags, int> _focusPressureModifiers = new Dictionary<FocusTags, int>();

    public TagManager()
    {
        EncounterTagSystem = new EncounterTagSystem();
        ActiveTags = new List<IEncounterTag>();
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

    public void UpdateActiveTags(IEnumerable<IEncounterTag> locationTags)
    {
        HashSet<string> previouslyActive = new HashSet<string>(ActiveTags.Select(t => t.NarrativeName));

        ActiveTags.Clear();
        ResetTagEffects();

        foreach (IEncounterTag tag in locationTags)
        {
            bool shouldActivate = tag is EnvironmentPropertyTag || (tag is NarrativeTag && tag.IsActive(EncounterTagSystem));

            if (shouldActivate)
            {
                ActiveTags.Add(tag);
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

    public int GetTotalMomentum(ChoiceCard choice, int baseMomentum)
    {
        int total = baseMomentum;

        if (_focusMomentumBonuses.ContainsKey(choice.Focus))
            total += _focusMomentumBonuses[choice.Focus];

        return total;
    }

    public int GetTotalPressure(ChoiceCard choice, int basePressure)
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

    public List<IEncounterTag> GetNewlyActivatedTags(EncounterTagSystem projectedTags, IEnumerable<IEncounterTag> locationTags)
    {
        List<IEncounterTag> newlyActivated = new List<IEncounterTag>();

        foreach (IEncounterTag tag in locationTags)
        {
            bool wasActive = ActiveTags.Any(t => t.NarrativeName == tag.NarrativeName);
            bool willBeActive = tag is EnvironmentPropertyTag || (tag is NarrativeTag && tag.IsActive(projectedTags));

            if (!wasActive && willBeActive)
            {
                newlyActivated.Add(tag);
            }
        }

        return newlyActivated;
    }

    public List<IEncounterTag> GetDeactivatedTags(EncounterTagSystem projectedTags, IEnumerable<IEncounterTag> locationTags)
    {
        List<IEncounterTag> deactivated = new List<IEncounterTag>();

        foreach (IEncounterTag tag in locationTags)
        {
            bool wasActive = ActiveTags.Any(t => t.NarrativeName == tag.NarrativeName);
            bool willBeActive = tag is EnvironmentPropertyTag || (tag is NarrativeTag && tag.IsActive(projectedTags));

            if (wasActive && !willBeActive)
            {
                deactivated.Add(tag);
            }
        }

        return deactivated;
    }

    public EncounterTagSystem CloneTagSystem() => EncounterTagSystem.Clone();

    public List<EnvironmentPropertyTag> GetStrategicActiveTags()
    {
        List<EnvironmentPropertyTag> list = ActiveTags
            .Where(x => x is EnvironmentPropertyTag strategicTag)
            .Select(x => (EnvironmentPropertyTag)x)
            .ToList();

        return list;
    }
}
