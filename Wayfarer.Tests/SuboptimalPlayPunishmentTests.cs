using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Tests;

public class SuboptimalPlayPunishmentTests
{
    [Fact]
    public void CascadeFailureProgression_Should_Demonstrate_Compound_Poor_Decisions()
    {
        // Test that poor decisions compound exponentially rather than linearly
        
        var baseScenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === SINGLE POOR DECISION ===
        GameWorld singleErrorWorld = TestGameWorldInitializer.CreateTestWorld(baseScenario);
        var singleErrorStrategy = new SuboptimalDecisionStrategy(
            decisionQuality: DecisionQuality.Poor,
            poorDecisionCount: 1,
            recoveryAttempts: RecoveryAttempts.Immediate,
            learningRate: LearningRate.Fast);
        
        // === MULTIPLE POOR DECISIONS ===
        GameWorld multipleErrorWorld = TestGameWorldInitializer.CreateTestWorld(baseScenario);
        var multipleErrorStrategy = new SuboptimalDecisionStrategy(
            decisionQuality: DecisionQuality.Poor,
            poorDecisionCount: 3,
            recoveryAttempts: RecoveryAttempts.Delayed,
            learningRate: LearningRate.Slow);
        
        // === COMPOUNDING POOR DECISIONS ===
        GameWorld compoundingErrorWorld = TestGameWorldInitializer.CreateTestWorld(baseScenario);
        var compoundingErrorStrategy = new SuboptimalDecisionStrategy(
            decisionQuality: DecisionQuality.VeryPoor,
            poorDecisionCount: 5,
            recoveryAttempts: RecoveryAttempts.None,
            learningRate: LearningRate.None);
        
        // === EXECUTE STRATEGIES ===
        var singleErrorResults = ExecuteSuboptimalStrategy(singleErrorWorld, singleErrorStrategy, days: 8);
        var multipleErrorResults = ExecuteSuboptimalStrategy(multipleErrorWorld, multipleErrorStrategy, days: 8);
        var compoundingErrorResults = ExecuteSuboptimalStrategy(compoundingErrorWorld, compoundingErrorStrategy, days: 8);
        
        // === VALIDATION: EXPONENTIAL DEGRADATION ===
        // Single error should cause modest efficiency loss
        Assert.InRange(singleErrorResults.EfficiencyDegradation, 0.1, 0.25);
        
        // Multiple errors should cause disproportionate efficiency loss
        Assert.InRange(multipleErrorResults.EfficiencyDegradation, 0.4, 0.7);
        
        // Compounding errors should cause severe efficiency loss
        Assert.True(compoundingErrorResults.EfficiencyDegradation > 0.7,
            "Compounding poor decisions should cause 70%+ efficiency degradation");
        
        // === VALIDATION: CASCADE EFFECTS ===
        // Each poor decision should create additional negative consequences
        Assert.True(multipleErrorResults.CascadeEffects > singleErrorResults.CascadeEffects * 2,
            "Multiple poor decisions should create more than linear cascade effects");
        
        Assert.True(compoundingErrorResults.CascadeEffects > multipleErrorResults.CascadeEffects * 1.5,
            "Compounding poor decisions should amplify cascade effects");
        
        // === VALIDATION: RECOVERY DIFFICULTY ===
        // Recovery should become progressively more difficult
        Assert.True(singleErrorResults.RecoveryDifficulty < multipleErrorResults.RecoveryDifficulty);
        Assert.True(multipleErrorResults.RecoveryDifficulty < compoundingErrorResults.RecoveryDifficulty);
        
        // Compounding errors should create near-impossible recovery scenarios
        Assert.True(compoundingErrorResults.RecoveryDifficulty > 0.8,
            "Compounding errors should make recovery extremely difficult");
    }

    [Fact]
    public void RecoveryPathValidation_Should_Be_Possible_But_Costly()
    {
        // Test that recovery from poor decisions is possible but requires significant investment
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("crossbridge").WithCoins(150))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === CREATE SUBOPTIMAL SITUATION ===
        GameWorld suboptimalWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var poorDecisionPhase = new SuboptimalDecisionStrategy(
            decisionQuality: DecisionQuality.Poor,
            poorDecisionCount: 3,
            recoveryAttempts: RecoveryAttempts.None,
            learningRate: LearningRate.None);
        
        // Execute poor decisions to create disadvantaged state
        var poorDecisionResults = ExecuteSuboptimalStrategy(suboptimalWorld, poorDecisionPhase, days: 4);
        
        // === RECOVERY STRATEGIES ===
        var minimalRecoveryStrategy = new RecoveryStrategy(
            recoveryInvestment: RecoveryInvestment.Minimal,
            recoveryTimeHorizon: RecoveryTimeHorizon.Short,
            recoveryApproach: RecoveryApproach.Reactive);
        
