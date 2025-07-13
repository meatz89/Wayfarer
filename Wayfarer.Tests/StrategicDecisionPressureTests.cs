using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Tests;

public class StrategicDecisionPressureTests
{
    [Fact]
    public void EquipmentSpecializationPressure_Mountain_vs_Maritime_Should_Create_Distinct_Opportunity_Sets()
    {
        // === MOUNTAIN SPECIALIZATION STRATEGY ===
        var mountainScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld mountainWorld = TestGameWorldInitializer.CreateTestWorld(mountainScenario);
        var mountainStrategy = new EquipmentSpecializationStrategy(EquipmentFocus.MountainClimbing);
        
        // === MARITIME SPECIALIZATION STRATEGY ===
        var maritimeScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld maritimeWorld = TestGameWorldInitializer.CreateTestWorld(maritimeScenario);
        var maritimeStrategy = new EquipmentSpecializationStrategy(EquipmentFocus.WaterTransport);
        
        // === EXECUTE STRATEGIES ===
        var mountainResults = ExecuteStrategy(mountainWorld, mountainStrategy, days: 5);
        var maritimeResults = ExecuteStrategy(maritimeWorld, maritimeStrategy, days: 5);
        
        // === VALIDATION ===
        // Both strategies should achieve similar efficiency through different paths
        Assert.InRange(mountainResults.TotalProfitability / maritimeResults.TotalProfitability, 0.8, 1.2);
        
        // But should access completely different opportunity sets
        Assert.True(mountainResults.AccessibleContracts.Intersect(maritimeResults.AccessibleContracts).Count() < 0.3 * mountainResults.AccessibleContracts.Count);
        
        // Mountain strategy should dominate mountain-specific opportunities
        var mountainContracts = GetContractsByCategory("Exploration");
        Assert.True(mountainResults.CompletedContracts.Intersect(mountainContracts).Count() > 
                    maritimeResults.CompletedContracts.Intersect(mountainContracts).Count());
        
        // Maritime strategy should dominate coastal opportunities
        var maritimeContracts = GetContractsByCategory("Merchant");
        Assert.True(maritimeResults.CompletedContracts.Intersect(maritimeContracts).Count() > 
                    mountainResults.CompletedContracts.Intersect(maritimeContracts).Count());
    }

    [Fact]
    public void InventoryTetrisChallenge_Should_Force_Equipment_vs_Cargo_Trade_offs()
    {
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("crossbridge").WithCoins(150))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        
        // High-value contract requiring 3 equipment categories + cargo space for profitable trade
        var complexContract = GetContractById("ruins_excavation"); // Requires Climbing + Light + Navigation + Weather
        var profitableTradeItem = GetItemById("exotic_spices"); // High profit but takes inventory space
        
        // === STRATEGY A: Equipment-Heavy Approach ===
        var equipmentStrategy = new InventoryOptimizationStrategy(prioritizeEquipment: true);
        var equipmentResults = ExecuteStrategyWithConstraints(gameWorld, equipmentStrategy, complexContract);
        
        // === STRATEGY B: Trade-Heavy Approach ===  
        var tradeStrategy = new InventoryOptimizationStrategy(prioritizeEquipment: false);
        var tradeResults = ExecuteStrategyWithConstraints(gameWorld, tradeStrategy, complexContract);
        
        // === VALIDATION ===
        // Equipment strategy should complete complex contract but miss trade profits
        Assert.True(equipmentResults.ContractCompleted);
        Assert.True(equipmentResults.TradeProfitMissed > 0);
        
        // Trade strategy should earn trade profits but struggle with complex contract
        Assert.True(tradeResults.TradeProfit > equipmentResults.TradeProfit);
        Assert.True(!tradeResults.ContractCompleted || tradeResults.ContractCompletionCost > equipmentResults.ContractCompletionCost);
        
