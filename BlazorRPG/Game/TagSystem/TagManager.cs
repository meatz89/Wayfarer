public class TagManager
{
    public List<IEncounterTag> EncounterTags { get; } = new List<IEncounterTag>();

    public TagManager()
    {
        EncounterTags = new List<IEncounterTag>();
    }

    public void CreateEncounterTags(List<IEncounterTag> locationTags)
    {
        HashSet<string> previouslyActive = new(EncounterTags.Select(t =>
        {
            return t.NarrativeName;
        }));

        EncounterTags.Clear();

        foreach (IEncounterTag tag in locationTags)
        {
            bool shouldActivate = tag is StrategicTag || (tag is NarrativeTag);

            if (shouldActivate)
            {
                EncounterTags.Add(tag);
            }
        }
    }

    public List<StrategicTag> GetStrategicActiveTags()
    {
        List<StrategicTag> list = EncounterTags
            .Where(x =>
            {
                return x is StrategicTag strategicTag;
            })
            .Select(x =>
            {
                return (StrategicTag)x;
            })
            .ToList();

        return list;
    }

    public List<NarrativeTag> GetNarrativeActiveTags()
    {
        List<NarrativeTag> list = EncounterTags
            .Where(x =>
            {
                return x is NarrativeTag narrativeTag;
            })
            .Select(x =>
            {
                return (NarrativeTag)x;
            })
            .ToList();

        return list;
    }
}
