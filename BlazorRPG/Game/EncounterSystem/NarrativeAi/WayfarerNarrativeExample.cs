namespace BlazorRPG.Game.EncounterManager.NarrativeAi
{
    /// <summary>
    /// Example of how to use the Wayfarer Encounter System with GPT narrative generation
    /// </summary>
    public class WayfarerNarrativeExample
    {
        private const string apiKey = "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";

        public static async Task RunExampleAsync()
        {
            // Create the core components
            ChoiceRepository choiceRepository = new ChoiceRepository();
            CardSelectionAlgorithm cardSelector = new CardSelectionAlgorithm(choiceRepository);
            NarrativePresenter narrativePresenter = new NarrativePresenter();

            // Create encounter manager
            Encounter encounterManager = new Encounter(cardSelector, choiceRepository, narrativePresenter);

            // Create a location
            LocationInfo villageMarket = LocationFactory.CreateRoadsideInn();

            // Add special choices for this location
            SpecialChoice negotiatePriceChoice = new SpecialChoice(
                "Negotiate Better Price",
                "Use your market knowledge and rapport to secure a favorable deal",
                ApproachTypes.Charm,
                FocusTags.Resource,
                new List<TagModification>
                {
                    TagModification.ForApproach(ApproachTags.Rapport, 1),
                    TagModification.ForFocus(FocusTags.Resource, 2)
                },
                new List<Func<BaseTagSystem, bool>>
                {
                    ChoiceFactory.ApproachTagRequirement(ApproachTags.Rapport, 2),
                    ChoiceFactory.FocusTagRequirement(FocusTags.Resource, 2)
                }
            );

            choiceRepository.AddSpecialChoice(villageMarket.Name, negotiatePriceChoice);

            // Create the narrative AI service
            GPTNarrativeService narrativeService = new GPTNarrativeService(apiKey);

            // Start the encounter with narrative
            NarrativeResult initialResult = await encounterManager.StartEncounterWithNarrativeAsync(
                villageMarket,
                "decided to visit the market to purchase supplies",
                narrativeService);

            Console.WriteLine("=== Encounter Introduction ===");
            Console.WriteLine(initialResult.Narrative);
            Console.WriteLine();

            Console.WriteLine("=== Available Choices ===");
            for (int i = 0; i < initialResult.Choices.Count; i++)
            {
                IChoice choice = initialResult.Choices[i];
                string description = initialResult.ChoiceDescriptions[choice];
                ChoiceProjection projection = initialResult.Projections[i];

                string effectType = choice.EffectType == EffectTypes.Momentum ? "Momentum" : "Pressure";
                string effect = choice.EffectType == EffectTypes.Momentum
                    ? $"+{projection.MomentumGained} Momentum"
                    : $"+{projection.PressureBuilt} Pressure";

                Console.WriteLine($"{i + 1}. {choice.Name} ({effectType}: {effect})");
                Console.WriteLine($"   {description}");
                Console.WriteLine();
            }

            // Run a simplified encounter loop with narrative
            NarrativeResult currentResult = initialResult;
            while (!currentResult.IsEncounterOver)
            {
                // Simulate player selection (in a real game, this would come from UI)
                Console.Write("Select a choice (1-4): ");
                if (!int.TryParse(Console.ReadLine(), out int selectedIndex) || selectedIndex < 1 || selectedIndex > currentResult.Choices.Count)
                {
                    Console.WriteLine("Invalid choice, defaulting to 1");
                    selectedIndex = 1;
                }

                IChoice selectedChoice = currentResult.Choices[selectedIndex - 1];
                string selectedDescription = currentResult.ChoiceDescriptions[selectedChoice];

                // Apply the choice with narrative
                currentResult = await encounterManager.ApplyChoiceWithNarrativeAsync(
                    selectedChoice,
                    selectedDescription);

                Console.WriteLine("\n=== Outcome ===");
                Console.WriteLine(currentResult.Narrative);
                Console.WriteLine();

                // Check if encounter is over
                if (currentResult.IsEncounterOver)
                {
                    Console.WriteLine($"\n=== Encounter Over: {currentResult.Outcome} ===");
                    break;
                }

                Console.WriteLine("=== Available Choices ===");
                for (int i = 0; i < currentResult.Choices.Count; i++)
                {
                    IChoice choice = currentResult.Choices[i];
                    string description = currentResult.ChoiceDescriptions[choice];
                    ChoiceProjection projection = currentResult.Projections[i];

                    string effectType = choice.EffectType == EffectTypes.Momentum ? "Momentum" : "Pressure";
                    string effect = choice.EffectType == EffectTypes.Momentum
                        ? $"+{projection.MomentumGained} Momentum"
                        : $"+{projection.PressureBuilt} Pressure";

                    Console.WriteLine($"{i + 1}. {choice.Name} ({effectType}: {effect})");
                    Console.WriteLine($"   {description}");
                    Console.WriteLine();
                }
            }

            // Output full narrative
            Console.WriteLine("\n=== Full Narrative ===");
            Console.WriteLine(encounterManager.GetNarrativeContext().ToPrompt());
        }
    }
}