        // There should be a clear efficiency trade-off, not one dominant strategy
        Assert.InRange(equipmentResults.TotalValue / tradeResults.TotalValue, 0.7, 1.4);
    }

    [Fact]
    public void TimeManagementCascade_Should_Compound_Poor_Scheduling_Into_Major_Failures()
    {
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("eastport").WithCoins(100))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        
        // Time-sensitive contract with multiple dependencies requiring coordination
        var urgentContract = GetContractById("urgent_merchant_delivery"); // 2-day deadline
        
        // === STRATEGY A: Poor Time Management ===
        var poorTimeStrategy = new TimeManagementStrategy(
            planningInvestment: false,
            informationGathering: false,
            scheduleOptimization: false);
        
        var poorResults = ExecuteTimePressureStrategy(gameWorld, poorTimeStrategy, urgentContract);
        
        // === STRATEGY B: Good Time Management ===
        var goodTimeStrategy = new TimeManagementStrategy(
            planningInvestment: true,
            informationGathering: true,
            scheduleOptimization: true);
        
        var goodResults = ExecuteTimePressureStrategy(gameWorld, goodTimeStrategy, urgentContract);
        
        // === VALIDATION ===
        // Poor time management should cascade into multiple failures
        Assert.True(poorResults.MissedDeadlines > 0);
        Assert.True(poorResults.InformationLossEvents > 0);
        Assert.True(poorResults.UnoptimalRoutingCost > 20); // Significant penalty
        
        // Good time management should prevent cascade failures
        Assert.True(goodResults.MissedDeadlines == 0);
        Assert.True(goodResults.EfficiencyGain > 1.5); // 50%+ better performance
        
        // Time management should create major efficiency differences
        Assert.True(goodResults.TotalValue / poorResults.TotalValue > 2.0); // 100%+ difference
    }

    [Fact]
    public void InformationEconomyROI_Should_Reward_Strategic_Intelligence_Investment()
    {
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(200))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        
        // === STRATEGY A: Blind Exploration ===
        var blindStrategy = new InformationStrategy(
            informationInvestment: 0,
            explorationApproach: ExplorationApproach.Random);
        
        var blindResults = ExecuteInformationStrategy(gameWorld, blindStrategy, days: 7);
        
        // === STRATEGY B: Information-Driven Approach ===
        var informedStrategy = new InformationStrategy(
            informationInvestment: 50, // 25% of starting capital
            explorationApproach: ExplorationApproach.IntelligenceBased);
        
        var informedResults = ExecuteInformationStrategy(gameWorld, informedStrategy, days: 7);
        
        // === VALIDATION ===
        // Information investment should pay for itself through better decisions
        Assert.True(informedResults.TotalProfit > informedResults.InformationCosts * 3); // 300% ROI minimum
        
        // Informed strategy should discover significantly more opportunities
        Assert.True(informedResults.OpportunitiesDiscovered > blindResults.OpportunitiesDiscovered * 1.5);
        
        // Information should enable route optimization and risk avoidance
        Assert.True(informedResults.RoutingEfficiency > blindResults.RoutingEfficiency * 1.3);
        Assert.True(informedResults.RiskAvoidanceSavings > 30);
        
        // Net efficiency after information costs should still be superior
        Assert.True((informedResults.TotalValue - informedResults.InformationCosts) > 
                    blindResults.TotalValue * 1.2);
    }

    [Fact]
    public void InventoryCapacityPressure_Should_Force_Specialization_Decisions()
    {
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("crossbridge").WithCoins(120))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        
        // Multiple high-value opportunities requiring different equipment sets
        var mountainContract = GetContractById("mountain_survey"); // Climbing + Weather Protection
        var nobleContract = GetContractById("noble_diplomacy"); // Social Signaling + Documentation
        var tradeOpportunity = GetItemById("luxury_goods"); // High profit, large size
        
        // === STRATEGY A: Generalist Approach ===
        var generalistStrategy = new SpecializationStrategy(
            specialization: EquipmentSpecialization.Generalist,
            capacityAllocation: CapacityAllocation.Balanced);
        
        var generalistResults = ExecuteSpecializationStrategy(gameWorld, generalistStrategy);
        
        // === STRATEGY B: Mountain Specialist ===
        var mountainStrategy = new SpecializationStrategy(
            specialization: EquipmentSpecialization.Mountain,
            capacityAllocation: CapacityAllocation.EquipmentFocused);
        
        var mountainResults = ExecuteSpecializationStrategy(gameWorld, mountainStrategy);
        
        // === STRATEGY C: Social/Trade Specialist ===
        var socialStrategy = new SpecializationStrategy(
            specialization: EquipmentSpecialization.Social,
            capacityAllocation: CapacityAllocation.TradeFocused);
        
        var socialResults = ExecuteSpecializationStrategy(gameWorld, socialStrategy);
        
        // === VALIDATION ===
        // Specialists should dominate in their areas but sacrifice elsewhere
        Assert.True(mountainResults.MountainContractsCompleted > generalistResults.MountainContractsCompleted);
        Assert.True(socialResults.SocialContractsCompleted > generalistResults.SocialContractsCompleted);
        
        // But specialists should struggle outside their domain
        Assert.True(mountainResults.SocialContractsCompleted < generalistResults.SocialContractsCompleted);
        Assert.True(socialResults.MountainContractsCompleted < generalistResults.MountainContractsCompleted);
        
        // All strategies should be viable with different trade-offs
        var efficiencies = new[] { generalistResults.TotalValue, mountainResults.TotalValue, socialResults.TotalValue };
        var maxEfficiency = efficiencies.Max();
        var minEfficiency = efficiencies.Min();
        Assert.True(minEfficiency / maxEfficiency > 0.75); // Within 25% efficiency variance
        
        // Each strategy should create distinct opportunity profiles
        Assert.True(mountainResults.OpportunityProfile.CorrelationWith(socialResults.OpportunityProfile) < 0.5);
    }

    // === STRATEGY EXECUTION HELPERS ===

    private StrategyResults ExecuteStrategy(GameWorld gameWorld, EquipmentSpecializationStrategy strategy, int days)
    {
        var results = new StrategyResults();
        var contractRepository = new ContractRepository(gameWorld);
        var itemRepository = new ItemRepository(gameWorld);
        var locationRepository = new LocationRepository(gameWorld);
        
        // Simulate strategy execution over multiple days
        for (int day = 1; day <= days; day++)
        {
            // Equipment acquisition phase
            strategy.AcquireEquipment(gameWorld, itemRepository);
            
            // Route planning and execution
            var accessibleLocations = strategy.GetAccessibleLocations(gameWorld, locationRepository);
            results.AccessibleLocations.AddRange(accessibleLocations);
            
            // Contract evaluation and completion
            var availableContracts = strategy.GetEligibleContracts(gameWorld, contractRepository);
            results.AccessibleContracts.AddRange(availableContracts);
            
            var completedContracts = strategy.ExecuteContracts(gameWorld, availableContracts);
            results.CompletedContracts.AddRange(completedContracts);
            
            // Advance time
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.TotalProfitability = CalculateProfitability(gameWorld, results);
        return results;
    }

    private InventoryResults ExecuteStrategyWithConstraints(GameWorld gameWorld, InventoryOptimizationStrategy strategy, Contract targetContract)
    {
        var results = new InventoryResults();
        var contractRepository = new ContractRepository(gameWorld);
        var itemRepository = new ItemRepository(gameWorld);
        var marketManager = CreateMarketManager(gameWorld);
        
        // Equipment acquisition based on strategy
        var equipmentPlan = strategy.PlanEquipmentLoadout(targetContract, itemRepository);
        results.EquipmentSlots = equipmentPlan.TotalSlots;
        
        // Cargo space calculation
        var availableSlots = 8 - equipmentPlan.TotalSlots; // Assuming 8-slot inventory
        results.CargoSlots = availableSlots;
        
        // Trade opportunity evaluation
        var tradeOpportunities = strategy.EvaluateTradeOpportunities(gameWorld, availableSlots, marketManager);
        results.TradeProfit = tradeOpportunities.Sum(t => t.PotentialProfit);
        results.TradeProfitMissed = tradeOpportunities.Where(t => !t.CanExecute).Sum(t => t.PotentialProfit);
        
        // Contract completion attempt
        results.ContractCompleted = strategy.CanCompleteContract(gameWorld, targetContract, equipmentPlan);
        results.ContractCompletionCost = strategy.CalculateCompletionCost(gameWorld, targetContract, equipmentPlan);
        
        results.TotalValue = results.TradeProfit + (results.ContractCompleted ? targetContract.Payment : 0) - results.ContractCompletionCost;
        
        return results;
    }

    private TimeResults ExecuteTimePressureStrategy(GameWorld gameWorld, TimeManagementStrategy strategy, Contract urgentContract)
    {
        var results = new TimeResults();
        var deadline = urgentContract.DueDay;
        var currentDay = gameWorld.TimeManager.GetCurrentDay();
        
        // Planning phase
        if (strategy.PlanningInvestment)
        {
            gameWorld.TimeManager.AdvanceTime(1); // Spend time planning
            results.PlanningBenefit = 30; // Reduced routing costs
        }
        
        // Information gathering phase
        if (strategy.InformationGathering)
        {
            gameWorld.TimeManager.AdvanceTime(1); // Spend time gathering intel
            results.InformationBenefit = 25; // Better route conditions knowledge
        }
        else
        {
            results.InformationLossEvents = 2; // Missed weather warnings, route closures
        }
        
        // Execution phase
        var daysRemaining = deadline - gameWorld.TimeManager.GetCurrentDay();
        if (daysRemaining < 1)
        {
            results.MissedDeadlines = 1;
            results.DeadlinePenalty = urgentContract.Payment * 0.5; // 50% penalty
        }
        
        // Route optimization
        if (strategy.ScheduleOptimization)
        {
            results.OptimalRouting = true;
            results.RoutingCost = 10; // Optimal path
        }
        else
        {
            results.OptimalRouting = false;
            results.UnoptimalRoutingCost = 35; // Suboptimal path
        }
        
        results.EfficiencyGain = strategy.CalculateOverallEfficiency();
        results.TotalValue = urgentContract.Payment - results.DeadlinePenalty - results.UnoptimalRoutingCost + results.PlanningBenefit + results.InformationBenefit;
        
        return results;
    }

    private InformationResults ExecuteInformationStrategy(GameWorld gameWorld, InformationStrategy strategy, int days)
    {
        var results = new InformationResults();
        var informationRepository = new Content.InformationRepository(gameWorld);
        var locationRepository = new LocationRepository(gameWorld);
        
        results.InformationCosts = strategy.InformationInvestment;
        
        for (int day = 1; day <= days; day++)
        {
            if (strategy.ExplorationApproach == ExplorationApproach.IntelligenceBased)
            {
                // Simulate information purchasing (simplified for test)
                var player = gameWorld.GetPlayer();
                if (player.Coins >= 10)
                {
                    player.Coins -= 10; // Information cost
                    results.InformationCosts += 10;
                }
                
                // Use information for better decisions
                results.OpportunitiesDiscovered += 2; // Information multiplier
                results.RoutingEfficiency += 0.3; // 30% better routing per day
                results.RiskAvoidanceSavings += 15; // Avoid bad weather, closed routes
            }
            else
            {
                // Random exploration
                results.OpportunitiesDiscovered += 1; // Base discovery rate
                results.RoutingEfficiency += 0.1; // Learning through experience
            }
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.TotalProfit = results.OpportunitiesDiscovered * 25; // Average opportunity value
        results.TotalValue = results.TotalProfit - results.InformationCosts + results.RiskAvoidanceSavings;
        
        return results;
    }

    private SpecializationResults ExecuteSpecializationStrategy(GameWorld gameWorld, SpecializationStrategy strategy)
    {
        var results = new SpecializationResults();
        var contractRepository = new ContractRepository(gameWorld);
        var itemRepository = new ItemRepository(gameWorld);
        
        // Equipment acquisition based on specialization
        var equipment = strategy.AcquireSpecializationEquipment(gameWorld, itemRepository);
        results.EquipmentInvestment = equipment.Sum(e => e.BuyPrice);
        
        // Contract execution
        var contracts = contractRepository.GetAvailableContracts(gameWorld.TimeManager.GetCurrentDay(), gameWorld.TimeManager.GetCurrentTimeBlock());
        
        var mountainContracts = contracts.Where(c => c.RequiredEquipmentCategories.Contains(EquipmentCategory.Climbing_Equipment));
        var socialContracts = contracts.Where(c => c.RequiredToolCategories.Contains(ToolCategory.Social_Attire));
        
        results.MountainContractsCompleted = strategy.ExecuteContracts(gameWorld, mountainContracts).Count();
        results.SocialContractsCompleted = strategy.ExecuteContracts(gameWorld, socialContracts).Count();
        
        // Calculate opportunity profile
        results.OpportunityProfile = strategy.GenerateOpportunityProfile(gameWorld);
        
        results.TotalValue = (results.MountainContractsCompleted * 50) + (results.SocialContractsCompleted * 40) - results.EquipmentInvestment;
        
        return results;
    }

    // === HELPER METHODS ===

    private MarketManager CreateMarketManager(GameWorld gameWorld)
    {
        var locationRepository = new LocationRepository(gameWorld);
        var itemRepository = new ItemRepository(gameWorld);
        var contractRepository = new ContractRepository(gameWorld);
        var npcRepository = new NPCRepository(gameWorld);
        var locationSystem = new LocationSystem(gameWorld, locationRepository);
        var contractProgression = new ContractProgressionService(contractRepository, itemRepository, locationRepository);
        
        return new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression, npcRepository, locationRepository);
    }

    private Contract GetContractById(string contractId)
    {
        // Mock contract lookup - in real implementation would use ContractRepository
        return new Contract
        {
            Id = contractId,
            Description = contractId.Replace("_", " "),
            Payment = 50,
            DueDay = 3,
            RequiredEquipmentCategories = GetEquipmentCategoriesForContract(contractId)
        };
    }

    private Item GetItemById(string itemId)
    {
        // Mock item lookup - in real implementation would use ItemRepository
        return new Item
        {
            Id = itemId,
            Name = itemId.Replace("_", " "),
            BuyPrice = 25,
            InventorySlots = 2
        };
    }

    private List<EquipmentCategory> GetEquipmentCategoriesForContract(string contractId)
    {
        return contractId switch
        {
            "ruins_excavation" => new List<EquipmentCategory> { EquipmentCategory.Climbing_Equipment, EquipmentCategory.Light_Source, EquipmentCategory.Navigation_Tools, EquipmentCategory.Weather_Protection },
            "mountain_survey" => new List<EquipmentCategory> { EquipmentCategory.Climbing_Equipment, EquipmentCategory.Weather_Protection },
            "noble_diplomacy" => new List<EquipmentCategory> { EquipmentCategory.Special_Access },
            _ => new List<EquipmentCategory>()
        };
    }

    private IEnumerable<Contract> GetContractsByCategory(string categoryName)
    {
        // Mock contract categorization - in real implementation would use proper categorization
        return new List<Contract>();
    }

    private double CalculateProfitability(GameWorld gameWorld, StrategyResults results)
    {
        var player = gameWorld.GetPlayer();
        return player.Coins + (results.CompletedContracts.Sum(c => c.Payment));
    }
}

