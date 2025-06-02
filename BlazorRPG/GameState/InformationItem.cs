public class InformationItem
{
    public string Key { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public List<string> Tags { get; private set; } = new List<string>();
    public bool IsSecretKnowledge { get; private set; }
    public int DiscoveryDay { get; private set; }

    public InformationItem(string key, string title, string content, List<string> tags, bool isSecretKnowledge)
    {
        Key = key;
        Title = title;
        Content = content;
        Tags = tags;
        IsSecretKnowledge = isSecretKnowledge;
        DiscoveryDay = GameWorld.CurrentDay;
    }
}