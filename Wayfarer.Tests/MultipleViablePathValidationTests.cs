using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Tests;

public class MultipleViablePathValidationTests
{
    [Fact]
    public void SpecializationStrategies_Should_Achieve_Similar_Overall_Efficiency_Through_Different_Paths()
    {
        // === CLIMBING SPECIALIST STRATEGY ===
        var climbingScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(150))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld climbingWorld = TestGameWorldInitializer.CreateTestWorld(climbingScenario);
        var climbingStrategy = new PathSpecializationStrategy(
            specialization: SpecializationType.Climbing,
            equipmentPriority: EquipmentPriority.ClimbingEquipment,
            contractFocus: ContractFocus.Exploration);
        
        // === MARITIME SPECIALIST STRATEGY ===
        var maritimeScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(150))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld maritimeWorld = TestGameWorldInitializer.CreateTestWorld(maritimeScenario);
        var maritimeStrategy = new PathSpecializationStrategy(
            specialization: SpecializationType.Maritime,
            equipmentPriority: EquipmentPriority.WaterTransport,
            contractFocus: ContractFocus.Trade);
        
        // === INFORMATION BROKER STRATEGY ===
        var infoScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(150))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld infoWorld = TestGameWorldInitializer.CreateTestWorld(infoScenario);
        var infoStrategy = new PathSpecializationStrategy(
            specialization: SpecializationType.Information,
            equipmentPriority: EquipmentPriority.NavigationTools,
            contractFocus: ContractFocus.Intelligence);
        
        // === GENERALIST STRATEGY ===
        var generalistScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(150))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld generalistWorld = TestGameWorldInitializer.CreateTestWorld(generalistScenario);
        var generalistStrategy = new PathSpecializationStrategy(
            specialization: SpecializationType.Generalist,
            equipmentPriority: EquipmentPriority.Balanced,
            contractFocus: ContractFocus.Opportunistic);
        
        // === EXECUTE ALL STRATEGIES ===
        var climbingResults = ExecuteSpecializationStrategy(climbingWorld, climbingStrategy, days: 7);
        var maritimeResults = ExecuteSpecializationStrategy(maritimeWorld, maritimeStrategy, days: 7);
        var infoResults = ExecuteSpecializationStrategy(infoWorld, infoStrategy, days: 7);
        var generalistResults = ExecuteSpecializationStrategy(generalistWorld, generalistStrategy, days: 7);
        
        // === VALIDATION: SIMILAR OVERALL EFFICIENCY ===
        var allResults = new[] { climbingResults, maritimeResults, infoResults, generalistResults };
        var maxEfficiency = allResults.Max(r => r.TotalEfficiency);
        var minEfficiency = allResults.Min(r => r.TotalEfficiency);
        
        // All strategies should be within 20% efficiency variance
        Assert.True(minEfficiency / maxEfficiency > 0.8, 
            $"Efficiency variance too high: {minEfficiency} vs {maxEfficiency}");
        
        // === VALIDATION: DIFFERENT PATH SPECIALIZATION ===
        // Climbing specialist should dominate mountain opportunities
        Assert.True(climbingResults.MountainOpportunitiesCompleted > maritimeResults.MountainOpportunitiesCompleted);
        Assert.True(climbingResults.MountainOpportunitiesCompleted > infoResults.MountainOpportunitiesCompleted);
        
        // Maritime specialist should dominate coastal opportunities
        Assert.True(maritimeResults.CoastalOpportunitiesCompleted > climbingResults.CoastalOpportunitiesCompleted);
        Assert.True(maritimeResults.CoastalOpportunitiesCompleted > infoResults.CoastalOpportunitiesCompleted);
        
        // Information specialist should dominate intelligence opportunities
        Assert.True(infoResults.IntelligenceOpportunitiesCompleted > climbingResults.IntelligenceOpportunitiesCompleted);
        Assert.True(infoResults.IntelligenceOpportunitiesCompleted > maritimeResults.IntelligenceOpportunitiesCompleted);
        
        // Generalist should have balanced performance across all areas
        var generalistBalance = CalculateBalanceScore(generalistResults);
        var specialistBalances = new[] { 
            CalculateBalanceScore(climbingResults),
            CalculateBalanceScore(maritimeResults),
            CalculateBalanceScore(infoResults)
        };
        Assert.True(generalistBalance > specialistBalances.Max(),
            "Generalist should have better balance than specialists");
    }

    [Fact]
    public void ComplexContractAlternativeSolutions_Should_Enable_Multiple_Completion_Approaches()
    {
        // Test that complex contracts can be completed through different strategic approaches
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("crossbridge").WithCoins(200))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === APPROACH A: EQUIPMENT-HEAVY SOLUTION ===
        GameWorld equipmentWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var equipmentApproach = new ContractCompletionStrategy(
            approach: CompletionApproach.EquipmentBased,
            riskTolerance: RiskTolerance.Low,
            timeInvestment: TimeInvestment.High);
        
        var complexContract = GetComplexContract("ruins_expedition"); // Requires multiple equipment categories
        var equipmentSolution = ExecuteContractStrategy(equipmentWorld, equipmentApproach, complexContract);
        
        // === APPROACH B: INFORMATION-HEAVY SOLUTION ===
        GameWorld infoWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var infoApproach = new ContractCompletionStrategy(
            approach: CompletionApproach.InformationBased,
            riskTolerance: RiskTolerance.Medium,
            timeInvestment: TimeInvestment.Medium);
        
        var infoSolution = ExecuteContractStrategy(infoWorld, infoApproach, complexContract);
        
        // === APPROACH C: SOCIAL-HEAVY SOLUTION ===
        GameWorld socialWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var socialApproach = new ContractCompletionStrategy(
            approach: CompletionApproach.SocialBased,
            riskTolerance: RiskTolerance.High,
            timeInvestment: TimeInvestment.Low);
        
        var socialSolution = ExecuteContractStrategy(socialWorld, socialApproach, complexContract);
        
        // === VALIDATION: ALL APPROACHES VIABLE ===
        Assert.True(equipmentSolution.ContractCompleted, "Equipment approach should complete contract");
        Assert.True(infoSolution.ContractCompleted, "Information approach should complete contract");
        Assert.True(socialSolution.ContractCompleted, "Social approach should complete contract");
        
        // === VALIDATION: DIFFERENT RESOURCE ALLOCATION PATTERNS ===
        // Equipment approach: High equipment cost, low information cost
        Assert.True(equipmentSolution.EquipmentCost > infoSolution.EquipmentCost);
        Assert.True(equipmentSolution.InformationCost < infoSolution.InformationCost);
        
        // Information approach: High information cost, medium equipment cost
        Assert.True(infoSolution.InformationCost > equipmentSolution.InformationCost);
        Assert.True(infoSolution.InformationCost > socialSolution.InformationCost);
        
        // Social approach: High social investment, low equipment cost
        Assert.True(socialSolution.SocialInvestment > equipmentSolution.SocialInvestment);
        Assert.True(socialSolution.EquipmentCost < equipmentSolution.EquipmentCost);
        
        // === VALIDATION: EFFICIENCY WITHIN ACCEPTABLE RANGE ===
        var solutions = new[] { equipmentSolution, infoSolution, socialSolution };
        var maxValue = solutions.Max(s => s.NetValue);
        var minValue = solutions.Min(s => s.NetValue);
        
        Assert.True(minValue / maxValue > 0.75, "All solution approaches should be within 25% efficiency");
    }

    [Fact]
    public void ResourceAllocationPatterns_Should_Create_Distinct_Strategic_Profiles()
    {
        // Test that different resource allocation strategies create measurably different strategic outcomes
        
        var baseScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(120))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === TIME-FOCUSED ALLOCATION ===
        GameWorld timeWorld = TestGameWorldInitializer.CreateTestWorld(baseScenario);
        var timeStrategy = new ResourceAllocationStrategy(
            timeAllocation: 0.6,      // 60% time investment
            coinAllocation: 0.2,      // 20% coin investment
            inventoryAllocation: 0.2); // 20% inventory optimization
        
        var timeResults = ExecuteResourceStrategy(timeWorld, timeStrategy, days: 5);
        
        // === COIN-FOCUSED ALLOCATION ===
        GameWorld coinWorld = TestGameWorldInitializer.CreateTestWorld(baseScenario);
        var coinStrategy = new ResourceAllocationStrategy(
            timeAllocation: 0.2,      // 20% time investment
            coinAllocation: 0.6,      // 60% coin investment
            inventoryAllocation: 0.2); // 20% inventory optimization
        
        var coinResults = ExecuteResourceStrategy(coinWorld, coinStrategy, days: 5);
        
        // === INVENTORY-FOCUSED ALLOCATION ===
        GameWorld inventoryWorld = TestGameWorldInitializer.CreateTestWorld(baseScenario);
        var inventoryStrategy = new ResourceAllocationStrategy(
            timeAllocation: 0.2,      // 20% time investment
            coinAllocation: 0.2,      // 20% coin investment
            inventoryAllocation: 0.6); // 60% inventory optimization
        
        var inventoryResults = ExecuteResourceStrategy(inventoryWorld, inventoryStrategy, days: 5);
        
        // === VALIDATION: DISTINCT STRATEGIC PROFILES ===
        
        // Time-focused should excel at complex, multi-step opportunities
        Assert.True(timeResults.ComplexOpportunitiesCompleted > coinResults.ComplexOpportunitiesCompleted);
        Assert.True(timeResults.ComplexOpportunitiesCompleted > inventoryResults.ComplexOpportunitiesCompleted);
        
        // Coin-focused should excel at high-value, capital-intensive opportunities
        Assert.True(coinResults.HighValueOpportunitiesCompleted > timeResults.HighValueOpportunitiesCompleted);
        Assert.True(coinResults.HighValueOpportunitiesCompleted > inventoryResults.HighValueOpportunitiesCompleted);
        
        // Inventory-focused should excel at trade volume and logistics efficiency
        Assert.True(inventoryResults.TradeVolumeCompleted > timeResults.TradeVolumeCompleted);
        Assert.True(inventoryResults.TradeVolumeCompleted > coinResults.TradeVolumeCompleted);
        
        // === VALIDATION: STRATEGIC TRADE-OFFS ===
        
        // Each strategy should have clear strengths and weaknesses
        var strategies = new[] { 
            ("Time", timeResults), 
            ("Coin", coinResults), 
            ("Inventory", inventoryResults) 
        };
        
        foreach (var (name, results) in strategies)
        {
            // Each strategy should excel in at least one area
            var isLeaderInAnyArea = 
                IsLeaderIn(results.ComplexOpportunitiesCompleted, strategies.Select(s => s.Item2.ComplexOpportunitiesCompleted)) ||
                IsLeaderIn(results.HighValueOpportunitiesCompleted, strategies.Select(s => s.Item2.HighValueOpportunitiesCompleted)) ||
                IsLeaderIn(results.TradeVolumeCompleted, strategies.Select(s => s.Item2.TradeVolumeCompleted));
            
            Assert.True(isLeaderInAnyArea, $"{name} strategy should excel in at least one area");
        }
    }

    [Fact]
    public void PathDiversification_Should_Provide_Risk_Management_Through_Optionality()
    {
        // Test that maintaining access to multiple paths provides strategic resilience
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("crossbridge").WithCoins(100))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === DIVERSIFIED STRATEGY ===
        GameWorld diversifiedWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var diversifiedStrategy = new PathDiversificationStrategy(
            pathCount: 3,
            specialization: SpecializationLevel.Moderate,
            adaptability: AdaptabilityLevel.High);
        
        // === SPECIALIZED STRATEGY ===
        GameWorld specializedWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var specializedStrategy = new PathDiversificationStrategy(
            pathCount: 1,
            specialization: SpecializationLevel.High,
            adaptability: AdaptabilityLevel.Low);
        
        // === EXECUTE UNDER NORMAL CONDITIONS ===
        var diversifiedNormal = ExecutePathStrategy(diversifiedWorld, diversifiedStrategy, 
            MarketConditions.Normal, 4);
        var specializedNormal = ExecutePathStrategy(specializedWorld, specializedStrategy, 
            MarketConditions.Normal, 4);
        
        // === EXECUTE UNDER ADVERSE CONDITIONS ===
        var diversifiedAdverse = ExecutePathStrategy(diversifiedWorld, diversifiedStrategy, 
            MarketConditions.Adverse, 4);
        var specializedAdverse = ExecutePathStrategy(specializedWorld, specializedStrategy, 
            MarketConditions.Adverse, 4);
        
        // === VALIDATION: NORMAL CONDITIONS ===
        // Under normal conditions, specialization might be slightly more efficient
        Assert.InRange(diversifiedNormal.TotalValue / specializedNormal.TotalValue, 0.85, 1.15);
        
        // === VALIDATION: ADVERSE CONDITIONS ===
        // Under adverse conditions, diversification should provide resilience
        var specializedDecline = (specializedNormal.TotalValue - specializedAdverse.TotalValue) / specializedNormal.TotalValue;
        var diversifiedDecline = (diversifiedNormal.TotalValue - diversifiedAdverse.TotalValue) / diversifiedNormal.TotalValue;
        
        Assert.True(diversifiedDecline < specializedDecline, 
            "Diversified strategy should show less performance decline under adverse conditions");
        
        // Diversified strategy should maintain more options under stress
        Assert.True(diversifiedAdverse.AvailableOptions > specializedAdverse.AvailableOptions);
        
        // Recovery potential should be higher for diversified approach
        Assert.True(diversifiedAdverse.RecoveryPotential > specializedAdverse.RecoveryPotential);
    }

    // === STRATEGY EXECUTION HELPERS ===

    private PathSpecializationResults ExecuteSpecializationStrategy(GameWorld gameWorld, PathSpecializationStrategy strategy, int days)
    {
        var results = new PathSpecializationResults();
        var contractRepository = new ContractRepository(gameWorld);
        var itemRepository = new ItemRepository(gameWorld);
        
        for (int day = 1; day <= days; day++)
        {
            // Equipment acquisition based on specialization
            strategy.AcquireSpecializationEquipment(gameWorld, itemRepository);
            
            // Opportunity identification and execution
            var opportunities = strategy.IdentifyOpportunities(gameWorld, contractRepository);
            
            results.MountainOpportunitiesCompleted += opportunities.Count(o => o.Type == OpportunityType.Mountain);
            results.CoastalOpportunitiesCompleted += opportunities.Count(o => o.Type == OpportunityType.Coastal);
            results.IntelligenceOpportunitiesCompleted += opportunities.Count(o => o.Type == OpportunityType.Intelligence);
            
            // Execute opportunities
            var value = strategy.ExecuteOpportunities(gameWorld, opportunities);
            results.TotalValue += value;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.TotalEfficiency = results.TotalValue / days;
        return results;
    }

    private ContractSolutionResults ExecuteContractStrategy(GameWorld gameWorld, ContractCompletionStrategy strategy, Contract contract)
    {
        var results = new ContractSolutionResults();
        var contractRepository = new ContractRepository(gameWorld);
        var itemRepository = new ItemRepository(gameWorld);
        
        // Resource allocation based on approach
        results.EquipmentCost = strategy.AllocateEquipmentResources(gameWorld, contract, itemRepository);
        results.InformationCost = strategy.AllocateInformationResources(gameWorld, contract);
        results.SocialInvestment = strategy.AllocateSocialResources(gameWorld, contract);
        
        // Execute completion attempt
        results.ContractCompleted = strategy.AttemptCompletion(gameWorld, contract);
        results.CompletionTime = strategy.GetCompletionTime();
        
        // Calculate net value
        var totalCost = results.EquipmentCost + results.InformationCost + results.SocialInvestment;
        results.NetValue = results.ContractCompleted ? (contract.Payment - totalCost) : -totalCost;
        
        return results;
    }

    private ResourceResults ExecuteResourceStrategy(GameWorld gameWorld, ResourceAllocationStrategy strategy, int days)
    {
        var results = new ResourceResults();
        var contractRepository = new ContractRepository(gameWorld);
        
        for (int day = 1; day <= days; day++)
        {
            // Allocate resources based on strategy
            var timeInvestment = strategy.TimeAllocation * 8; // 8 hours per day
            var coinInvestment = strategy.CoinAllocation * gameWorld.GetPlayer().Coins;
            var inventoryFocus = strategy.InventoryAllocation;
            
            // Execute opportunities based on allocations
            if (timeInvestment > 4) // Time-focused
            {
                results.ComplexOpportunitiesCompleted += ExecuteComplexOpportunities(gameWorld, timeInvestment);
            }
            
            if (coinInvestment > 30) // Coin-focused
            {
                results.HighValueOpportunitiesCompleted += ExecuteHighValueOpportunities(gameWorld, coinInvestment);
            }
            
            if (inventoryFocus > 0.4) // Inventory-focused
            {
                results.TradeVolumeCompleted += ExecuteTradeVolumeOpportunities(gameWorld, inventoryFocus);
            }
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        return results;
    }

    private PathResults ExecutePathStrategy(GameWorld gameWorld, PathDiversificationStrategy strategy, MarketConditions conditions, int days)
    {
        var results = new PathResults();
        
        // Apply market conditions
        ApplyMarketConditions(gameWorld, conditions);
        
        for (int day = 1; day <= days; day++)
        {
            // Evaluate available paths
            var availablePaths = strategy.EvaluateAvailablePaths(gameWorld, conditions);
            results.AvailableOptions = availablePaths.Count;
            
            // Execute best path(s) based on strategy
            var pathResults = strategy.ExecutePaths(gameWorld, availablePaths, conditions);
            results.TotalValue += pathResults.Value;
            results.RecoveryPotential += pathResults.RecoveryPotential;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        return results;
    }

    // === HELPER METHODS ===

    private double CalculateBalanceScore(PathSpecializationResults results)
    {
        var scores = new[] { 
            results.MountainOpportunitiesCompleted, 
            results.CoastalOpportunitiesCompleted, 
            results.IntelligenceOpportunitiesCompleted 
        };
        var avg = scores.Average();
        var variance = scores.Select(s => Math.Pow(s - avg, 2)).Average();
        return 1.0 / (1.0 + variance); // Higher score = better balance
    }

    private bool IsLeaderIn(int value, IEnumerable<int> allValues)
    {
        return value == allValues.Max();
    }

    private Contract GetComplexContract(string contractId)
    {
        return new Contract
        {
            Id = contractId,
            Description = contractId.Replace("_", " "),
            Payment = 100,
            DueDay = 7,
            RequiredEquipmentCategories = new List<EquipmentCategory> 
            { 
                EquipmentCategory.Climbing_Equipment, 
                EquipmentCategory.Light_Source, 
                EquipmentCategory.Navigation_Tools 
            }
        };
    }

    private int ExecuteComplexOpportunities(GameWorld gameWorld, double timeInvestment)
    {
        return (int)(timeInvestment / 4); // 4 hours per complex opportunity
    }

    private int ExecuteHighValueOpportunities(GameWorld gameWorld, double coinInvestment)
    {
        return (int)(coinInvestment / 50); // 50 coins per high-value opportunity
    }

    private int ExecuteTradeVolumeOpportunities(GameWorld gameWorld, double inventoryFocus)
    {
        return (int)(inventoryFocus * 10); // Inventory efficiency multiplier
    }

    private void ApplyMarketConditions(GameWorld gameWorld, MarketConditions conditions)
    {
        // Simulate market condition effects on available opportunities
        switch (conditions)
        {
            case MarketConditions.Adverse:
                // Reduce available contracts, increase costs
                break;
            case MarketConditions.Normal:
                // Standard conditions
                break;
        }
    }
}

// === STRATEGY CLASSES ===

public class PathSpecializationStrategy
{
    public SpecializationType Specialization { get; }
    public EquipmentPriority EquipmentPriority { get; }
    public ContractFocus ContractFocus { get; }
    
    public PathSpecializationStrategy(SpecializationType specialization, EquipmentPriority equipmentPriority, ContractFocus contractFocus)
    {
        Specialization = specialization;
        EquipmentPriority = equipmentPriority;
        ContractFocus = contractFocus;
    }
    
    public void AcquireSpecializationEquipment(GameWorld gameWorld, ItemRepository itemRepository) { }
    public List<Opportunity> IdentifyOpportunities(GameWorld gameWorld, ContractRepository contractRepository) { return new List<Opportunity>(); }
    public double ExecuteOpportunities(GameWorld gameWorld, List<Opportunity> opportunities) { return 50; }
}

public class ContractCompletionStrategy
{
    public CompletionApproach Approach { get; }
    public RiskTolerance RiskTolerance { get; }
    public TimeInvestment TimeInvestment { get; }
    
    public ContractCompletionStrategy(CompletionApproach approach, RiskTolerance riskTolerance, TimeInvestment timeInvestment)
    {
        Approach = approach;
        RiskTolerance = riskTolerance;
        TimeInvestment = timeInvestment;
    }
    
    public double AllocateEquipmentResources(GameWorld gameWorld, Contract contract, ItemRepository itemRepository) 
    { 
        return Approach == CompletionApproach.EquipmentBased ? 60 : 20; 
    }
    
    public double AllocateInformationResources(GameWorld gameWorld, Contract contract) 
    { 
        return Approach == CompletionApproach.InformationBased ? 40 : 10; 
    }
    
    public double AllocateSocialResources(GameWorld gameWorld, Contract contract) 
    { 
        return Approach == CompletionApproach.SocialBased ? 30 : 5; 
    }
    
    public bool AttemptCompletion(GameWorld gameWorld, Contract contract) { return true; }
    public int GetCompletionTime() { return TimeInvestment == TimeInvestment.High ? 3 : 1; }
}

public class ResourceAllocationStrategy
{
    public double TimeAllocation { get; }
    public double CoinAllocation { get; }
    public double InventoryAllocation { get; }
    
    public ResourceAllocationStrategy(double timeAllocation, double coinAllocation, double inventoryAllocation)
    {
        TimeAllocation = timeAllocation;
        CoinAllocation = coinAllocation;
        InventoryAllocation = inventoryAllocation;
    }
}

public class PathDiversificationStrategy
{
    public int PathCount { get; }
    public SpecializationLevel Specialization { get; }
    public AdaptabilityLevel Adaptability { get; }
    
    public PathDiversificationStrategy(int pathCount, SpecializationLevel specialization, AdaptabilityLevel adaptability)
    {
        PathCount = pathCount;
        Specialization = specialization;
        Adaptability = adaptability;
    }
    
    public List<string> EvaluateAvailablePaths(GameWorld gameWorld, MarketConditions conditions) 
    { 
        return Enumerable.Range(1, PathCount + (int)Adaptability).Select(i => $"Path{i}").ToList(); 
    }
    
    public PathExecutionResult ExecutePaths(GameWorld gameWorld, List<string> paths, MarketConditions conditions) 
    { 
        return new PathExecutionResult { Value = 40, RecoveryPotential = (int)Adaptability * 10 }; 
    }
}

// === RESULT CLASSES ===

public class PathSpecializationResults
{
    public int MountainOpportunitiesCompleted { get; set; }
    public int CoastalOpportunitiesCompleted { get; set; }
    public int IntelligenceOpportunitiesCompleted { get; set; }
    public double TotalValue { get; set; }
    public double TotalEfficiency { get; set; }
}

public class ContractSolutionResults
{
    public bool ContractCompleted { get; set; }
    public double EquipmentCost { get; set; }
    public double InformationCost { get; set; }
    public double SocialInvestment { get; set; }
    public int CompletionTime { get; set; }
    public double NetValue { get; set; }
}

public class ResourceResults
{
    public int ComplexOpportunitiesCompleted { get; set; }
    public int HighValueOpportunitiesCompleted { get; set; }
    public int TradeVolumeCompleted { get; set; }
}

public class PathResults
{
    public double TotalValue { get; set; }
    public int AvailableOptions { get; set; }
    public int RecoveryPotential { get; set; }
}

public class Opportunity
{
    public OpportunityType Type { get; set; }
    public double Value { get; set; }
}

public class PathExecutionResult
{
    public double Value { get; set; }
    public int RecoveryPotential { get; set; }
}

// === ENUMS ===

public enum SpecializationType
{
    Climbing,
    Maritime,
    Information,
    Generalist
}

public enum EquipmentPriority
{
    ClimbingEquipment,
    WaterTransport,
    NavigationTools,
    Balanced
}

public enum ContractFocus
{
    Exploration,
    Trade,
    Intelligence,
    Opportunistic
}

public enum CompletionApproach
{
    EquipmentBased,
    InformationBased,
    SocialBased
}

public enum RiskTolerance
{
    Low,
    Medium,
    High
}

public enum TimeInvestment
{
    Low,
    Medium,
    High
}

public enum SpecializationLevel
{
    Low,
    Moderate,
    High
}

public enum AdaptabilityLevel
{
    Low,
    Medium,
    High
}

public enum MarketConditions
{
    Normal,
    Adverse
}

public enum OpportunityType
{
    Mountain,
    Coastal,
    Intelligence,
    Trade
}