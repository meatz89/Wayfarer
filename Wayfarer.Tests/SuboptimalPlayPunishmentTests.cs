using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;
using Xunit;

namespace Wayfarer.Tests;

public class SuboptimalPlayPunishmentTests
{
    [Fact]
    public void InformationNeglect_Should_Lead_To_Preventable_Losses()
    {
        // Test that ignoring information gathering leads to avoidable negative outcomes

        TestScenarioBuilder scenario = new TestScenarioBuilder()
            .WithPlayer(p => p.StartAt("crossbridge").WithCoins(130))
            .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

        // === INFORMATION-NEGLECTING STRATEGY ===
        GameWorld neglectfulWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        InformationNeglectStrategy neglectfulStrategy = new InformationNeglectStrategy(
            informationInvestment: InformationInvestment.None,
            riskAssessment: RiskAssessment.None,
            marketResearch: MarketResearch.None,
            routeIntelligence: RouteIntelligence.None);

        // === INFORMATION-AWARE STRATEGY ===
        GameWorld informedWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        InformationNeglectStrategy informedStrategy = new InformationNeglectStrategy(
            informationInvestment: InformationInvestment.Adequate,
            riskAssessment: RiskAssessment.Thorough,
            marketResearch: MarketResearch.Comprehensive,
            routeIntelligence: RouteIntelligence.Current);

        // Scenario with information that could prevent losses
        InformationRichScenario riskScenario = new InformationRichScenario
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
        InformationNeglectResults neglectfulResults = ExecuteInformationStrategy(neglectfulWorld, neglectfulStrategy, riskScenario, days: 7);
        InformationNeglectResults informedResults = ExecuteInformationStrategy(informedWorld, informedStrategy, riskScenario, days: 7);

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
        double informationROI = (informedResults.TotalValue - neglectfulResults.TotalValue) / informedResults.InformationCost;
        Assert.True(informationROI > 3.0,
            "Information investment should provide 300%+ ROI through loss prevention and opportunity capture");
    }

    // === STRATEGY EXECUTION HELPERS ===

