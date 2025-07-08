public class JournalEntry
{
    public string Title { get; set; }                 // "Forest Edge Master"
    public string Description { get; set; }           // "Your deep understanding of the forest's edges gives you special insight..."
    public EntryType Type { get; set; }               // Place, Bond, or Insight
    public DateTime DiscoveryDate { get; set; }       // When this was recorded
    public PermanentEffect Effect { get; set; }       // The lasting benefit
    public List<string> RelatedEntries { get; set; }  // Connected journal entries
}