public enum EncounterStateCategory
{
    Standard,    // Normal progression with balanced choices
    Critical,    // High pressure (≥7) requires careful management
    Escalation   // Low outcome in later stages (stage ≥3, outcome <7) needs pushing for victory
}

public class EncounterStateAnalysis
{
    public EncounterStateCategory StateCategory { get; private set; }

    public static EncounterStateAnalysis Analyze(
        EncounterValues values,
        int stageNumber,
        PlayerState player)
    {
        EncounterStateAnalysis analysis = new();

        // Determine state category based on pressure and outcome progression
        if (values.Pressure >= 7)
        {
            // Critical state when pressure is high, forcing careful choices
            analysis.StateCategory = EncounterStateCategory.Critical;
        }
        else if (stageNumber >= 3 && values.Outcome < 7)
        {
            // Escalation state when we're not making enough progress
            analysis.StateCategory = EncounterStateCategory.Escalation;
        }
        else
        {
            // Standard state for normal progression
            analysis.StateCategory = EncounterStateCategory.Standard;
        }

        return analysis;
    }
}