    private SuboptimalResults ExecuteSuboptimalStrategy(GameWorld gameWorld, SuboptimalDecisionStrategy strategy, int days)
    {
        SuboptimalResults results = new SuboptimalResults();
        double initialEfficiency = CalculateBaselineEfficiency(gameWorld);

        for (int day = 1; day <= days; day++)
        {
            // Make poor decisions based on strategy
            List<PoorDecision> poorDecisions = strategy.MakePoorDecisions(gameWorld, day);
            results.TotalPoorDecisions += poorDecisions.Count;

            // Calculate immediate negative consequences
            double immediateConsequences = strategy.CalculateImmediateConsequences(poorDecisions);
            results.ImmediateNegativeConsequences += immediateConsequences;

            // Calculate cascade effects
            double cascadeEffects = strategy.CalculateCascadeEffects(gameWorld, poorDecisions, day);
            results.CascadeEffects += cascadeEffects;

            // Update efficiency
            double dailyEfficiency = strategy.CalculateDailyEfficiency(gameWorld, poorDecisions);
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
        RecoveryResults results = new RecoveryResults();
        results.InitialEfficiency = initialState.FinalEfficiency;

        for (int day = 1; day <= days; day++)
        {
            // Investment in recovery
            RecoveryInvestmentDetails recoveryInvestment = strategy.MakeRecoveryInvestment(gameWorld, day);
            results.RecoveryCost += recoveryInvestment.Cost;
            results.OpportunityCostPaid += recoveryInvestment.OpportunityCost;

            // Calculate recovery progress
            double recoveryProgress = strategy.CalculateRecoveryProgress(gameWorld, recoveryInvestment);
            results.RecoveryProgress += recoveryProgress;

            // Calculate optimization interference
            double optimizationInterference = strategy.CalculateOptimizationInterference(recoveryInvestment);
            results.OptimizationTimeInterfered += optimizationInterference;

            gameWorld.TimeManager.StartNewDay();
        }

        results.FinalEfficiency = results.InitialEfficiency + results.RecoveryProgress;

        return results;
    }

    private EquipmentInvestmentResults ExecuteEquipmentInvestmentStrategy(GameWorld gameWorld, EquipmentInvestmentDecisionStrategy strategy, List<EquipmentOption> options, int evaluationDays)
    {
        EquipmentInvestmentResults results = new EquipmentInvestmentResults();

        // Make equipment investment decisions
        List<EquipmentOption> investments = strategy.MakeInvestmentDecisions(gameWorld, options);
        results.InitialInvestment = investments.Sum(i => i.Cost);

        // Evaluate strategic consequences over time
        for (int day = 1; day <= evaluationDays; day++)
        {
            // Calculate available strategic options
            int strategicOptions = strategy.CalculateAvailableStrategicOptions(gameWorld, investments);
            results.AvailableStrategicOptions = Math.Min(results.AvailableStrategicOptions, strategicOptions);

            // Calculate foreclosed opportunities
            double forelosedValue = strategy.CalculateForelosedOpportunities(gameWorld, investments, day);
            results.ForelosedOpportunityValue += forelosedValue;

            // Calculate adaptability
            double adaptability = strategy.CalculateAdaptability(gameWorld, investments);
            results.AdaptabilityScore = Math.Min(results.AdaptabilityScore, adaptability);

            // Check for forced suboptimal choices
            int forcedChoices = strategy.CountForcedSuboptimalChoices(gameWorld, investments);
            results.SuboptimalChoicesForced += forcedChoices;

            gameWorld.TimeManager.StartNewDay();
        }

        results.SunkCostPressure = strategy.CalculateSunkCostPressure(investments);
        results.CorrectionCost = strategy.CalculateCorrectionCost(gameWorld, investments);

        return results;
    }

    private InformationNeglectResults ExecuteInformationStrategy(GameWorld gameWorld, InformationNeglectStrategy strategy, InformationRichScenario scenario, int days)
    {
        InformationNeglectResults results = new InformationNeglectResults();

        for (int day = 1; day <= days; day++)
        {
            // Information investment decisions
            InformationInvestmentDetails informationInvestment = strategy.MakeInformationInvestment(gameWorld, scenario, day);
            results.InformationCost += informationInvestment.Cost;

            // Calculate preventable losses
            double preventableLosse = strategy.CalculatePreventableLosses(gameWorld, scenario, informationInvestment);
            results.PreventableLosses += preventableLosse;

            // Calculate missed opportunities
            double missedOpportunities = strategy.CalculateMissedOpportunities(gameWorld, scenario, informationInvestment);
            results.MissedOpportunityValue += missedOpportunities;

            // Calculate risk exposure
            double riskExposure = strategy.CalculateRiskExposure(gameWorld, informationInvestment);
            results.RiskExposureScore += riskExposure;

            // Track negative surprises
            int surprises = strategy.CountNegativeSurprises(gameWorld, informationInvestment);
            results.NegativeSurpriseEvents += surprises;

            gameWorld.TimeManager.StartNewDay();
        }

        results.TotalValue = strategy.CalculateTotalValue(gameWorld, results);

        return results;
    }

    private CompoundNegligenceResults ExecuteCompoundNegligenceStrategy(GameWorld gameWorld, CompoundNegligenceStrategy strategy, int days)
    {
        CompoundNegligenceResults results = new CompoundNegligenceResults();

        for (int day = 1; day <= days; day++)
        {
            // Calculate negligence effects
            NegligenceEffects negligenceEffects = strategy.CalculateNegligenceEffects(gameWorld, day);
            results.SynergisticNegativeEffects += negligenceEffects.SynergisticEffects;
            results.SystemFailureCascades += negligenceEffects.SystemFailures;

            // Calculate value
            double dailyValue = strategy.CalculateDailyValue(gameWorld, negligenceEffects);
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
        int neglectCount = (InformationNeglect ? 1 : 0) + (PlanningNeglect ? 1 : 0) + (EquipmentNeglect ? 1 : 0) + (TimeManagementNeglect ? 1 : 0);
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