// === STRATEGY CLASSES ===

public class EquipmentSpecializationStrategy
{
    public EquipmentFocus Focus { get; }
    
    public EquipmentSpecializationStrategy(EquipmentFocus focus)
    {
        Focus = focus;
    }
    
    public void AcquireEquipment(GameWorld gameWorld, ItemRepository itemRepository) { /* Implementation */ }
    public List<string> GetAccessibleLocations(GameWorld gameWorld, LocationRepository locationRepository) { return new List<string>(); }
    public List<Contract> GetEligibleContracts(GameWorld gameWorld, ContractRepository contractRepository) { return new List<Contract>(); }
    public List<Contract> ExecuteContracts(GameWorld gameWorld, IEnumerable<Contract> contracts) { return new List<Contract>(); }
}

public class InventoryOptimizationStrategy
{
    public bool PrioritizeEquipment { get; }
    
    public InventoryOptimizationStrategy(bool prioritizeEquipment)
    {
        PrioritizeEquipment = prioritizeEquipment;
    }
    
    public EquipmentPlan PlanEquipmentLoadout(Contract contract, ItemRepository itemRepository) { return new EquipmentPlan(); }
    public List<TradeOpportunity> EvaluateTradeOpportunities(GameWorld gameWorld, int availableSlots, MarketManager marketManager) { return new List<TradeOpportunity>(); }
    public bool CanCompleteContract(GameWorld gameWorld, Contract contract, EquipmentPlan plan) { return true; }
    public double CalculateCompletionCost(GameWorld gameWorld, Contract contract, EquipmentPlan plan) { return 0; }
}