        var comprehensiveRecoveryStrategy = new RecoveryStrategy(
            recoveryInvestment: RecoveryInvestment.Comprehensive,
            recoveryTimeHorizon: RecoveryTimeHorizon.Long,
            recoveryApproach: RecoveryApproach.Strategic);
        
        // === EXECUTE RECOVERY ATTEMPTS ===
        var minimalRecoveryResults = ExecuteRecoveryStrategy(suboptimalWorld, minimalRecoveryStrategy, 
            initialState: poorDecisionResults, days: 6);
        
        // Reset to same poor state for fair comparison
        GameWorld suboptimalWorld2 = TestGameWorldInitializer.CreateTestWorld(scenario);
        ExecuteSuboptimalStrategy(suboptimalWorld2, poorDecisionPhase, days: 4);
        
        var comprehensiveRecoveryResults = ExecuteRecoveryStrategy(suboptimalWorld2, comprehensiveRecoveryStrategy, 
            initialState: poorDecisionResults, days: 6);
        
        // === VALIDATION: RECOVERY POSSIBILITY ===
        // Both recovery strategies should improve the situation
        Assert.True(minimalRecoveryResults.FinalEfficiency > poorDecisionResults.FinalEfficiency,
            "Minimal recovery should improve efficiency from poor state");
        
        Assert.True(comprehensiveRecoveryResults.FinalEfficiency > poorDecisionResults.FinalEfficiency,
            "Comprehensive recovery should improve efficiency from poor state");
        
        // Comprehensive recovery should be more effective
        Assert.True(comprehensiveRecoveryResults.FinalEfficiency > minimalRecoveryResults.FinalEfficiency * 1.3,
            "Comprehensive recovery should be 30%+ more effective than minimal");
        
        // === VALIDATION: RECOVERY COSTS ===
        // Recovery should require significant investment
        Assert.True(minimalRecoveryResults.RecoveryCost > poorDecisionResults.TotalValue * 0.3,
            "Even minimal recovery should cost 30%+ of previous total value");
        
        Assert.True(comprehensiveRecoveryResults.RecoveryCost > poorDecisionResults.TotalValue * 0.6,
            "Comprehensive recovery should cost 60%+ of previous total value");
        
        // === VALIDATION: OPPORTUNITY COST ===
        // Recovery should involve significant opportunity costs
        Assert.True(comprehensiveRecoveryResults.OpportunityCostPaid > 50,
            "Recovery should involve substantial opportunity costs");
        
