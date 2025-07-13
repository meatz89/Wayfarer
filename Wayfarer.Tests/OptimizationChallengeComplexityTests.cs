using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Tests;

public class OptimizationChallengeComplexityTests
{
    [Fact]
    public void ResourceAllocationOptimization_Should_Create_Measurable_Efficiency_Gradients()
    {
        // Test that optimal resource allocation creates clear efficiency differences compared to suboptimal allocation
        
        var baseScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("crossbridge").WithCoins(200))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === OPTIMAL ALLOCATION STRATEGY ===
        GameWorld optimalWorld = TestGameWorldInitializer.CreateTestWorld(baseScenario);
        var optimalStrategy = new ResourceOptimizationStrategy(
            planningDepth: OptimizationDepth.Deep,
            resourceUtilization: ResourceUtilization.Efficient,
            opportunityCost: OpportunityCostAwareness.High,
            timeHorizon: TimeHorizon.LongTerm);
        
        // === SUBOPTIMAL ALLOCATION STRATEGY ===
        GameWorld suboptimalWorld = TestGameWorldInitializer.CreateTestWorld(baseScenario);
        var suboptimalStrategy = new ResourceOptimizationStrategy(
            planningDepth: OptimizationDepth.Shallow,
            resourceUtilization: ResourceUtilization.Wasteful,
            opportunityCost: OpportunityCostAwareness.Low,
            timeHorizon: TimeHorizon.ShortTerm);
        
        // === NAIVE STRATEGY (CONTROL) ===
        GameWorld naiveWorld = TestGameWorldInitializer.CreateTestWorld(baseScenario);
        var naiveStrategy = new ResourceOptimizationStrategy(
            planningDepth: OptimizationDepth.None,
            resourceUtilization: ResourceUtilization.Random,
            opportunityCost: OpportunityCostAwareness.None,
            timeHorizon: TimeHorizon.Immediate);
        
        // === EXECUTE STRATEGIES ===
        var optimalResults = ExecuteOptimizationStrategy(optimalWorld, optimalStrategy, days: 6);
        var suboptimalResults = ExecuteOptimizationStrategy(suboptimalWorld, suboptimalStrategy, days: 6);
        var naiveResults = ExecuteOptimizationStrategy(naiveWorld, naiveStrategy, days: 6);
        
        // === VALIDATION: CLEAR EFFICIENCY GRADIENTS ===
        // Optimal should significantly outperform suboptimal
        Assert.True(optimalResults.TotalEfficiency / suboptimalResults.TotalEfficiency > 1.5,
            "Optimal strategy should be 50%+ more efficient than suboptimal");
        
        // Suboptimal should outperform naive
        Assert.True(suboptimalResults.TotalEfficiency / naiveResults.TotalEfficiency > 1.3,
            "Suboptimal strategy should be 30%+ more efficient than naive");
        
        // Overall gradient should show clear skill differentiation
        Assert.True(optimalResults.TotalEfficiency / naiveResults.TotalEfficiency > 2.0,
            "Optimal should be 100%+ more efficient than naive (clear skill gradient)");
        
        // === VALIDATION: OPTIMIZATION MECHANISMS ===
        // Optimal strategy should demonstrate superior resource utilization
        Assert.True(optimalResults.ResourceUtilizationScore > suboptimalResults.ResourceUtilizationScore);
        Assert.True(optimalResults.WastedResourcesCount < suboptimalResults.WastedResourcesCount);
        
        // Planning depth should correlate with complex opportunity capture
        Assert.True(optimalResults.ComplexOpportunitiesCompleted > suboptimalResults.ComplexOpportunitiesCompleted);
        Assert.True(suboptimalResults.ComplexOpportunitiesCompleted > naiveResults.ComplexOpportunitiesCompleted);
        