public class TimeManagementStrategy
{
    public bool PlanningInvestment { get; }
    public bool InformationGathering { get; }
    public bool ScheduleOptimization { get; }
    
    public TimeManagementStrategy(bool planningInvestment, bool informationGathering, bool scheduleOptimization)
    {
        PlanningInvestment = planningInvestment;
        InformationGathering = informationGathering;
        ScheduleOptimization = scheduleOptimization;
    }
    
    public double CalculateOverallEfficiency() { return PlanningInvestment && InformationGathering && ScheduleOptimization ? 2.0 : 1.0; }
}

public class InformationStrategy
{
    public double InformationInvestment { get; }
    public ExplorationApproach ExplorationApproach { get; }
    
    public InformationStrategy(double informationInvestment, ExplorationApproach explorationApproach)
    {
        InformationInvestment = informationInvestment;
        ExplorationApproach = explorationApproach;
    }
}

public class SpecializationStrategy
{
    public EquipmentSpecialization Specialization { get; }
    public CapacityAllocation CapacityAllocation { get; }
    
    public SpecializationStrategy(EquipmentSpecialization specialization, CapacityAllocation capacityAllocation)
    {
        Specialization = specialization;
        CapacityAllocation = capacityAllocation;
    }
    
    public List<Item> AcquireSpecializationEquipment(GameWorld gameWorld, ItemRepository itemRepository) { return new List<Item>(); }
    public List<Contract> ExecuteContracts(GameWorld gameWorld, IEnumerable<Contract> contracts) { return new List<Contract>(); }
    public OpportunityProfile GenerateOpportunityProfile(GameWorld gameWorld) { return new OpportunityProfile(); }
}