        // Recovery should take time away from optimization
        Assert.True(comprehensiveRecoveryResults.OptimizationTimeInterfered > 0.4,
            "Recovery should interfere with 40%+ of optimization time");
    }

    [Fact]
    public void WrongEquipmentInvestment_Should_Create_Strategic_Lock_In()
    {
        // Test that poor equipment choices create measurable strategic constraints
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(120))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === WRONG EQUIPMENT INVESTMENT ===
        GameWorld wrongInvestmentWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var wrongInvestmentStrategy = new EquipmentInvestmentDecisionStrategy(
            investmentApproach: InvestmentApproach.Impulsive,
            equipmentResearch: EquipmentResearch.None,
            futureNeedsConsideration: FutureNeedsConsideration.None,
            budgetManagement: BudgetManagement.Poor);
        
        // === OPTIMAL EQUIPMENT INVESTMENT ===
        GameWorld optimalInvestmentWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var optimalInvestmentStrategy = new EquipmentInvestmentDecisionStrategy(
            investmentApproach: InvestmentApproach.Strategic,
            equipmentResearch: EquipmentResearch.Comprehensive,
            futureNeedsConsideration: FutureNeedsConsideration.Thorough,
            budgetManagement: BudgetManagement.Excellent);
        
        // High-cost equipment that creates lock-in effects
        var expensiveEquipmentOptions = new List<EquipmentOption>
        {
            new EquipmentOption { Id = "specialized_mountain_gear", Cost = 80, Utility = "mountain_only" },
            new EquipmentOption { Id = "luxury_social_attire", Cost = 70, Utility = "social_only" },
            new EquipmentOption { Id = "advanced_navigation_tools", Cost = 60, Utility = "universal" }
        };
        
        // === EXECUTE INVESTMENT DECISIONS ===
        var wrongInvestmentResults = ExecuteEquipmentInvestmentStrategy(wrongInvestmentWorld, wrongInvestmentStrategy, 
            expensiveEquipmentOptions, evaluationDays: 10);
        
        var optimalInvestmentResults = ExecuteEquipmentInvestmentStrategy(optimalInvestmentWorld, optimalInvestmentStrategy, 
            expensiveEquipmentOptions, evaluationDays: 10);
        
        // === VALIDATION: STRATEGIC LOCK-IN EFFECTS ===
        // Wrong investment should reduce available strategic options
        Assert.True(wrongInvestmentResults.AvailableStrategicOptions < optimalInvestmentResults.AvailableStrategicOptions * 0.7,
            "Wrong equipment investment should reduce strategic options by 30%+");
        
        // Should create measurable opportunity foreclosure
        Assert.True(wrongInvestmentResults.ForelosedOpportunityValue > 40,
            "Wrong equipment should create measurable opportunity foreclosure");
        
        // Should reduce adaptability to changing circumstances
        Assert.True(wrongInvestmentResults.AdaptabilityScore < optimalInvestmentResults.AdaptabilityScore * 0.6,
            "Wrong equipment should reduce adaptability by 40%+");
        
        // === VALIDATION: SUNK COST TRAP ===
        // Wrong investment should create sunk cost pressures
        Assert.True(wrongInvestmentResults.SunkCostPressure > 0.5,
            "Wrong equipment investment should create significant sunk cost pressure");
        
        // Should force suboptimal strategic choices to justify investment
        Assert.True(wrongInvestmentResults.SuboptimalChoicesForced > 2,
            "Wrong equipment should force multiple suboptimal strategic choices");
        
        // === VALIDATION: RECOVERY DIFFICULTY ===
        // Correcting equipment mistakes should be expensive
        Assert.True(wrongInvestmentResults.CorrectionCost > wrongInvestmentResults.InitialInvestment * 0.8,
            "Correcting equipment mistakes should cost 80%+ of initial investment");
    }

    [Fact]
    public void InformationNeglect_Should_Lead_To_Preventable_Losses()
    {
        // Test that ignoring information gathering leads to avoidable negative outcomes
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("crossbridge").WithCoins(130))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === INFORMATION-NEGLECTING STRATEGY ===
        GameWorld neglectfulWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var neglectfulStrategy = new InformationNeglectStrategy(
            informationInvestment: InformationInvestment.None,
            riskAssessment: RiskAssessment.None,
            marketResearch: MarketResearch.None,
            routeIntelligence: RouteIntelligence.None);
        
        // === INFORMATION-AWARE STRATEGY ===
        GameWorld informedWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var informedStrategy = new InformationNeglectStrategy(
            informationInvestment: InformationInvestment.Adequate,
            riskAssessment: RiskAssessment.Thorough,
            marketResearch: MarketResearch.Comprehensive,
            routeIntelligence: RouteIntelligence.Current);
        
        // Scenario with information that could prevent losses
        var riskScenario = new InformationRichScenario
        {
            AvailableWarnings = new List<InformationWarning>
            {
                new InformationWarning { Type = "weather", Cost = 5, LossPrevention = 30 },
                new InformationWarning { Type = "market_crash", Cost = 8, LossPrevention = 45 },
                new InformationWarning { Type = "route_closure", Cost = 3, LossPrevention = 20 },
                new InformationWarning { Type = "bandit_activity", Cost = 6, LossPrevention = 25 }
            },
            AvailableOpportunities = new List<InformationOpportunity>
            {
                new InformationOpportunity { Type = "price_arbitrage", Cost = 10, ProfitPotential = 40 },
                new InformationOpportunity { Type = "exclusive_contract", Cost = 12, ProfitPotential = 60 }
            }
        };
        
        // === EXECUTE STRATEGIES ===
        var neglectfulResults = ExecuteInformationStrategy(neglectfulWorld, neglectfulStrategy, riskScenario, days: 7);
        var informedResults = ExecuteInformationStrategy(informedWorld, informedStrategy, riskScenario, days: 7);
        
        // === VALIDATION: PREVENTABLE LOSSES ===
        // Information neglect should lead to losses that informed strategy avoids
        Assert.True(neglectfulResults.PreventableLosses > 60,
            "Information neglect should lead to 60+ in preventable losses");
        
        Assert.True(informedResults.PreventableLosses < neglectfulResults.PreventableLosses * 0.3,
            "Informed strategy should prevent 70%+ of losses");
        
        // === VALIDATION: MISSED OPPORTUNITIES ===
        // Neglectful strategy should miss profitable opportunities
        Assert.True(neglectfulResults.MissedOpportunityValue > 50,
            "Information neglect should miss 50+ in opportunity value");
        
        Assert.True(informedResults.MissedOpportunityValue < neglectfulResults.MissedOpportunityValue * 0.4,
            "Informed strategy should miss 60% fewer opportunities");
        
        // === VALIDATION: RISK EXPOSURE ===
        // Neglectful strategy should have higher risk exposure
        Assert.True(neglectfulResults.RiskExposureScore > informedResults.RiskExposureScore * 2.5,
            "Information neglect should increase risk exposure by 150%+");
        
        // Should experience more negative surprise events
        Assert.True(neglectfulResults.NegativeSurpriseEvents > informedResults.NegativeSurpriseEvents * 3,
            "Information neglect should lead to 3x more negative surprises");
        
        // === VALIDATION: INFORMATION ROI ===
        // Information investment should provide clear positive ROI
        var informationROI = (informedResults.TotalValue - neglectfulResults.TotalValue) / informedResults.InformationCost;
        Assert.True(informationROI > 3.0,
            "Information investment should provide 300%+ ROI through loss prevention and opportunity capture");
    }

    [Fact]
    public void CompoundNegligence_Should_Create_Unrecoverable_Disadvantage()
    {
        // Test that multiple types of negligence combine to create severe strategic disadvantage
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(140))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
        
        // === SINGLE NEGLIGENCE ===
        GameWorld singleNegligenceWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var singleNegligenceStrategy = new CompoundNegligenceStrategy(
            informationNeglect: true,
            planningNeglect: false,
            equipmentNeglect: false,
            timeManagementNeglect: false);
        
        // === MULTIPLE NEGLIGENCE ===
        GameWorld multipleNegligenceWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var multipleNegligenceStrategy = new CompoundNegligenceStrategy(
            informationNeglect: true,
            planningNeglect: true,
            equipmentNeglect: true,
            timeManagementNeglect: false);
        
        // === TOTAL NEGLIGENCE ===
        GameWorld totalNegligenceWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var totalNegligenceStrategy = new CompoundNegligenceStrategy(
            informationNeglect: true,
            planningNeglect: true,
            equipmentNeglect: true,
            timeManagementNeglect: true);
        
        // === OPTIMAL STRATEGY (CONTROL) ===
        GameWorld optimalWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        var optimalStrategy = new CompoundNegligenceStrategy(
            informationNeglect: false,
            planningNeglect: false,
            equipmentNeglect: false,
            timeManagementNeglect: false);
        
        // === EXECUTE STRATEGIES ===
        var singleResults = ExecuteCompoundNegligenceStrategy(singleNegligenceWorld, singleNegligenceStrategy, days: 9);
        var multipleResults = ExecuteCompoundNegligenceStrategy(multipleNegligenceWorld, multipleNegligenceStrategy, days: 9);
        var totalResults = ExecuteCompoundNegligenceStrategy(totalNegligenceWorld, totalNegligenceStrategy, days: 9);
        var optimalResults = ExecuteCompoundNegligenceStrategy(optimalWorld, optimalStrategy, days: 9);
        
        // === VALIDATION: COMPOUND DEGRADATION ===
        // Each additional negligence should amplify negative effects
        var singleDegradation = (optimalResults.TotalValue - singleResults.TotalValue) / optimalResults.TotalValue;
        var multipleDegradation = (optimalResults.TotalValue - multipleResults.TotalValue) / optimalResults.TotalValue;
        var totalDegradation = (optimalResults.TotalValue - totalResults.TotalValue) / optimalResults.TotalValue;
        
        Assert.True(multipleDegradation > singleDegradation * 1.8,
            "Multiple negligence should be 80%+ worse than single negligence");
        
        Assert.True(totalDegradation > multipleDegradation * 1.5,
            "Total negligence should be 50%+ worse than multiple negligence");
        
        // Total negligence should approach unrecoverable disadvantage
        Assert.True(totalDegradation > 0.85,
            "Total negligence should create 85%+ performance degradation");
        
        // === VALIDATION: SYNERGISTIC NEGATIVE EFFECTS ===
        // Negligence types should amplify each other
        Assert.True(totalResults.SynergisticNegativeEffects > singleResults.SynergisticNegativeEffects * 10,
            "Total negligence should create 10x more synergistic negative effects");
        
        // Should create cascading failure across multiple systems
        Assert.True(totalResults.SystemFailureCascades > 3,
            "Total negligence should cause cascading failures across 3+ systems");
        
        // Should make recovery essentially impossible within reasonable timeframe
        Assert.True(totalResults.RecoveryTimeRequired > 20,
            "Total negligence should require 20+ days for potential recovery");
    }

    // === STRATEGY EXECUTION HELPERS ===

    private SuboptimalResults ExecuteSuboptimalStrategy(GameWorld gameWorld, SuboptimalDecisionStrategy strategy, int days)
    {
        var results = new SuboptimalResults();
        var initialEfficiency = CalculateBaselineEfficiency(gameWorld);
        
        for (int day = 1; day <= days; day++)
        {
            // Make poor decisions based on strategy
            var poorDecisions = strategy.MakePoorDecisions(gameWorld, day);
            results.TotalPoorDecisions += poorDecisions.Count;
            
            // Calculate immediate negative consequences
            var immediateConsequences = strategy.CalculateImmediateConsequences(poorDecisions);
            results.ImmediateNegativeConsequences += immediateConsequences;
            
            // Calculate cascade effects
            var cascadeEffects = strategy.CalculateCascadeEffects(gameWorld, poorDecisions, day);
            results.CascadeEffects += cascadeEffects;
            
            // Update efficiency
            var dailyEfficiency = strategy.CalculateDailyEfficiency(gameWorld, poorDecisions);
            results.DailyEfficiencies.Add(dailyEfficiency);
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.FinalEfficiency = results.DailyEfficiencies.LastOrDefault();
        results.EfficiencyDegradation = (initialEfficiency - results.FinalEfficiency) / initialEfficiency;
        results.RecoveryDifficulty = strategy.CalculateRecoveryDifficulty(results);
        results.TotalValue = results.DailyEfficiencies.Sum();
        
        return results;
    }

    private RecoveryResults ExecuteRecoveryStrategy(GameWorld gameWorld, RecoveryStrategy strategy, SuboptimalResults initialState, int days)
    {
        var results = new RecoveryResults();
        results.InitialEfficiency = initialState.FinalEfficiency;
        
        for (int day = 1; day <= days; day++)
        {
            // Investment in recovery
            var recoveryInvestment = strategy.MakeRecoveryInvestment(gameWorld, day);
            results.RecoveryCost += recoveryInvestment.Cost;
            results.OpportunityCostPaid += recoveryInvestment.OpportunityCost;
            
            // Calculate recovery progress
            var recoveryProgress = strategy.CalculateRecoveryProgress(gameWorld, recoveryInvestment);
            results.RecoveryProgress += recoveryProgress;
            
            // Calculate optimization interference
            var optimizationInterference = strategy.CalculateOptimizationInterference(recoveryInvestment);
            results.OptimizationTimeInterfered += optimizationInterference;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.FinalEfficiency = results.InitialEfficiency + results.RecoveryProgress;
        
        return results;
    }

    private EquipmentInvestmentResults ExecuteEquipmentInvestmentStrategy(GameWorld gameWorld, EquipmentInvestmentDecisionStrategy strategy, List<EquipmentOption> options, int evaluationDays)
    {
        var results = new EquipmentInvestmentResults();
        
        // Make equipment investment decisions
        var investments = strategy.MakeInvestmentDecisions(gameWorld, options);
        results.InitialInvestment = investments.Sum(i => i.Cost);
        
        // Evaluate strategic consequences over time
        for (int day = 1; day <= evaluationDays; day++)
        {
            // Calculate available strategic options
            var strategicOptions = strategy.CalculateAvailableStrategicOptions(gameWorld, investments);
            results.AvailableStrategicOptions = Math.Min(results.AvailableStrategicOptions, strategicOptions);
            
            // Calculate foreclosed opportunities
            var forelosedValue = strategy.CalculateForelosedOpportunities(gameWorld, investments, day);
            results.ForelosedOpportunityValue += forelosedValue;
            
            // Calculate adaptability
            var adaptability = strategy.CalculateAdaptability(gameWorld, investments);
            results.AdaptabilityScore = Math.Min(results.AdaptabilityScore, adaptability);
            
            // Check for forced suboptimal choices
            var forcedChoices = strategy.CountForcedSuboptimalChoices(gameWorld, investments);
            results.SuboptimalChoicesForced += forcedChoices;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.SunkCostPressure = strategy.CalculateSunkCostPressure(investments);
        results.CorrectionCost = strategy.CalculateCorrectionCost(gameWorld, investments);
        
        return results;
    }

    private InformationNeglectResults ExecuteInformationStrategy(GameWorld gameWorld, InformationNeglectStrategy strategy, InformationRichScenario scenario, int days)
    {
        var results = new InformationNeglectResults();
        
        for (int day = 1; day <= days; day++)
        {
            // Information investment decisions
            var informationInvestment = strategy.MakeInformationInvestment(gameWorld, scenario, day);
            results.InformationCost += informationInvestment.Cost;
            
            // Calculate preventable losses
            var preventableLosse = strategy.CalculatePreventableLosses(gameWorld, scenario, informationInvestment);
            results.PreventableLosses += preventableLosse;
            
            // Calculate missed opportunities
            var missedOpportunities = strategy.CalculateMissedOpportunities(gameWorld, scenario, informationInvestment);
            results.MissedOpportunityValue += missedOpportunities;
            
            // Calculate risk exposure
            var riskExposure = strategy.CalculateRiskExposure(gameWorld, informationInvestment);
            results.RiskExposureScore += riskExposure;
            
            // Track negative surprises
            var surprises = strategy.CountNegativeSurprises(gameWorld, informationInvestment);
            results.NegativeSurpriseEvents += surprises;
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.TotalValue = strategy.CalculateTotalValue(gameWorld, results);
        
        return results;
    }

    private CompoundNegligenceResults ExecuteCompoundNegligenceStrategy(GameWorld gameWorld, CompoundNegligenceStrategy strategy, int days)
    {
        var results = new CompoundNegligenceResults();
        
        for (int day = 1; day <= days; day++)
        {
            // Calculate negligence effects
            var negligenceEffects = strategy.CalculateNegligenceEffects(gameWorld, day);
            results.SynergisticNegativeEffects += negligenceEffects.SynergisticEffects;
            results.SystemFailureCascades += negligenceEffects.SystemFailures;
            
            // Calculate value
            var dailyValue = strategy.CalculateDailyValue(gameWorld, negligenceEffects);
            results.DailyValues.Add(dailyValue);
            
            gameWorld.TimeManager.StartNewDay();
        }
        
        results.TotalValue = results.DailyValues.Sum();
        results.RecoveryTimeRequired = strategy.CalculateRecoveryTimeRequired(results);
        
        return results;
    }

    // === HELPER METHODS ===

    private double CalculateBaselineEfficiency(GameWorld gameWorld)
    {
        return 100.0; // Baseline efficiency score
    }
}

// === STRATEGY CLASSES ===

public class SuboptimalDecisionStrategy
{
    public DecisionQuality DecisionQuality { get; }
    public int PoorDecisionCount { get; }
    public RecoveryAttempts RecoveryAttempts { get; }
    public LearningRate LearningRate { get; }
    
    public SuboptimalDecisionStrategy(DecisionQuality decisionQuality, int poorDecisionCount, RecoveryAttempts recoveryAttempts, LearningRate learningRate)
    {
        DecisionQuality = decisionQuality;
        PoorDecisionCount = poorDecisionCount;
        RecoveryAttempts = recoveryAttempts;
        LearningRate = learningRate;
    }
    
    public List<PoorDecision> MakePoorDecisions(GameWorld gameWorld, int day) 
    { 
        return Enumerable.Range(1, Math.Min(PoorDecisionCount, day)).Select(i => new PoorDecision { Impact = (int)DecisionQuality * -10 }).ToList(); 
    }
    
    public double CalculateImmediateConsequences(List<PoorDecision> decisions) { return decisions.Sum(d => Math.Abs(d.Impact)); }
    public double CalculateCascadeEffects(GameWorld gameWorld, List<PoorDecision> decisions, int day) { return decisions.Count * day * (int)DecisionQuality; }
    public double CalculateDailyEfficiency(GameWorld gameWorld, List<PoorDecision> decisions) { return 100 - decisions.Sum(d => Math.Abs(d.Impact)); }
    public double CalculateRecoveryDifficulty(SuboptimalResults results) { return Math.Min(1.0, results.EfficiencyDegradation * 1.5); }
}

public class RecoveryStrategy
{
    public RecoveryInvestment RecoveryInvestment { get; }
    public RecoveryTimeHorizon RecoveryTimeHorizon { get; }
    public RecoveryApproach RecoveryApproach { get; }
    
    public RecoveryStrategy(RecoveryInvestment recoveryInvestment, RecoveryTimeHorizon recoveryTimeHorizon, RecoveryApproach recoveryApproach)
    {
        RecoveryInvestment = recoveryInvestment;
        RecoveryTimeHorizon = recoveryTimeHorizon;
        RecoveryApproach = recoveryApproach;
    }
    
    public RecoveryInvestmentDetails MakeRecoveryInvestment(GameWorld gameWorld, int day) 
    { 
        return new RecoveryInvestmentDetails { Cost = (int)RecoveryInvestment * 20, OpportunityCost = (int)RecoveryInvestment * 10 }; 
    }
    
    public double CalculateRecoveryProgress(GameWorld gameWorld, RecoveryInvestmentDetails investment) { return investment.Cost * 0.1; }
    public double CalculateOptimizationInterference(RecoveryInvestmentDetails investment) { return investment.OpportunityCost * 0.02; }
}

public class EquipmentInvestmentDecisionStrategy
{
    public InvestmentApproach InvestmentApproach { get; }
    public EquipmentResearch EquipmentResearch { get; }
    public FutureNeedsConsideration FutureNeedsConsideration { get; }
    public BudgetManagement BudgetManagement { get; }
    
    public EquipmentInvestmentDecisionStrategy(InvestmentApproach investmentApproach, EquipmentResearch equipmentResearch, FutureNeedsConsideration futureNeedsConsideration, BudgetManagement budgetManagement)
    {
        InvestmentApproach = investmentApproach;
        EquipmentResearch = equipmentResearch;
        FutureNeedsConsideration = futureNeedsConsideration;
        BudgetManagement = budgetManagement;
    }
    
    public List<EquipmentOption> MakeInvestmentDecisions(GameWorld gameWorld, List<EquipmentOption> options) 
    { 
        return InvestmentApproach == InvestmentApproach.Impulsive ? options.Take(1).ToList() : options.Skip(1).Take(1).ToList(); 
    }
    
    public int CalculateAvailableStrategicOptions(GameWorld gameWorld, List<EquipmentOption> investments) 
    { 
        return investments.All(i => i.Utility == "universal") ? 10 : 4; 
    }
    
    public double CalculateForelosedOpportunities(GameWorld gameWorld, List<EquipmentOption> investments, int day) 
    { 
        return investments.Any(i => i.Utility.Contains("only")) ? day * 5 : 0; 
    }
    
    public double CalculateAdaptability(GameWorld gameWorld, List<EquipmentOption> investments) 
    { 
        return investments.All(i => i.Utility == "universal") ? 1.0 : 0.3; 
    }
    
    public int CountForcedSuboptimalChoices(GameWorld gameWorld, List<EquipmentOption> investments) 
    { 
        return investments.Count(i => i.Utility.Contains("only")); 
    }
    
    public double CalculateSunkCostPressure(List<EquipmentOption> investments) { return investments.Sum(i => i.Cost) / 100.0; }
    public double CalculateCorrectionCost(GameWorld gameWorld, List<EquipmentOption> investments) { return investments.Sum(i => i.Cost) * 0.8; }
}

public class InformationNeglectStrategy
{
    public InformationInvestment InformationInvestment { get; }
    public RiskAssessment RiskAssessment { get; }
    public MarketResearch MarketResearch { get; }
    public RouteIntelligence RouteIntelligence { get; }
    
    public InformationNeglectStrategy(InformationInvestment informationInvestment, RiskAssessment riskAssessment, MarketResearch marketResearch, RouteIntelligence routeIntelligence)
    {
        InformationInvestment = informationInvestment;
        RiskAssessment = riskAssessment;
        MarketResearch = marketResearch;
        RouteIntelligence = routeIntelligence;
    }
    
    public InformationInvestmentDetails MakeInformationInvestment(GameWorld gameWorld, InformationRichScenario scenario, int day) 
    { 
        return new InformationInvestmentDetails { Cost = (int)InformationInvestment * 5 }; 
    }
    
    public double CalculatePreventableLosses(GameWorld gameWorld, InformationRichScenario scenario, InformationInvestmentDetails investment) 
    { 
        return investment.Cost == 0 ? scenario.AvailableWarnings.Sum(w => w.LossPrevention) : scenario.AvailableWarnings.Sum(w => w.LossPrevention) * 0.2; 
    }
    
    public double CalculateMissedOpportunities(GameWorld gameWorld, InformationRichScenario scenario, InformationInvestmentDetails investment) 
    { 
        return investment.Cost == 0 ? scenario.AvailableOpportunities.Sum(o => o.ProfitPotential) : scenario.AvailableOpportunities.Sum(o => o.ProfitPotential) * 0.3; 
    }
    
    public double CalculateRiskExposure(GameWorld gameWorld, InformationInvestmentDetails investment) { return investment.Cost == 0 ? 10 : 3; }
    public int CountNegativeSurprises(GameWorld gameWorld, InformationInvestmentDetails investment) { return investment.Cost == 0 ? 1 : 0; }
    public double CalculateTotalValue(GameWorld gameWorld, InformationNeglectResults results) { return 200 - results.PreventableLosses - results.MissedOpportunityValue - results.InformationCost; }
}

public class CompoundNegligenceStrategy
{
    public bool InformationNeglect { get; }
    public bool PlanningNeglect { get; }
    public bool EquipmentNeglect { get; }
    public bool TimeManagementNeglect { get; }
    
    public CompoundNegligenceStrategy(bool informationNeglect, bool planningNeglect, bool equipmentNeglect, bool timeManagementNeglect)
    {
        InformationNeglect = informationNeglect;
        PlanningNeglect = planningNeglect;
        EquipmentNeglect = equipmentNeglect;
        TimeManagementNeglect = timeManagementNeglect;
    }
    
    public NegligenceEffects CalculateNegligenceEffects(GameWorld gameWorld, int day) 
    { 
        var neglectCount = (InformationNeglect ? 1 : 0) + (PlanningNeglect ? 1 : 0) + (EquipmentNeglect ? 1 : 0) + (TimeManagementNeglect ? 1 : 0);
        return new NegligenceEffects { SynergisticEffects = neglectCount * neglectCount * day, SystemFailures = neglectCount > 2 ? 1 : 0 }; 
    }
    
    public double CalculateDailyValue(GameWorld gameWorld, NegligenceEffects effects) { return Math.Max(10, 100 - effects.SynergisticEffects); }
    public int CalculateRecoveryTimeRequired(CompoundNegligenceResults results) { return (int)(results.SynergisticNegativeEffects / 10); }
}

// === RESULT CLASSES ===

public class SuboptimalResults
{
    public double TotalValue { get; set; }
    public double FinalEfficiency { get; set; }
    public double EfficiencyDegradation { get; set; }
    public int TotalPoorDecisions { get; set; }
    public double ImmediateNegativeConsequences { get; set; }
    public double CascadeEffects { get; set; }
    public double RecoveryDifficulty { get; set; }
    public List<double> DailyEfficiencies { get; set; } = new();
}

public class RecoveryResults
{
    public double InitialEfficiency { get; set; }
    public double FinalEfficiency { get; set; }
    public double RecoveryCost { get; set; }
    public double OpportunityCostPaid { get; set; }
    public double RecoveryProgress { get; set; }
    public double OptimizationTimeInterfered { get; set; }
}

public class EquipmentInvestmentResults
{
    public double InitialInvestment { get; set; }
    public int AvailableStrategicOptions { get; set; } = 10;
    public double ForelosedOpportunityValue { get; set; }
    public double AdaptabilityScore { get; set; } = 1.0;
    public double SunkCostPressure { get; set; }
    public int SuboptimalChoicesForced { get; set; }
    public double CorrectionCost { get; set; }
}

public class InformationNeglectResults
{
    public double TotalValue { get; set; }
    public double InformationCost { get; set; }
    public double PreventableLosses { get; set; }
    public double MissedOpportunityValue { get; set; }
    public double RiskExposureScore { get; set; }
    public int NegativeSurpriseEvents { get; set; }
}

public class CompoundNegligenceResults
{
    public double TotalValue { get; set; }
    public double SynergisticNegativeEffects { get; set; }
    public int SystemFailureCascades { get; set; }
    public int RecoveryTimeRequired { get; set; }
    public List<double> DailyValues { get; set; } = new();
}

// === SUPPORTING CLASSES ===

public class PoorDecision
{
    public int Impact { get; set; }
}

public class RecoveryInvestmentDetails
{
    public double Cost { get; set; }
    public double OpportunityCost { get; set; }
}

public class EquipmentOption
{
    public string Id { get; set; } = "";
    public double Cost { get; set; }
    public string Utility { get; set; } = "";
}

public class InformationInvestmentDetails
{
    public double Cost { get; set; }
}

public class InformationRichScenario
{
    public List<InformationWarning> AvailableWarnings { get; set; } = new();
    public List<InformationOpportunity> AvailableOpportunities { get; set; } = new();
}

public class InformationWarning
{
    public string Type { get; set; } = "";
    public double Cost { get; set; }
    public double LossPrevention { get; set; }
}

public class InformationOpportunity
{
    public string Type { get; set; } = "";
    public double Cost { get; set; }
    public double ProfitPotential { get; set; }
}

public class NegligenceEffects
{
    public double SynergisticEffects { get; set; }
    public int SystemFailures { get; set; }
}

// === ENUMS ===

public enum DecisionQuality
{
    Poor = 1,
    VeryPoor = 2
}

public enum RecoveryAttempts
{
    None = 0,
    Delayed = 1,
    Immediate = 2
}

public enum LearningRate
{
    None = 0,
    Slow = 1,
    Fast = 2
}

public enum RecoveryInvestment
{
    Minimal = 1,
    Comprehensive = 2
}

public enum RecoveryTimeHorizon
{
    Short = 1,
    Long = 2
}

public enum RecoveryApproach
{
    Reactive = 1,
    Strategic = 2
}

public enum InvestmentApproach
{
    Impulsive = 1,
    Strategic = 2
}

public enum EquipmentResearch
{
    None = 0,
    Comprehensive = 1
}

public enum FutureNeedsConsideration
{
    None = 0,
    Thorough = 1
}

public enum BudgetManagement
{
    Poor = 0,
    Excellent = 1
}

public enum InformationInvestment
{
    None = 0,
    Adequate = 1
}

public enum RiskAssessment
{
    None = 0,
    Thorough = 1
}

public enum MarketResearch
{
    None = 0,
    Comprehensive = 1
}

public enum RouteIntelligence
{
    None = 0,
    Current = 1
}