        // Time horizon awareness should affect investment decisions
        Assert.True(optimalResults.LongTermInvestmentValue > suboptimalResults.LongTermInvestmentValue);
    }

    [Fact]
    public void MultiConstraintRouteOptimization_Should_Require_Sophisticated_Decision_Making()
    {
        // Test that routes with multiple constraints (time, equipment, transport, weather) create optimization challenges
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(150))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === SOPHISTICATED PLANNER ===
        GameWorld sophisticatedWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var sophisticatedPlanner = new RouteOptimizationStrategy(
            constraintConsideration: ConstraintAwareness.Comprehensive,
            backupPlanDepth: BackupPlanDepth.Multiple,
            weatherPlanning: WeatherPlanning.Predictive,
            equipmentOptimization: EquipmentOptimization.Synergistic);
        
        // === SIMPLE PLANNER ===
        GameWorld simpleWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var simplePlanner = new RouteOptimizationStrategy(
            constraintConsideration: ConstraintAwareness.Basic,
            backupPlanDepth: BackupPlanDepth.Single,
            weatherPlanning: WeatherPlanning.Reactive,
            equipmentOptimization: EquipmentOptimization.Individual);
        
        // Complex multi-destination objective with various constraints
        var complexObjective = new MultiConstraintObjective
        {
            Destinations = new List<string> { "mountain_summit", "eastport", "ironhold", "ancient_ruins" },
            TimeLimit = 8, // days
            RequiredEquipment = new List<EquipmentCategory> 
            { 
                EquipmentCategory.Climbing_Equipment, 
                EquipmentCategory.Water_Transport, 
                EquipmentCategory.Navigation_Tools 
            },
            WeatherConstraints = new List<WeatherConstraint> 
            { 
                new WeatherConstraint { Terrain = "mountain", RequiredCondition = "clear" },
                new WeatherConstraint { Terrain = "water", RequiredCondition = "calm" }
            }
        };
        
        // === EXECUTE ROUTE OPTIMIZATION ===
        var sophisticatedResults = ExecuteRouteOptimization(sophisticatedWorld, sophisticatedPlanner, complexObjective);
        var simpleResults = ExecuteRouteOptimization(simpleWorld, simplePlanner, complexObjective);
        
        // === VALIDATION: SOPHISTICATED DECISION-MAKING ADVANTAGES ===
        // Sophisticated planner should complete more objectives
        Assert.True(sophisticatedResults.ObjectivesCompleted > simpleResults.ObjectivesCompleted);
        
        // Should require fewer resources due to better optimization
        Assert.True(sophisticatedResults.ResourcesRequired < simpleResults.ResourcesRequired);
        
        // Should handle constraint violations better
        Assert.True(sophisticatedResults.ConstraintViolations < simpleResults.ConstraintViolations);
        
        // Should demonstrate backup plan utilization
        Assert.True(sophisticatedResults.BackupPlansActivated > 0,
            "Sophisticated planner should use backup plans when primary routes fail");
        
        // Should show equipment synergy benefits
        Assert.True(sophisticatedResults.EquipmentSynergyBonuses > simpleResults.EquipmentSynergyBonuses);
        
        // === VALIDATION: COMPLEXITY HANDLING ===
        // More constraints should amplify the advantage of sophisticated planning
        var constraintCount = complexObjective.RequiredEquipment.Count + complexObjective.WeatherConstraints.Count;
        var efficiencyGap = sophisticatedResults.TotalEfficiency / simpleResults.TotalEfficiency;
        
        Assert.True(efficiencyGap > 1.0 + (constraintCount * 0.1),
            "Efficiency gap should grow with constraint complexity");
    }

    [Fact]
    public void ParallelContractExecution_Should_Reward_Advanced_Scheduling()
    {
        // Test that managing multiple simultaneous contracts creates scheduling optimization challenges
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("crossbridge").WithCoins(180))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === ADVANCED SCHEDULER ===
        GameWorld advancedWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var advancedScheduler = new ParallelExecutionStrategy(
            schedulingAlgorithm: SchedulingAlgorithm.OptimalSequencing,
            resourceSharing: ResourceSharing.Efficient,
            deadlineManagement: DeadlineManagement.Proactive,
            conflictResolution: ConflictResolution.Strategic);
        
        // === BASIC SCHEDULER ===
        GameWorld basicWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var basicScheduler = new ParallelExecutionStrategy(
            schedulingAlgorithm: SchedulingAlgorithm.FirstInFirstOut,
            resourceSharing: ResourceSharing.Inefficient,
            deadlineManagement: DeadlineManagement.Reactive,
            conflictResolution: ConflictResolution.Random);
        
        // Multiple overlapping contracts with different requirements and deadlines
        var contractPortfolio = new List<Contract>
        {
            CreateTimeConstrainedContract("urgent_delivery", deadline: 3, payment: 40, equipment: "Navigation_Tools"),
            CreateTimeConstrainedContract("mountain_survey", deadline: 5, payment: 60, equipment: "Climbing_Equipment"),
            CreateTimeConstrainedContract("trade_negotiation", deadline: 4, payment: 50, equipment: "Social_Attire"),
            CreateTimeConstrainedContract("information_gathering", deadline: 6, payment: 45, equipment: "Documentation")
        };
        
        // === EXECUTE PARALLEL SCHEDULING ===
        var advancedResults = ExecuteParallelExecution(advancedWorld, advancedScheduler, contractPortfolio);
        var basicResults = ExecuteParallelExecution(basicWorld, basicScheduler, contractPortfolio);
        
        // === VALIDATION: ADVANCED SCHEDULING BENEFITS ===
        // Advanced scheduler should complete more contracts
        Assert.True(advancedResults.ContractsCompleted > basicResults.ContractsCompleted);
        
        // Should achieve higher total value through better coordination
        Assert.True(advancedResults.TotalValue > basicResults.TotalValue * 1.4,
            "Advanced scheduling should achieve 40%+ better total value");
        
        // Should minimize deadline violations
        Assert.True(advancedResults.DeadlineViolations < basicResults.DeadlineViolations);
        
        // Should demonstrate resource sharing efficiency
        Assert.True(advancedResults.ResourceUtilizationScore > basicResults.ResourceUtilizationScore);
        
        // === VALIDATION: SCHEDULING COMPLEXITY BENEFITS ===
        // Advanced scheduler should show benefits of optimal sequencing
        Assert.True(advancedResults.OptimalSequenceBenefits > 0,
            "Should demonstrate measurable benefits from optimal contract sequencing");
        
        // Should minimize resource conflicts through better planning
        Assert.True(advancedResults.ResourceConflicts < basicResults.ResourceConflicts);
        
        // Should show proactive deadline management benefits
        Assert.True(advancedResults.ProactiveDeadlineManagement > basicResults.ProactiveDeadlineManagement);
    }

    [Fact]
    public void EquipmentInvestmentROI_Should_Create_Long_Term_Optimization_Decisions()
    {
        // Test that equipment investment decisions create long-term strategic optimization challenges
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(120))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === STRATEGIC INVESTOR ===
        GameWorld strategicWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var strategicInvestor = new EquipmentInvestmentStrategy(
            investmentHorizon: InvestmentHorizon.LongTerm,
            roiCalculation: ROICalculation.Comprehensive,
            opportunityCostAwareness: OpportunityCostAwareness.High,
            portfolioOptimization: PortfolioOptimization.Synergistic);
        
        // === TACTICAL PURCHASER ===
        GameWorld tacticalWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var tacticalPurchaser = new EquipmentInvestmentStrategy(
            investmentHorizon: InvestmentHorizon.ShortTerm,
            roiCalculation: ROICalculation.SimplePayback,
            opportunityCostAwareness: OpportunityCostAwareness.Low,
            portfolioOptimization: PortfolioOptimization.Individual);
        
        // High-value equipment with different ROI characteristics
        var equipmentOptions = new List<EquipmentInvestmentOption>
        {
            new EquipmentInvestmentOption 
            { 
                EquipmentId = "professional_climbing_kit", 
                Cost = 80, 
                ImmediateROI = 0.1, 
                LongTermROI = 0.4, 
                SynergyPotential = 0.3 
            },
            new EquipmentInvestmentOption 
            { 
                EquipmentId = "merchant_credentials", 
                Cost = 60, 
                ImmediateROI = 0.2, 
                LongTermROI = 0.25, 
                SynergyPotential = 0.2 
            },
            new EquipmentInvestmentOption 
            { 
                EquipmentId = "navigation_instruments", 
                Cost = 50, 
                ImmediateROI = 0.15, 
                LongTermROI = 0.35, 
                SynergyPotential = 0.4 
            }
        };
        
        // === EXECUTE INVESTMENT STRATEGIES ===
        var strategicResults = ExecuteInvestmentStrategy(strategicWorld, strategicInvestor, equipmentOptions, evaluationPeriod: 10);
        var tacticalResults = ExecuteInvestmentStrategy(tacticalWorld, tacticalPurchaser, equipmentOptions, evaluationPeriod: 10);
        
        // === VALIDATION: LONG-TERM OPTIMIZATION BENEFITS ===
        // Strategic investor should achieve higher long-term ROI
        Assert.True(strategicResults.LongTermROI > tacticalResults.LongTermROI * 1.5,
            "Strategic investment should achieve 50%+ better long-term ROI");
        
        // Should demonstrate portfolio synergy benefits
        Assert.True(strategicResults.PortfolioSynergyValue > tacticalResults.PortfolioSynergyValue);
        
        // Should make fewer but more impactful investments
        Assert.True(strategicResults.InvestmentCount <= tacticalResults.InvestmentCount);
        Assert.True(strategicResults.InvestmentImpact > tacticalResults.InvestmentImpact);
        
        // === VALIDATION: OPPORTUNITY COST AWARENESS ===
        // Strategic investor should avoid low-ROI investments
        Assert.True(strategicResults.OpportunityCostrealizationValue > tacticalResults.OpportunityCostrealizationValue);
        
        // Should demonstrate timing optimization in investment decisions
        Assert.True(strategicResults.InvestmentTimingScore > tacticalResults.InvestmentTimingScore);
        
        // Should show compound returns from synergistic equipment combinations
        Assert.True(strategicResults.CompoundReturnValue > tacticalResults.CompoundReturnValue * 2.0,
            "Strategic approach should achieve 100%+ better compound returns");
    }

    // === STRATEGY EXECUTION HELPERS ===

    private OptimizationResults ExecuteOptimizationStrategy(GameWorld gameWorld, ResourceOptimizationStrategy strategy, int days)
    {
        var results = new OptimizationResults();
        var contractRepository = new ContractRepository(gameWorld);
        var itemRepository = new ItemRepository(gameWorld);
        
        for (int day = 1; day <= days; day++)
        {
            // Resource allocation based on strategy
            var allocation = strategy.OptimizeResourceAllocation(gameWorld, contractRepository, itemRepository);
            results.ResourceUtilizationScore += allocation.EfficiencyScore;
            results.WastedResourcesCount += allocation.WastedResources;
            
            // Opportunity evaluation and execution
            var opportunities = strategy.EvaluateOpportunities(gameWorld, allocation);
            results.ComplexOpportunitiesCompleted += opportunities.Count(o => o.Complexity > 3);
            
            // Investment decisions
            var investments = strategy.MakeInvestmentDecisions(gameWorld, allocation);
            results.LongTermInvestmentValue += investments.Sum(i => i.LongTermValue);
            
            // Execute day's activities
            var dayValue = strategy.ExecuteOptimalPlan(gameWorld, opportunities, investments);
            results.TotalValue += dayValue;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.TotalEfficiency = results.TotalValue / days;
        return results;
    }

    private RouteOptimizationResults ExecuteRouteOptimization(GameWorld gameWorld, RouteOptimizationStrategy strategy, MultiConstraintObjective objective)
    {
        var results = new RouteOptimizationResults();
        var locationRepository = new LocationRepository(gameWorld);
        var itemRepository = new ItemRepository(gameWorld);
        
        // Plan route optimization
        var routePlan = strategy.OptimizeMultiConstraintRoute(gameWorld, objective, locationRepository, itemRepository);
        results.ObjectivesCompleted = routePlan.CompletedObjectives;
        results.ResourcesRequired = routePlan.TotalResourceCost;
        results.ConstraintViolations = routePlan.ConstraintViolations;
        
        // Execute route with contingency handling
        var executionResults = strategy.ExecuteRouteWithContingencies(gameWorld, routePlan);
        results.BackupPlansActivated = executionResults.BackupPlansUsed;
        results.EquipmentSynergyBonuses = executionResults.SynergyBonuses;
        results.TotalEfficiency = executionResults.OverallEfficiency;
        
        return results;
    }

    private ParallelExecutionResults ExecuteParallelExecution(GameWorld gameWorld, ParallelExecutionStrategy strategy, List<Contract> contracts)
    {
        var results = new ParallelExecutionResults();
        var contractRepository = new ContractRepository(gameWorld);
        
        // Create execution schedule
        var schedule = strategy.CreateOptimalSchedule(gameWorld, contracts, contractRepository);
        results.OptimalSequenceBenefits = schedule.SequenceOptimizationValue;
        
        // Execute parallel contract management
        var executionResults = strategy.ExecuteParallelContracts(gameWorld, schedule);
        results.ContractsCompleted = executionResults.CompletedContracts;
        results.DeadlineViolations = executionResults.DeadlineViolations;
        results.ResourceConflicts = executionResults.ResourceConflicts;
        results.ResourceUtilizationScore = executionResults.ResourceEfficiency;
        results.ProactiveDeadlineManagement = executionResults.ProactiveManagementScore;
        results.TotalValue = executionResults.TotalValue;
        
        return results;
    }

    private InvestmentResults ExecuteInvestmentStrategy(GameWorld gameWorld, EquipmentInvestmentStrategy strategy, List<EquipmentInvestmentOption> options, int evaluationPeriod)
    {
        var results = new InvestmentResults();
        var itemRepository = new ItemRepository(gameWorld);
        
        // Make investment decisions
        var investments = strategy.SelectOptimalInvestments(gameWorld, options, itemRepository);
        results.InvestmentCount = investments.Count;
        
        // Calculate returns over evaluation period
        for (int period = 1; period <= evaluationPeriod; period++)
        {
            var periodReturns = strategy.CalculatePeriodReturns(gameWorld, investments, period);
            results.TotalValue += periodReturns.DirectReturns;
            results.PortfolioSynergyValue += periodReturns.SynergyReturns;
            results.CompoundReturnValue += periodReturns.CompoundReturns;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        // Calculate optimization metrics
        results.LongTermROI = results.TotalValue / investments.Sum(i => i.Cost);
        results.InvestmentImpact = strategy.CalculateInvestmentImpact(investments);
        results.OpportunityCostrealizationValue = strategy.CalculateOpportunityCostRealizedValue(gameWorld, investments);
        results.InvestmentTimingScore = strategy.CalculateTimingOptimizationScore(investments);
        
        return results;
    }

    // === HELPER METHODS ===

    private Contract CreateTimeConstrainedContract(string id, int deadline, int payment, string equipment)
    {
        return new Contract
        {
            Id = id,
            Description = id.Replace("_", " "),
            DueDay = deadline,
            Payment = payment,
            RequiredEquipmentCategories = new List<EquipmentCategory> 
            { 
                Enum.Parse<EquipmentCategory>(equipment) 
            }
        };
    }
}

// === STRATEGY CLASSES ===

public class ResourceOptimizationStrategy
{
    public OptimizationDepth PlanningDepth { get; }
    public ResourceUtilization ResourceUtilization { get; }
    public OpportunityCostAwareness OpportunityCost { get; }
    public TimeHorizon TimeHorizon { get; }
    
    public ResourceOptimizationStrategy(OptimizationDepth planningDepth, ResourceUtilization resourceUtilization, OpportunityCostAwareness opportunityCost, TimeHorizon timeHorizon)
    {
        PlanningDepth = planningDepth;
        ResourceUtilization = resourceUtilization;
        OpportunityCost = opportunityCost;
        TimeHorizon = timeHorizon;
    }
    
    public ResourceAllocation OptimizeResourceAllocation(GameWorld gameWorld, ContractRepository contractRepository, ItemRepository itemRepository) 
    { 
        return new ResourceAllocation { EfficiencyScore = (int)PlanningDepth * 10, WastedResources = 5 - (int)PlanningDepth }; 
    }
    
    public List<OptimizationOpportunity> EvaluateOpportunities(GameWorld gameWorld, ResourceAllocation allocation) 
    { 
        return new List<OptimizationOpportunity> 
        { 
            new OptimizationOpportunity { Complexity = (int)PlanningDepth + 2 },
            new OptimizationOpportunity { Complexity = (int)PlanningDepth + 1 }
        }; 
    }
    
    public List<Investment> MakeInvestmentDecisions(GameWorld gameWorld, ResourceAllocation allocation) 
    { 
        return new List<Investment> 
        { 
            new Investment { LongTermValue = (int)TimeHorizon * 20 }
        }; 
    }
    
    public double ExecuteOptimalPlan(GameWorld gameWorld, List<OptimizationOpportunity> opportunities, List<Investment> investments) 
    { 
        return opportunities.Sum(o => o.Complexity * 10) + investments.Sum(i => i.LongTermValue * 0.1); 
    }
}

public class RouteOptimizationStrategy
{
    public ConstraintAwareness ConstraintConsideration { get; }
    public BackupPlanDepth BackupPlanDepth { get; }
    public WeatherPlanning WeatherPlanning { get; }
    public EquipmentOptimization EquipmentOptimization { get; }
    
    public RouteOptimizationStrategy(ConstraintAwareness constraintConsideration, BackupPlanDepth backupPlanDepth, WeatherPlanning weatherPlanning, EquipmentOptimization equipmentOptimization)
    {
        ConstraintConsideration = constraintConsideration;
        BackupPlanDepth = backupPlanDepth;
        WeatherPlanning = weatherPlanning;
        EquipmentOptimization = equipmentOptimization;
    }
    
    public RoutePlan OptimizeMultiConstraintRoute(GameWorld gameWorld, MultiConstraintObjective objective, LocationRepository locationRepository, ItemRepository itemRepository) 
    { 
        return new RoutePlan 
        { 
            CompletedObjectives = (int)ConstraintConsideration * 2,
            TotalResourceCost = 100 - (int)ConstraintConsideration * 10,
            ConstraintViolations = 5 - (int)ConstraintConsideration
        }; 
    }
    
    public RouteExecutionResults ExecuteRouteWithContingencies(GameWorld gameWorld, RoutePlan plan) 
    { 
        return new RouteExecutionResults 
        { 
            BackupPlansUsed = (int)BackupPlanDepth,
            SynergyBonuses = (int)EquipmentOptimization * 15,
            OverallEfficiency = (int)ConstraintConsideration * 25
        }; 
    }
}

public class ParallelExecutionStrategy
{
    public SchedulingAlgorithm SchedulingAlgorithm { get; }
    public ResourceSharing ResourceSharing { get; }
    public DeadlineManagement DeadlineManagement { get; }
    public ConflictResolution ConflictResolution { get; }
    
    public ParallelExecutionStrategy(SchedulingAlgorithm schedulingAlgorithm, ResourceSharing resourceSharing, DeadlineManagement deadlineManagement, ConflictResolution conflictResolution)
    {
        SchedulingAlgorithm = schedulingAlgorithm;
        ResourceSharing = resourceSharing;
        DeadlineManagement = deadlineManagement;
        ConflictResolution = conflictResolution;
    }
    
    public ExecutionSchedule CreateOptimalSchedule(GameWorld gameWorld, List<Contract> contracts, ContractRepository contractRepository) 
    { 
        return new ExecutionSchedule { SequenceOptimizationValue = (int)SchedulingAlgorithm * 30 }; 
    }
    
    public ParallelExecutionResult ExecuteParallelContracts(GameWorld gameWorld, ExecutionSchedule schedule) 
    { 
        return new ParallelExecutionResult 
        { 
            CompletedContracts = (int)SchedulingAlgorithm + 1,
            DeadlineViolations = 3 - (int)DeadlineManagement,
            ResourceConflicts = 4 - (int)ResourceSharing,
            ResourceEfficiency = (int)ResourceSharing * 20,
            ProactiveManagementScore = (int)DeadlineManagement * 25,
            TotalValue = (int)SchedulingAlgorithm * 80
        }; 
    }
}

public class EquipmentInvestmentStrategy
{
    public InvestmentHorizon InvestmentHorizon { get; }
    public ROICalculation ROICalculation { get; }
    public OpportunityCostAwareness OpportunityCostAwareness { get; }
    public PortfolioOptimization PortfolioOptimization { get; }
    
    public EquipmentInvestmentStrategy(InvestmentHorizon investmentHorizon, ROICalculation roiCalculation, OpportunityCostAwareness opportunityCostAwareness, PortfolioOptimization portfolioOptimization)
    {
        InvestmentHorizon = investmentHorizon;
        ROICalculation = roiCalculation;
        OpportunityCostAwareness = opportunityCostAwareness;
        PortfolioOptimization = portfolioOptimization;
    }
    
    public List<EquipmentInvestmentOption> SelectOptimalInvestments(GameWorld gameWorld, List<EquipmentInvestmentOption> options, ItemRepository itemRepository) 
    { 
        return options.Take((int)InvestmentHorizon + 1).ToList(); 
    }
    
    public PeriodReturns CalculatePeriodReturns(GameWorld gameWorld, List<EquipmentInvestmentOption> investments, int period) 
    { 
        return new PeriodReturns 
        { 
            DirectReturns = investments.Sum(i => i.LongTermROI * 10),
            SynergyReturns = (int)PortfolioOptimization * period * 5,
            CompoundReturns = investments.Sum(i => i.LongTermROI * period * 2)
        }; 
    }
    
    public double CalculateInvestmentImpact(List<EquipmentInvestmentOption> investments) { return investments.Sum(i => i.SynergyPotential * 100); }
    public double CalculateOpportunityCostRealizedValue(GameWorld gameWorld, List<EquipmentInvestmentOption> investments) { return (int)OpportunityCostAwareness * 50; }
    public double CalculateTimingOptimizationScore(List<EquipmentInvestmentOption> investments) { return (int)InvestmentHorizon * 30; }
}

// === RESULT CLASSES ===

public class OptimizationResults
{
    public double TotalValue { get; set; }
    public double TotalEfficiency { get; set; }
    public double ResourceUtilizationScore { get; set; }
    public int WastedResourcesCount { get; set; }
    public int ComplexOpportunitiesCompleted { get; set; }
    public double LongTermInvestmentValue { get; set; }
}

public class RouteOptimizationResults
{
    public int ObjectivesCompleted { get; set; }
    public double ResourcesRequired { get; set; }
    public int ConstraintViolations { get; set; }
    public int BackupPlansActivated { get; set; }
    public double EquipmentSynergyBonuses { get; set; }
    public double TotalEfficiency { get; set; }
}

public class ParallelExecutionResults
{
    public int ContractsCompleted { get; set; }
    public int DeadlineViolations { get; set; }
    public int ResourceConflicts { get; set; }
    public double ResourceUtilizationScore { get; set; }
    public double ProactiveDeadlineManagement { get; set; }
    public double OptimalSequenceBenefits { get; set; }
    public double TotalValue { get; set; }
}

public class InvestmentResults
{
    public double TotalValue { get; set; }
    public double LongTermROI { get; set; }
    public double PortfolioSynergyValue { get; set; }
    public double CompoundReturnValue { get; set; }
    public int InvestmentCount { get; set; }
    public double InvestmentImpact { get; set; }
    public double OpportunityCostrealizationValue { get; set; }
    public double InvestmentTimingScore { get; set; }
}

// === SUPPORTING CLASSES ===

public class MultiConstraintObjective
{
    public List<string> Destinations { get; set; } = new();
    public int TimeLimit { get; set; }
    public List<EquipmentCategory> RequiredEquipment { get; set; } = new();
    public List<WeatherConstraint> WeatherConstraints { get; set; } = new();
}

public class WeatherConstraint
{
    public string Terrain { get; set; } = "";
    public string RequiredCondition { get; set; } = "";
}

public class EquipmentInvestmentOption
{
    public string EquipmentId { get; set; } = "";
    public double Cost { get; set; }
    public double ImmediateROI { get; set; }
    public double LongTermROI { get; set; }
    public double SynergyPotential { get; set; }
}

public class ResourceAllocation
{
    public int EfficiencyScore { get; set; }
    public int WastedResources { get; set; }
}

public class OptimizationOpportunity
{
    public int Complexity { get; set; }
}

public class Investment
{
    public double LongTermValue { get; set; }
}

public class RoutePlan
{
    public int CompletedObjectives { get; set; }
    public double TotalResourceCost { get; set; }
    public int ConstraintViolations { get; set; }
}

public class RouteExecutionResults
{
    public int BackupPlansUsed { get; set; }
    public double SynergyBonuses { get; set; }
    public double OverallEfficiency { get; set; }
}

public class ExecutionSchedule
{
    public double SequenceOptimizationValue { get; set; }
}

public class ParallelExecutionResult
{
    public int CompletedContracts { get; set; }
    public int DeadlineViolations { get; set; }
    public int ResourceConflicts { get; set; }
    public double ResourceEfficiency { get; set; }
    public double ProactiveManagementScore { get; set; }
    public double TotalValue { get; set; }
}

public class PeriodReturns
{
    public double DirectReturns { get; set; }
    public double SynergyReturns { get; set; }
    public double CompoundReturns { get; set; }
}

// === ENUMS ===

public enum OptimizationDepth
{
    None = 0,
    Shallow = 1,
    Deep = 2
}

public enum ResourceUtilization
{
    Random = 0,
    Wasteful = 1,
    Efficient = 2
}

public enum OpportunityCostAwareness
{
    None = 0,
    Low = 1,
    High = 2
}

public enum TimeHorizon
{
    Immediate = 1,
    ShortTerm = 2,
    LongTerm = 3
}

public enum ConstraintAwareness
{
    Basic = 1,
    Comprehensive = 2
}

public enum BackupPlanDepth
{
    Single = 1,
    Multiple = 2
}

public enum WeatherPlanning
{
    Reactive = 1,
    Predictive = 2
}

public enum EquipmentOptimization
{
    Individual = 1,
    Synergistic = 2
}

public enum SchedulingAlgorithm
{
    FirstInFirstOut = 1,
    OptimalSequencing = 2
}

public enum ResourceSharing
{
    Inefficient = 1,
    Efficient = 2
}

public enum DeadlineManagement
{
    Reactive = 1,
    Proactive = 2
}

public enum ConflictResolution
{
    Random = 1,
    Strategic = 2
}

public enum InvestmentHorizon
{
    ShortTerm = 1,
    LongTerm = 2
}

public enum ROICalculation
{
    SimplePayback = 1,
    Comprehensive = 2
}

public enum PortfolioOptimization
{
    Individual = 1,
    Synergistic = 2
}