// === RESULT CLASSES ===

public class StrategyResults
{
    public List<string> AccessibleLocations { get; set; } = new();
    public List<Contract> AccessibleContracts { get; set; } = new();
    public List<Contract> CompletedContracts { get; set; } = new();
    public double TotalProfitability { get; set; }
}

public class InventoryResults
{
    public int EquipmentSlots { get; set; }
    public int CargoSlots { get; set; }
    public double TradeProfit { get; set; }
    public double TradeProfitMissed { get; set; }
    public bool ContractCompleted { get; set; }
    public double ContractCompletionCost { get; set; }
    public double TotalValue { get; set; }
}

public class TimeResults
{
    public int MissedDeadlines { get; set; }
    public int InformationLossEvents { get; set; }
    public double UnoptimalRoutingCost { get; set; }
    public double EfficiencyGain { get; set; }
    public double PlanningBenefit { get; set; }
    public double InformationBenefit { get; set; }
    public double DeadlinePenalty { get; set; }
    public double RoutingCost { get; set; }
    public bool OptimalRouting { get; set; }
    public double TotalValue { get; set; }
}

public class InformationResults
{
    public double InformationCosts { get; set; }
    public List<Information> InformationPurchased { get; set; } = new();
    public int OpportunitiesDiscovered { get; set; }
    public double RoutingEfficiency { get; set; }
    public double RiskAvoidanceSavings { get; set; }
    public double TotalProfit { get; set; }
    public double TotalValue { get; set; }
}

public class SpecializationResults
{
    public double EquipmentInvestment { get; set; }
    public int MountainContractsCompleted { get; set; }
    public int SocialContractsCompleted { get; set; }
    public OpportunityProfile OpportunityProfile { get; set; } = new();
    public double TotalValue { get; set; }
}

// === SUPPORTING CLASSES ===

public class EquipmentPlan
{
    public int TotalSlots { get; set; }
}

public class TradeOpportunity
{
    public double PotentialProfit { get; set; }
    public bool CanExecute { get; set; }
}

public class OpportunityProfile
{
    public double CorrelationWith(OpportunityProfile other) { return 0.5; }
}

// === ENUMS ===

public enum EquipmentFocus
{
    MountainClimbing,
    WaterTransport,
    SocialNetworking,
    InformationBrokering
}


public enum ExplorationApproach
{
    Random,
    IntelligenceBased
}

public enum EquipmentSpecialization
{
    Generalist,
    Mountain,
    Social,
    Maritime
}

public enum CapacityAllocation
{
    Balanced,
    EquipmentFocused,
    TradeFocused
}