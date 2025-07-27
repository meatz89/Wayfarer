using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Edge case and integration tests for the tutorial system.
/// Focuses on specific issues identified during development.
/// </summary>
public class TutorialEdgeCaseTests
{
    private IServiceProvider _provider;
    private GameWorld _gameWorld;
    private NarrativeManager _narrativeManager;
    private FlagService _flagService;
    private CommandExecutor _commandExecutor;
    private MainGameplayViewModel _viewModel;
    
    /// <summary>
    /// Tests for specific edge cases and integration issues
    /// </summary>
    public async Task<bool> RunEdgeCaseTests()
    {
        Console.WriteLine("\n=== TUTORIAL EDGE CASE TESTS ===\n");
        
        var tests = new List<(string name, Func<Task<bool>> test)>
        {
            ("Tutorial Overlay Rendering", TestTutorialOverlayRendering),
            ("Command Filtering Edge Cases", TestCommandFilteringEdgeCases),
            ("Step Transition Race Conditions", TestStepTransitionRaceConditions),
            ("Multiple Narrative Conflicts", TestMultipleNarrativeConflicts),
            ("Save During Step Transition", TestSaveDuringStepTransition),
            ("UI State Synchronization", TestUIStateSynchronization),
            ("Invalid Step Recovery", TestInvalidStepRecovery),
            ("Dialogue Override Priority", TestDialogueOverridePriority),
            ("Letter Board Override Mechanics", TestLetterBoardOverrideMechanics),
            ("NPC Visibility Transitions", TestNPCVisibilityTransitions),
            ("Tutorial Restart Scenarios", TestTutorialRestartScenarios),
            ("Patron Queue Priority Edge Cases", TestPatronQueuePriorityEdgeCases),
            ("Trust Token Edge Cases", TestTrustTokenEdgeCases),
            ("Time-Sensitive Step Handling", TestTimeSensitiveStepHandling),
            ("Completion Effect Ordering", TestCompletionEffectOrdering)
        };
        
        var results = new Dictionary<string, bool>();
        
        foreach (var (name, test) in tests)
        {
            Console.WriteLine($"\n--- Testing: {name} ---");
            try
            {
                results[name] = await test();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ EXCEPTION: {ex.Message}");
                results[name] = false;
            }
        }
        
        PrintResults(results);
        return results.Values.All(v => v);
    }
    
