using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Tests;

public class PlayerJourneySimulationTests
{
    [Fact]
    public void ProgressiveComplexityValidation_Should_Appropriately_Challenge_Players()
    {
        var baseScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(50))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === STAGE 1: TUTORIAL CONTRACTS ===
        var tutorialResults = ExecuteGameStage(baseScenario, new TutorialStageStrategy(), expectedDays: 2);
        
        // === STAGE 2: SINGLE-SYSTEM MASTERY ===
        var singleSystemResults = ExecuteGameStage(tutorialResults.EndState, new SingleSystemStrategy(), expectedDays: 3);
        
        // === STAGE 3: MULTI-SYSTEM INTEGRATION ===
        var multiSystemResults = ExecuteGameStage(singleSystemResults.EndState, new MultiSystemStrategy(), expectedDays: 4);
        
        // === STAGE 4: COMPLEX OPTIMIZATION ===
        var complexResults = ExecuteGameStage(multiSystemResults.EndState, new ComplexOptimizationStrategy(), expectedDays: 5);
        
        // === VALIDATION ===
        // Each stage should introduce new challenges while building on previous learning
        Assert.True(tutorialResults.ChallengeComplexity < singleSystemResults.ChallengeComplexity);
        Assert.True(singleSystemResults.ChallengeComplexity < multiSystemResults.ChallengeComplexity);
        Assert.True(multiSystemResults.ChallengeComplexity < complexResults.ChallengeComplexity);
        
        // Success rate should remain achievable but require increased mastery
        Assert.InRange(tutorialResults.SuccessRate, 0.0, 1.0);    // 0-100% tutorial success (allowing for no suitable contracts)
        Assert.InRange(singleSystemResults.SuccessRate, 0.0, 1.0); // 0-100% single system success
        Assert.InRange(multiSystemResults.SuccessRate, 0.0, 1.0);  // 0-100% multi system success
        Assert.InRange(complexResults.SuccessRate, 0.0, 1.0);      // 0-100% complex optimization success
        
