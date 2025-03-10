
using BlazorRPG.Game.EncounterManager;

/// <summary>
/// Example of how to use the Wayfarer Encounter System with projections
/// </summary>
public class WayfarerEncounterProjectionExample
{
    public static void RunExample(Encounter encounterManager)
    {
        // Run a simplified encounter loop with projections
        while (true)
        {
            // Get encounter status
            EncounterStatus status = encounterManager.GetEncounterStatus();
            Console.WriteLine($"Turn {status.CurrentTurn}/{status.MaxTurns}");
            Console.WriteLine($"Momentum: {status.Momentum}, Pressure: {status.Pressure}");

            // Display active tags
            Console.WriteLine("Active Tags: " + string.Join(", ", status.ActiveTagNames));

            // Display approach and focus tags
            Console.WriteLine("Approach Tags:");
            foreach (KeyValuePair<ApproachTags, int> tag in status.ApproachTags)
                Console.WriteLine($"  {tag.Key}: {tag.Value}");

            Console.WriteLine("Focus Tags:");
            foreach (KeyValuePair<FocusTags, int> tag in status.FocusTags)
                Console.WriteLine($"  {tag.Key}: {tag.Value}");

            // Get projections for all available choices
            List<ChoiceProjection> projections = encounterManager.GetChoiceProjections();

            // Display choices to player with projected outcomes
            Console.WriteLine("\nAvailable Choices:");
            for (int i = 0; i < projections.Count; i++)
            {
                ChoiceProjection projection = projections[i];
                IChoice choice = projection.Choice;
                string description = encounterManager.GetFormattedChoiceDescription(choice);
                string effectType = choice.EffectType == EffectTypes.Momentum ? "Momentum" : "Pressure";

                Console.WriteLine($"{i + 1}. {choice.Name} ({effectType})");
                Console.WriteLine($"   {description}");

                // Show projected effects
                Console.WriteLine($"   Momentum: +{projection.MomentumGained} (Total: {projection.FinalMomentum})");
                Console.WriteLine($"   Pressure: +{projection.PressureBuilt} (Total: {projection.FinalPressure})");

                // Show tag changes
                if (projection.ApproachTagChanges.Count > 0 || projection.FocusTagChanges.Count > 0)
                {
                    Console.WriteLine("   Tag Changes:");
                    foreach (KeyValuePair<ApproachTags, int> tagChange in projection.ApproachTagChanges)
                        Console.WriteLine($"     {tagChange.Key}: {(tagChange.Value > 0 ? "+" : "")}{tagChange.Value}");
                    foreach (KeyValuePair<FocusTags, int> tagChange in projection.FocusTagChanges)
                        Console.WriteLine($"     {tagChange.Key}: {(tagChange.Value > 0 ? "+" : "")}{tagChange.Value}");
                }

                // Show newly activated tags
                if (projection.NewlyActivatedTags.Count > 0)
                {
                    Console.WriteLine("   Newly Activated Tags:");
                    foreach (string tag in projection.NewlyActivatedTags)
                        Console.WriteLine($"     {tag}");
                }
            }

            // Simulate player selection (in a real game, this would come from UI)
            int selectedIndex = 0; // Always pick the first choice for this example
            ChoiceProjection selectedProjection = projections[selectedIndex];

            // Apply the choice projection
            ChoiceOutcome outcome = encounterManager.ApplyChoiceProjection(selectedProjection);
            Console.WriteLine("\nOutcome:");
            Console.WriteLine(outcome.Description);

            if (outcome.MomentumGain > 0)
                Console.WriteLine($"Gained {outcome.MomentumGain} momentum");

            if (outcome.PressureGain > 0)
                Console.WriteLine($"Built {outcome.PressureGain} pressure");

            // Check if encounter is over
            if (outcome.IsEncounterOver)
            {
                Console.WriteLine($"\nEncounter Over: {outcome.Outcome}");
                break;
            }

            Console.WriteLine("\n------------------------------\n");
        }
    }
}