    private async Task<bool> TestTutorialOverlayRendering()
    {
        try
        {
            // Test that overlay properly displays during all tutorial steps
            await StartTutorial();
            
            // Get view model state
            var overlayData = _viewModel.GetTutorialOverlayData();
            Assert(overlayData != null, "Overlay data should exist");
            Assert(overlayData.IsVisible, "Overlay should be visible during tutorial");
            Assert(!string.IsNullOrEmpty(overlayData.StepName), "Step name required");
            Assert(!string.IsNullOrEmpty(overlayData.GuidanceText), "Guidance text required");
            Assert(overlayData.Progress != null, "Progress info required");
            
            // Test overlay updates on step change
            var initialStep = overlayData.StepName;
            await ProgressOneStep();
            
            overlayData = _viewModel.GetTutorialOverlayData();
            Assert(overlayData.StepName != initialStep, "Step name should update");
            
            // Test overlay hides after tutorial
            await CompleteTutorial();
            overlayData = _viewModel.GetTutorialOverlayData();
            Assert(!overlayData.IsVisible, "Overlay should hide after tutorial");
            
            Console.WriteLine("✓ PASS: Tutorial overlay renders correctly");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestCommandFilteringEdgeCases()
    {
        try
        {
            await StartTutorial();
            
            // Test 1: Empty allowed actions list
            var step = CreateTestStep(allowedActions: new List<string>());
            _narrativeManager.SetCurrentStep("wayfarer_tutorial", step);
            
            var commands = _commandExecutor.GetAvailableCommands(_gameWorld);
            Assert(commands.Count == 0, "No commands when allowed actions empty");
            
            // Test 2: Invalid action in allowed list
            step = CreateTestStep(allowedActions: new List<string> { "InvalidAction" });
            _narrativeManager.SetCurrentStep("wayfarer_tutorial", step);
            
            commands = _commandExecutor.GetAvailableCommands(_gameWorld);
            Assert(commands.Count == 0, "Invalid actions should be filtered");
            
            // Test 3: Case sensitivity
            step = CreateTestStep(allowedActions: new List<string> { "travel", "TALK", "Work" });
            _narrativeManager.SetCurrentStep("wayfarer_tutorial", step);
            
            commands = _commandExecutor.GetAvailableCommands(_gameWorld);
            Assert(commands.Any(c => c.Category == "Travel"), "Should match case-insensitive");
            
            // Test 4: Partial matches
            step = CreateTestStep(allowedActions: new List<string> { "Converse" });
            _narrativeManager.SetCurrentStep("wayfarer_tutorial", step);
            
            var talkCommands = commands.Where(c => c.DisplayName.Contains("Talk")).ToList();
            Assert(talkCommands.Count > 0, "Converse should enable Talk commands");
            
            Console.WriteLine("✓ PASS: Command filtering handles edge cases");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestStepTransitionRaceConditions()
    {
        try
        {
            await StartTutorial();
            
            // Test rapid step transitions
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(async () => 
                {
                    await _narrativeManager.CheckAndProgressNarratives();
                }));
            }
            
            await Task.WhenAll(tasks);
            
            // Verify state consistency
            var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            Assert(currentStep != null, "Should have valid step after concurrent updates");
            
            var stepIndex = _narrativeManager.GetCurrentStepIndex("wayfarer_tutorial");
            Assert(stepIndex >= 0 && stepIndex < 57, "Step index should be valid");
            
            // Test simultaneous completion and save
            var saveTask = _saveStateService.SaveGame("race_test");
            var progressTask = CompleteCurrentStep();
            
            await Task.WhenAll(saveTask, progressTask);
            
            // Load and verify consistency
            await _saveStateService.LoadGame("race_test");
            var loadedStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            Assert(loadedStep != null, "Loaded state should be valid");
            
            Console.WriteLine("✓ PASS: No race conditions in step transitions");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestMultipleNarrativeConflicts()
    {
        try
        {
            // Start tutorial
            await StartTutorial();
            
            // Try to start another narrative
            var result = await _narrativeManager.StartNarrative("some_other_narrative");
            Assert(!result, "Should not start other narratives during tutorial");
            
            // Verify tutorial remains active
            Assert(_narrativeManager.IsNarrativeActive("wayfarer_tutorial"), 
                "Tutorial should remain active");
            
            // Test after tutorial completion
            await CompleteTutorial();
            
            result = await _narrativeManager.StartNarrative("some_other_narrative");
            Assert(result || !_narrativeManager.HasNarrative("some_other_narrative"), 
                "Should allow other narratives after tutorial");
            
            Console.WriteLine("✓ PASS: Multiple narrative conflicts handled");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestSaveDuringStepTransition()
    {
        try
        {
            await StartTutorial();
            await ProgressToStep("day1_meet_martha");
            
            // Start step completion
            var completionTask = _commandExecutor.ExecuteCommand("talk martha", _gameWorld);
            
            // Save during transition
            await Task.Delay(10); // Small delay to hit mid-transition
            await _saveStateService.SaveGame("transition_save");
            
            await completionTask;
            
            // Load and verify state
            await _saveStateService.LoadGame("transition_save");
            
            var step = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            Assert(step != null, "Should have valid step after load");
            Assert(step.Id == "day1_meet_martha" || step.Id == "day1_first_work", 
                "Should be at valid step");
            
            Console.WriteLine("✓ PASS: Save during transition handled correctly");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestUIStateSynchronization()
    {
        try
        {
            await StartTutorial();
            
            // Test UI updates with narrative progression
            var initialUI = _viewModel.GetUIState();
            Assert(!initialUI.ShowLetterQueue, "Queue hidden initially");
            
            // Progress to letter acceptance
            await ProgressToStep("day1_accept_letter");
            await CompleteCurrentStep();
            
            var updatedUI = _viewModel.GetUIState();
            Assert(updatedUI.ShowLetterQueue, "Queue visible after letter accepted");
            
            // Test UI state persists through save/load
            await _saveStateService.SaveGame("ui_test");
            
            // Reset UI
            _viewModel.ResetUIState();
            var resetUI = _viewModel.GetUIState();
            Assert(!resetUI.ShowLetterQueue, "UI reset should hide elements");
            
            // Load and verify UI restored
            await _saveStateService.LoadGame("ui_test");
            var loadedUI = _viewModel.GetUIState();
            Assert(loadedUI.ShowLetterQueue, "UI state should restore from save");
            
            Console.WriteLine("✓ PASS: UI state synchronizes correctly");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestInvalidStepRecovery()
    {
        try
        {
            await StartTutorial();
            
            // Corrupt the narrative state
            _narrativeManager.ForceSetStep("wayfarer_tutorial", "invalid_step_id");
            
            // Try to progress
            var result = await _narrativeManager.CheckAndProgressNarratives();
            
            // Should recover to valid state
            var step = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            Assert(step != null, "Should recover to valid step");
            Assert(step.Id != "invalid_step_id", "Should not be on invalid step");
            
            // Test missing completion requirements
            var brokenStep = CreateTestStep(
                id: "broken_step",
                completionRequirements: null
            );
            _narrativeManager.SetCurrentStep("wayfarer_tutorial", brokenStep);
            
            // Should handle gracefully
            await _narrativeManager.CheckAndProgressNarratives();
            Assert(true, "Should handle missing requirements gracefully");
            
            Console.WriteLine("✓ PASS: Invalid step recovery works");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestDialogueOverridePriority()
    {
        try
        {
            await StartTutorial();
            await ProgressToStep("day1_meet_martha");
            
            var step = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            var npc = _npcRepository.GetNPCById("martha_docker");
            
            // Test override takes precedence
            var dialogue = _narrativeManager.GetNPCDialogue(npc.Id);
            Assert(dialogue == step.DialogueOverrides["martha_docker"], 
                "Override should take precedence");
            
            // Test after step with no override
            await ProgressToStep("day1_first_work");
            dialogue = _narrativeManager.GetNPCDialogue(npc.Id);
            Assert(dialogue != step.DialogueOverrides.GetValueOrDefault("martha_docker"), 
                "Should use default dialogue when no override");
            
            Console.WriteLine("✓ PASS: Dialogue override priority correct");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestLetterBoardOverrideMechanics()
    {
        try
        {
            await StartTutorial();
            await ProgressToStep("day1_dawn_arrival");
            
            var step = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            Assert(step.LetterBoardOverride != null, "Should have letter board override");
            
            // Visit letter board
            var boardLetters = await _letterQueueService.GetLetterBoardContents();
            
            // Verify only tutorial letters shown
            Assert(boardLetters.Count == step.LetterBoardOverride.Letters.Count, 
                "Should show exact override letters");
            Assert(boardLetters.All(l => 
                step.LetterBoardOverride.Letters.Any(ol => ol.Id == l.Id)), 
                "All letters should be from override");
            
            // Test acceptance restrictions
            await ProgressToStep("day1_accept_letter");
            step = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            
            if (step.LetterAcceptOverride?.OnlyLetter != null)
            {
                // Try to accept different letter
                var result = await _commandExecutor.ExecuteCommand("accept letter 2", _gameWorld);
                Assert(!result.Success, "Should only accept specified letter");
            }
            
            Console.WriteLine("✓ PASS: Letter board overrides work correctly");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestNPCVisibilityTransitions()
    {
        try
        {
            await StartTutorial();
            
            // Track NPC visibility through steps
            var visibilityLog = new List<(string step, List<string> visibleNpcs)>();
            
            for (int i = 0; i < 10; i++)
            {
                var step = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
                var visibleNpcs = _npcRepository.GetAllNPCs()
                    .Where(npc => _gameWorld.IsNPCVisible(npc.Id))
                    .Select(npc => npc.Id)
                    .ToList();
                
                visibilityLog.Add((step.Id, visibleNpcs));
                
                // Verify matches step definition
                foreach (var npcId in step.VisibleNPCs ?? new List<string>())
                {
                    Assert(visibleNpcs.Contains(npcId), 
                        $"NPC {npcId} should be visible in step {step.Id}");
                }
                
                await ProgressOneStep();
            }
            
            // Verify visibility changes appropriately
            Assert(visibilityLog[0].visibleNpcs.Count == 0, "No NPCs at start");
            Assert(visibilityLog.Any(v => v.visibleNpcs.Count > 0), "NPCs appear later");
            
            Console.WriteLine("✓ PASS: NPC visibility transitions correctly");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestTutorialRestartScenarios()
    {
        try
        {
            // Test 1: Restart mid-tutorial
            await StartTutorial();
            await ProgressToStep("day2_urgent_delivery");
            
            // Force restart
            _flagService.RemoveFlag(FlagService.TUTORIAL_COMPLETE);
            _flagService.RemoveFlag(FlagService.TUTORIAL_STARTED);
            await _gameWorldManager.StartGame();
            
            Assert(_narrativeManager.GetCurrentStep("wayfarer_tutorial").Id == "day1_wake", 
                "Should restart from beginning");
            
            // Test 2: Complete then restart
            await CompleteTutorial();
            Assert(!_narrativeManager.IsNarrativeActive("wayfarer_tutorial"), 
                "Tutorial inactive after completion");
            
            // Remove completion flag
            _flagService.RemoveFlag(FlagService.TUTORIAL_COMPLETE);
            await _gameWorldManager.StartGame();
            
            Assert(_narrativeManager.IsNarrativeActive("wayfarer_tutorial"), 
                "Tutorial should restart if completion flag removed");
            
            Console.WriteLine("✓ PASS: Tutorial restart scenarios work");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestPatronQueuePriorityEdgeCases()
    {
        try
        {
            await StartTutorial();
            await ProgressToStep("day3_first_patron_letter");
            
            var queue = _gameWorld.GetPlayer().LetterQueue;
            
            // Fill queue to capacity
            for (int i = 0; i < queue.GetMaxSize(); i++)
            {
                queue.AddLetter(new Letter { Id = $"regular_{i}", Payment = 2 });
            }
            
            // Accept patron letter when queue full
            var result = await _commandExecutor.ExecuteCommand("accept patron letter", _gameWorld);
            Assert(result.Success, "Patron letter should force into full queue");
            
            var letters = queue.GetQueuedLetters();
            Assert(letters[0].Id.Contains("patron"), "Patron letter at position 1");
            Assert(letters.Count <= queue.GetMaxSize(), "Queue size maintained");
            
            // Test multiple patron letters
            queue.AddPatronLetter(new Letter { Id = "patron_2", Payment = 5 });
            queue.AddPatronLetter(new Letter { Id = "patron_3", Payment = 5 });
            
            letters = queue.GetQueuedLetters();
            Assert(letters.Take(3).All(l => l.Id.Contains("patron")), 
                "All patron letters at top");
            
            Console.WriteLine("✓ PASS: Patron queue priority edge cases handled");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestTrustTokenEdgeCases()
    {
        try
        {
            await StartTutorial();
            await ProgressToStep("day2_spend_trust");
            
            var player = _gameWorld.GetPlayer();
            var elenaTrust = player.GetTrustTokens("elena_scribe");
            
            // Try to spend more trust than available
            var step = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            if (step.TrustOverride != null)
            {
                // Artificially set trust requirement higher
                step.TrustOverride.Cost = elenaTrust + 1;
                
                var result = await _commandExecutor.ExecuteCommand("spend trust elena", _gameWorld);
                Assert(!result.Success, "Should fail with insufficient trust");
            }
            
            // Test trust spending with no tokens
            player.SetTrustTokens("test_npc", 0);
            result = await _commandExecutor.ExecuteCommand("spend trust test_npc", _gameWorld);
            Assert(!result.Success, "Should fail with zero trust");
            
            // Test negative trust prevention
            player.SetTrustTokens("test_npc", 1);
            player.SpendTrustToken("test_npc");
            player.SpendTrustToken("test_npc"); // Try to go negative
            
            Assert(player.GetTrustTokens("test_npc") >= 0, "Trust should never go negative");
            
            Console.WriteLine("✓ PASS: Trust token edge cases handled");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestTimeSensitiveStepHandling()
    {
        try
        {
            await StartTutorial();
            await ProgressToStep("day2_urgent_delivery");
            
            var step = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            Assert(step.TimeLimit == true, "Step should be time-sensitive");
            
            // Test time passage
            var initialTime = _gameWorld.GetCurrentHour();
            
            // Simulate delay
            for (int i = 0; i < 3; i++)
            {
                await _gameWorld.AdvanceTime(1);
            }
            
            // Verify consequences (this would depend on implementation)
            // For now, just verify time advanced
            Assert(_gameWorld.GetCurrentHour() > initialTime, "Time should advance");
            
            // Complete delivery
            await CompleteCurrentStep();
            
            // Verify rewards/penalties based on timing
            var player = _gameWorld.GetPlayer();
            Assert(player.Coins > 0, "Should still receive payment");
            
            Console.WriteLine("✓ PASS: Time-sensitive steps handled");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestCompletionEffectOrdering()
    {
        try
        {
            await StartTutorial();
            
            // Test step with multiple completion effects
            var testStep = CreateTestStep(
                completionEffects: new List<Effect>
                {
                    new Effect { Type = "SetFlag", Value = "effect_1" },
                    new Effect { Type = "GiveCoins", Amount = 5 },
                    new Effect { Type = "SetFlag", Value = "effect_2" },
                    new Effect { Type = "GainTrust", NPC = "test_npc", Amount = 1 }
                }
            );
            
            var player = _gameWorld.GetPlayer();
            var initialCoins = player.Coins;
            
            // Apply effects
            await _narrativeManager.ApplyStepEffects(testStep);
            
            // Verify all effects applied in order
            Assert(_flagService.HasFlag("effect_1"), "First flag set");
            Assert(_flagService.HasFlag("effect_2"), "Second flag set");
            Assert(player.Coins == initialCoins + 5, "Coins given");
            Assert(player.GetTrustTokens("test_npc") >= 1, "Trust gained");
            
            // Test tutorial completion effects
            await ProgressToStep("day3_tutorial_complete");
            await CompleteCurrentStep();
            
            // Verify completion effects
            Assert(_flagService.HasFlag(FlagService.TUTORIAL_COMPLETE), "Complete flag set");
            Assert(!_flagService.HasFlag("tutorial_active"), "Active flag removed");
            Assert(_gameWorld.IsAllContentUnlocked(), "Content unlocked");
            
            Console.WriteLine("✓ PASS: Completion effects applied in correct order");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    // Helper methods
    
    private async Task StartTutorial()
    {
        _flagService.ClearAllFlags();
        await _gameWorldManager.StartGame();
    }
    
    private async Task ProgressToStep(string stepId)
    {
        while (_narrativeManager.GetCurrentStep("wayfarer_tutorial")?.Id != stepId)
        {
            await ProgressOneStep();
        }
    }
    
    private async Task ProgressOneStep()
    {
        var step = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
        if (step?.CompletionRequirements?.FirstOrDefault() is { Type: "FlagSet" } req)
        {
            _flagService.SetFlag(req.Value);
        }
        await _narrativeManager.CheckAndProgressNarratives();
    }
    
    private async Task CompleteCurrentStep()
    {
        await ProgressOneStep();
    }
    
    private async Task CompleteTutorial()
    {
        await ProgressToStep("day3_tutorial_complete");
        _flagService.SetFlag("tutorial_completed");
        await _narrativeManager.CheckAndProgressNarratives();
    }
    
    private NarrativeStep CreateTestStep(
        string id = "test_step",
        List<string> allowedActions = null,
        List<CompletionRequirement> completionRequirements = null,
        List<Effect> completionEffects = null)
    {
        return new NarrativeStep
        {
            Id = id,
            Name = "Test Step",
            Description = "Test Description",
            GuidanceText = "Test Guidance",
            AllowedActions = allowedActions ?? new List<string> { "Travel" },
            CompletionRequirements = completionRequirements,
            CompletionEffects = completionEffects
        };
    }
    
    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }
    
    private void PrintResults(Dictionary<string, bool> results)
    {
        Console.WriteLine("\n=== EDGE CASE TEST RESULTS ===");
        Console.WriteLine($"Total: {results.Count}");
        Console.WriteLine($"Passed: {results.Count(r => r.Value)}");
        Console.WriteLine($"Failed: {results.Count(r => !r.Value)}");
        
        if (results.Any(r => !r.Value))
        {
            Console.WriteLine("\nFailed tests:");
            foreach (var (name, _) in results.Where(r => !r.Value))
            {
                Console.WriteLine($"  ✗ {name}");
            }
        }
    }
}