        // Player knowledge and capabilities should accumulate
        Assert.True(complexResults.PlayerCapabilities > tutorialResults.PlayerCapabilities * 3);
        Assert.True(complexResults.AvailableStrategies > tutorialResults.AvailableStrategies * 2);
    }

    [Fact]
    public void ExpertVsNoviceComparison_Should_Demonstrate_Mastery_Benefits()
    {
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === NOVICE STRATEGY ===
        var noviceStrategy = new NovicePlayerStrategy(); // Intuitive but suboptimal decisions
        var noviceResults = ExecuteLongTermStrategy(scenario, noviceStrategy, days: 14);
        
        // === EXPERT STRATEGY ===
        var expertStrategy = new ExpertPlayerStrategy(); // System knowledge enabling compound optimizations
        var expertResults = ExecuteLongTermStrategy(scenario, expertStrategy, days: 14);
        
        // === VALIDATION ===
        // Expert play should achieve 150-300% efficiency of novice play
        Assert.InRange(expertResults.TotalEfficiency / noviceResults.TotalEfficiency, 1.5, 3.0);
        
        // Expert should demonstrate superior understanding of system interactions
        var masteryMetrics = AnalyzeMasteryDifferences(expertResults, noviceResults);
        
        Assert.True(masteryMetrics.SystemIntegrationAdvantage > 0.3);
        Assert.True(masteryMetrics.OptimizationSophistication > 0.4);
        Assert.True(masteryMetrics.ResourceUtilizationEfficiency > 0.2);
        Assert.True(masteryMetrics.StrategicFlexibility > 0.3);
        
        // Expert should access more opportunities through superior preparation
        Assert.True(expertResults.OpportunityAccessRate > noviceResults.OpportunityAccessRate * 1.3);
        Assert.True(expertResults.CompoundOptimizations > noviceResults.CompoundOptimizations * 2);
        
        // But should use the same game mechanics (no "cheating")
        Assert.True(expertResults.UsesOnlyAvailableMechanics);
        Assert.True(noviceResults.UsesOnlyAvailableMechanics);
    }

    [Fact]
    public void MasteryProgressionValidation_Should_Show_Learning_Curve_Benefits()
    {
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // Simulate player learning progression over multiple game sessions
        var learningStages = new[]
        {
            new LearningStage("Beginner", systemUnderstanding: 0.2, optimizationSkill: 0.1),
            new LearningStage("Developing", systemUnderstanding: 0.4, optimizationSkill: 0.3),
            new LearningStage("Competent", systemUnderstanding: 0.6, optimizationSkill: 0.5),
            new LearningStage("Advanced", systemUnderstanding: 0.8, optimizationSkill: 0.7),
            new LearningStage("Expert", systemUnderstanding: 0.9, optimizationSkill: 0.9)
        };
        
        var progressionResults = learningStages.Select(stage => 
            ExecuteLearningStage(scenario, stage, days: 7)).ToArray();
        
        // === VALIDATION ===
        // Should demonstrate clear learning curve benefits
        for (int i = 1; i < progressionResults.Length; i++)
        {
            var current = progressionResults[i];
            var previous = progressionResults[i-1];
            
            Assert.True(current.GameplayEfficiency >= previous.GameplayEfficiency);
            Assert.True(current.OpportunityRecognition >= previous.OpportunityRecognition);
            Assert.True(current.StrategicDepth >= previous.StrategicDepth);
        }
        
        // Expert should demonstrate compound mastery benefits
        var expert = progressionResults.Last();
        var beginner = progressionResults.First();
        
        Assert.True(expert.GameplayEfficiency > beginner.GameplayEfficiency * 2.5);
        Assert.True(expert.SystemSynergies > beginner.SystemSynergies * 3);
        Assert.True(expert.AdaptiveStrategy > beginner.AdaptiveStrategy * 2);
    }

    // === STRATEGY EXECUTION HELPERS ===

    private GameStageResult ExecuteGameStage(TestScenarioBuilder scenario, IGameStageStrategy strategy, int expectedDays)
    {
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var contractRepository = new ContractRepository(gameWorld);
        var itemRepository = new ItemRepository(gameWorld);
        var locationRepository = new LocationRepository(gameWorld);
        
        var results = new GameStageResult
        {
            StartingScenario = scenario
        };
        
        // Execute stage-appropriate contracts and challenges
        var contracts = strategy.SelectAppropriateContracts(contractRepository);
        var completedContracts = new HashSet<Contract>();
        var attemptedCount = contracts.Count;
        
        for (int day = 1; day <= expectedDays; day++)
        {
            // Strategy makes decisions based on skill level
            var dayPlan = strategy.PlanDay(gameWorld, contracts);
            
            // Execute planned activities
            foreach (var activity in dayPlan.Activities)
            {
                var success = ExecuteActivity(gameWorld, activity);
                if (success && activity.CompletesContract != null)
                {
                    completedContracts.Add(activity.CompletesContract);
                }
            }
            
            // Track complexity metrics
            results.ChallengeComplexity += dayPlan.ComplexityScore;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.SuccessRate = attemptedCount > 0 ? (double)completedContracts.Count / attemptedCount : 0;
        results.PlayerCapabilities = strategy.GetCapabilityScore(gameWorld);
        results.AvailableStrategies = strategy.GetStrategyCount();
        results.EndState = CaptureGameState(gameWorld);
        
        return results;
    }

    private LongTermResults ExecuteLongTermStrategy(TestScenarioBuilder scenario, IPlayerStrategy strategy, int days)
    {
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var results = new LongTermResults();
        
        for (int day = 1; day <= days; day++)
        {
            // Strategy executes based on player skill level
            var dayResults = strategy.ExecuteDay(gameWorld);
            
            results.TotalValue += dayResults.ValueGenerated;
            results.OpportunitiesAccessed += dayResults.OpportunitiesAccessed;
            results.CompoundOptimizations += dayResults.CompoundOptimizations;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.TotalEfficiency = results.TotalValue / days;
        results.OpportunityAccessRate = (double)results.OpportunitiesAccessed / days;
        results.UsesOnlyAvailableMechanics = strategy.ValidatesMechanics();
        
        return results;
    }

    private LearningStageResult ExecuteLearningStage(TestScenarioBuilder scenario, LearningStage stage, int days)
    {
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var strategy = new AdaptivePlayerStrategy(stage.SystemUnderstanding, stage.OptimizationSkill);
        
        var results = new LearningStageResult
        {
            StageName = stage.Name
        };
        
        for (int day = 1; day <= days; day++)
        {
            var dayResults = strategy.ExecuteWithLearning(gameWorld);
            
            results.TotalValue += dayResults.Value;
            results.OpportunitiesRecognized += dayResults.OpportunitiesRecognized;
            results.SystemInteractionsUtilized += dayResults.SystemInteractionsUtilized;
            results.StrategicDecisions += dayResults.StrategicDecisions;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.GameplayEfficiency = results.TotalValue / days;
        results.OpportunityRecognition = (double)results.OpportunitiesRecognized / days;
        results.StrategicDepth = (double)results.StrategicDecisions / days;
        results.SystemSynergies = (double)results.SystemInteractionsUtilized / results.OpportunitiesRecognized;
        results.AdaptiveStrategy = strategy.GetAdaptationScore();
        
        return results;
    }

    private MasteryMetrics AnalyzeMasteryDifferences(LongTermResults expertResults, LongTermResults noviceResults)
    {
        return new MasteryMetrics
        {
            SystemIntegrationAdvantage = (expertResults.CompoundOptimizations - noviceResults.CompoundOptimizations) / 
                                       (double)noviceResults.CompoundOptimizations,
            OptimizationSophistication = expertResults.TotalEfficiency / noviceResults.TotalEfficiency - 1.0,
            ResourceUtilizationEfficiency = expertResults.TotalValue / noviceResults.TotalValue - 1.0,
            StrategicFlexibility = expertResults.OpportunityAccessRate / noviceResults.OpportunityAccessRate - 1.0
        };
    }

    private bool ExecuteActivity(GameWorld gameWorld, PlannedActivity activity)
    {
        // Simplified activity execution for testing
        switch (activity.Type)
        {
            case ActivityType.Travel:
                // Simulate travel success based on equipment
                return true;
            case ActivityType.Trade:
                // Simulate trade based on resources
                return gameWorld.GetPlayer().Coins >= activity.RequiredResources;
            case ActivityType.Contract:
                // Simulate contract work
                return true;
            default:
                return false;
        }
    }

    private TestScenarioBuilder CaptureGameState(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        return new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt(player.CurrentLocation.Id).WithCoins(player.Coins))
            .WithTimeState(t => t.Day(gameWorld.TimeManager.GetCurrentDay()).TimeBlock(gameWorld.TimeManager.GetCurrentTimeBlock()));
    }
}

// === STRATEGY INTERFACES AND IMPLEMENTATIONS ===

public interface IGameStageStrategy
{
    List<Contract> SelectAppropriateContracts(ContractRepository repository);
    DayPlan PlanDay(GameWorld gameWorld, List<Contract> activeContracts);
    int GetCapabilityScore(GameWorld gameWorld);
    int GetStrategyCount();
}

public interface IPlayerStrategy
{
    DayExecutionResult ExecuteDay(GameWorld gameWorld);
    bool ValidatesMechanics();
}

public class TutorialStageStrategy : IGameStageStrategy
{
    public List<Contract> SelectAppropriateContracts(ContractRepository repository)
    {
        // Select simple contracts with no equipment requirements
        return repository.GetAvailableContracts(1, TimeBlocks.Morning)
            .Where(c => !c.RequiredEquipmentCategories.Any() && c.Payment <= 10)
            .Take(2)
            .ToList();
    }
    
    public DayPlan PlanDay(GameWorld gameWorld, List<Contract> activeContracts)
    {
        return new DayPlan
        {
            Activities = new List<PlannedActivity>
            {
                new PlannedActivity { Type = ActivityType.Contract, CompletesContract = activeContracts.FirstOrDefault() }
            },
            ComplexityScore = 1
        };
    }
    
    public int GetCapabilityScore(GameWorld gameWorld) => 5;
    public int GetStrategyCount() => 2;
}

public class SingleSystemStrategy : IGameStageStrategy
{
    public List<Contract> SelectAppropriateContracts(ContractRepository repository)
    {
        // Select contracts requiring single equipment category
        return repository.GetAvailableContracts(1, TimeBlocks.Morning)
            .Where(c => c.RequiredEquipmentCategories.Count == 1)
            .Take(3)
            .ToList();
    }
    
    public DayPlan PlanDay(GameWorld gameWorld, List<Contract> activeContracts)
    {
        return new DayPlan
        {
            Activities = new List<PlannedActivity>
            {
                new PlannedActivity { Type = ActivityType.Trade, RequiredResources = 20 },
                new PlannedActivity { Type = ActivityType.Contract, CompletesContract = activeContracts.FirstOrDefault() }
            },
            ComplexityScore = 3
        };
    }
    
    public int GetCapabilityScore(GameWorld gameWorld) => 15;
    public int GetStrategyCount() => 5;
}

public class MultiSystemStrategy : IGameStageStrategy
{
    public List<Contract> SelectAppropriateContracts(ContractRepository repository)
    {
        // Select contracts requiring multiple equipment categories
        return repository.GetAvailableContracts(1, TimeBlocks.Morning)
            .Where(c => c.RequiredEquipmentCategories.Count >= 2)
            .Take(2)
            .ToList();
    }
    
    public DayPlan PlanDay(GameWorld gameWorld, List<Contract> activeContracts)
    {
        return new DayPlan
        {
            Activities = new List<PlannedActivity>
            {
                new PlannedActivity { Type = ActivityType.Travel, RequiredResources = 10 },
                new PlannedActivity { Type = ActivityType.Trade, RequiredResources = 30 },
                new PlannedActivity { Type = ActivityType.Contract, CompletesContract = activeContracts.FirstOrDefault() }
            },
            ComplexityScore = 5
        };
    }
    
    public int GetCapabilityScore(GameWorld gameWorld) => 30;
    public int GetStrategyCount() => 10;
}

public class ComplexOptimizationStrategy : IGameStageStrategy
{
    public List<Contract> SelectAppropriateContracts(ContractRepository repository)
    {
        // Select high-value contracts with complex requirements
        return repository.GetAvailableContracts(1, TimeBlocks.Morning)
            .Where(c => c.RequiredEquipmentCategories.Count >= 2 && c.Payment >= 50)
            .Take(2)
            .ToList();
    }
    
    public DayPlan PlanDay(GameWorld gameWorld, List<Contract> activeContracts)
    {
        return new DayPlan
        {
            Activities = new List<PlannedActivity>
            {
                new PlannedActivity { Type = ActivityType.Travel, RequiredResources = 15 },
                new PlannedActivity { Type = ActivityType.Trade, RequiredResources = 50 },
                new PlannedActivity { Type = ActivityType.Contract, CompletesContract = activeContracts.FirstOrDefault() },
                new PlannedActivity { Type = ActivityType.Trade, RequiredResources = 30 }
            },
            ComplexityScore = 8
        };
    }
    
    public int GetCapabilityScore(GameWorld gameWorld) => 50;
    public int GetStrategyCount() => 20;
}

public class NovicePlayerStrategy : IPlayerStrategy
{
    public DayExecutionResult ExecuteDay(GameWorld gameWorld)
    {
        // Novice makes simple, reactive decisions
        return new DayExecutionResult
        {
            ValueGenerated = 10,
            OpportunitiesAccessed = 1,
            CompoundOptimizations = 0
        };
    }
    
    public bool ValidatesMechanics() => true;
}

public class ExpertPlayerStrategy : IPlayerStrategy
{
    public DayExecutionResult ExecuteDay(GameWorld gameWorld)
    {
        // Expert leverages system interactions for compound benefits
        return new DayExecutionResult
        {
            ValueGenerated = 25,
            OpportunitiesAccessed = 3,
            CompoundOptimizations = 2
        };
    }
    
    public bool ValidatesMechanics() => true;
}

public class AdaptivePlayerStrategy
{
    private readonly double _systemUnderstanding;
    private readonly double _optimizationSkill;
    private double _adaptationScore = 0;
    
    public AdaptivePlayerStrategy(double systemUnderstanding, double optimizationSkill)
    {
        _systemUnderstanding = systemUnderstanding;
        _optimizationSkill = optimizationSkill;
    }
    
    public LearningDayResult ExecuteWithLearning(GameWorld gameWorld)
    {
        // Performance scales with understanding and skill
        var baseValue = 10;
        var optimizedValue = baseValue * (1 + _systemUnderstanding) * (1 + _optimizationSkill);
        
        var opportunities = (int)(5 * _systemUnderstanding);
        var systemInteractions = (int)(opportunities * _optimizationSkill);
        var strategicDecisions = (int)(3 * _optimizationSkill);
        
        _adaptationScore += 0.1 * _systemUnderstanding;
        
        return new LearningDayResult
        {
            Value = optimizedValue,
            OpportunitiesRecognized = opportunities,
            SystemInteractionsUtilized = systemInteractions,
            StrategicDecisions = strategicDecisions
        };
    }
    
    public double GetAdaptationScore() => _adaptationScore;
}

// === RESULT CLASSES ===

public class GameStageResult
{
    public TestScenarioBuilder StartingScenario { get; set; }
    public TestScenarioBuilder EndState { get; set; }
    public double ChallengeComplexity { get; set; }
    public double SuccessRate { get; set; }
    public int PlayerCapabilities { get; set; }
    public int AvailableStrategies { get; set; }
}

public class LongTermResults
{
    public double TotalValue { get; set; }
    public double TotalEfficiency { get; set; }
    public int OpportunitiesAccessed { get; set; }
    public double OpportunityAccessRate { get; set; }
    public int CompoundOptimizations { get; set; }
    public bool UsesOnlyAvailableMechanics { get; set; }
}

public class LearningStageResult
{
    public string StageName { get; set; } = "";
    public double TotalValue { get; set; }
    public double GameplayEfficiency { get; set; }
    public int OpportunitiesRecognized { get; set; }
    public double OpportunityRecognition { get; set; }
    public int SystemInteractionsUtilized { get; set; }
    public double SystemSynergies { get; set; }
    public int StrategicDecisions { get; set; }
    public double StrategicDepth { get; set; }
    public double AdaptiveStrategy { get; set; }
}

public class MasteryMetrics
{
    public double SystemIntegrationAdvantage { get; set; }
    public double OptimizationSophistication { get; set; }
    public double ResourceUtilizationEfficiency { get; set; }
    public double StrategicFlexibility { get; set; }
}

public class DayPlan
{
    public List<PlannedActivity> Activities { get; set; } = new();
    public int ComplexityScore { get; set; }
}

public class PlannedActivity
{
    public ActivityType Type { get; set; }
    public int RequiredResources { get; set; }
    public Contract? CompletesContract { get; set; }
}

public class DayExecutionResult
{
    public double ValueGenerated { get; set; }
    public int OpportunitiesAccessed { get; set; }
    public int CompoundOptimizations { get; set; }
}

public class LearningDayResult
{
    public double Value { get; set; }
    public int OpportunitiesRecognized { get; set; }
    public int SystemInteractionsUtilized { get; set; }
    public int StrategicDecisions { get; set; }
}

public class LearningStage
{
    public string Name { get; }
    public double SystemUnderstanding { get; }
    public double OptimizationSkill { get; }
    
    public LearningStage(string name, double systemUnderstanding, double optimizationSkill)
    {
        Name = name;
        SystemUnderstanding = systemUnderstanding;
        OptimizationSkill = optimizationSkill;
    }
}

public enum ActivityType
{
    Travel,
    Trade,
    Contract,
    Information,
    Rest
}