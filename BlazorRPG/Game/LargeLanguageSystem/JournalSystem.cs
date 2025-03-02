public class JournalSystem
{
    public string Background { get; set; }
    public string InitialSituation { get; set; }
    public List<string> JourneyEntries { get; set; } = new();
    public EncounterHistory LastEncounter { get; set; }
    public List<EncounterHistory> EncounterHistory { get; set; } = new();
    private const string NewLine = "\r\n";

    public JournalSystem(GameContentProvider gameContentProvider)
    {
        Background = gameContentProvider.GetBackground;
        InitialSituation = gameContentProvider.GetInitialSituation;
    }

    public List<Narrative> GetDescriptionForCurrentEncounter()
    {
        List<Narrative> narratives = new();
        EncounterHistory Encounter = LastEncounter;
        foreach (Narrative narrative in Encounter.Narratives)
        {
            narratives.Add(narrative);
        }
        return narratives;
    }

    public List<string> GetInitialEncounterDescriptions()
    {
        List<string> descriptions = new List<string>();
        string description1 = Background + NewLine + InitialSituation;
        AddToList(descriptions, description1);

        foreach (string journeyEntry in JourneyEntries)
        {
            AddToList(descriptions, journeyEntry);
        }

        EncounterHistory Encounter = LastEncounter;
        string description2 = InitialSituation + NewLine + NewLine + Encounter.InitialGoal;
        AddToList(descriptions, description2);

        return descriptions;
    }

    public void WriteJourneyEntry(string narrative)
    {
        JourneyEntries.Add(narrative);
    }

    public void StartEncounter(string initialSituation, string actionGoal)
    {
        LastEncounter = new EncounterHistory()
        {
            InitialSituation = initialSituation,
            InitialGoal = actionGoal,
        };
    }


    public void EndEncounter(string resultSituation)
    {
        LastEncounter.ResultSituation = resultSituation;

        EncounterHistory.Add(LastEncounter);
        LastEncounter = null;
    }

    private void AddToList(List<string> list, string description)
    {
        if (!string.IsNullOrWhiteSpace(description)) list.Add(description);
    }

    public void NoteNewEncounterNarrative(string narrative)
    {
        Narrative item = new Narrative() { Role = Roles.user, Text = narrative };
        LastEncounter.Narratives.Add(item);
    }

    public void NoteNewEncounterAssistantNarrative(string assistantNarrative)
    {
        Narrative item = new Narrative() { Role = Roles.user, Text = assistantNarrative };
        LastEncounter.Narratives.Add(item);
    }

    public string GetCurrentEncounterGoal()
    {
        return LastEncounter.InitialGoal;
    }
}
