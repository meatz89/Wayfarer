public class TagManager
{
    public List<IConversationTag> ConversationTags { get; } = new List<IConversationTag>();

    public TagManager()
    {
        ConversationTags = new List<IConversationTag>();
    }

    public void CreateConversationTags(List<IConversationTag> locationTags)
    {
        HashSet<string> previouslyActive = new(ConversationTags.Select(t =>
        {
            return t.NarrativeName;
        }));

        ConversationTags.Clear();

        foreach (IConversationTag tag in locationTags)
        {
            bool shouldActivate = tag is StrategicTag || (tag is NarrativeTag);

            if (shouldActivate)
            {
                ConversationTags.Add(tag);
            }
        }
    }

    public List<StrategicTag> GetStrategicActiveTags()
    {
        List<StrategicTag> list = ConversationTags
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
        List<NarrativeTag> list = ConversationTags
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
