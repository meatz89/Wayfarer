using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages momentum and doubt system for deterministic conversation mechanics.
/// Momentum accumulates to reach request thresholds, doubt penalizes success rates.
/// Replaces the probabilistic rapport system with deterministic resource management.
/// </summary>
public class MomentumManager
{
    private int currentMomentum = 0;
    private int currentDoubt = 0;
    private ConversationSession _session; // Reference to sync state back
    private const int MAX_DOUBT = 10;
    private const int TOKEN_TO_MOMENTUM_MULTIPLIER = 3;

    public int CurrentMomentum => currentMomentum;
    public int CurrentDoubt => currentDoubt;
    public int MaxDoubt => MAX_DOUBT;

    /// <summary>
    /// Initialize momentum manager with starting tokens converted to momentum
    /// </summary>
    /// <param name="tokens">Dictionary of connection types and their token counts</param>
    public MomentumManager(Dictionary<ConnectionType, int> tokens = null)
    {
        if (tokens != null)
        {
            currentMomentum = tokens.Values.Sum() * TOKEN_TO_MOMENTUM_MULTIPLIER;
        }
    }

    /// <summary>
    /// Initialize momentum manager for a new conversation with token data
    /// </summary>
    /// <param name="tokens">Dictionary of connection types and their token counts</param>
    public void InitializeForConversation(Dictionary<ConnectionType, int> tokens = null)
    {
        currentMomentum = 0;
        currentDoubt = 0;
        _session = null;

        if (tokens != null)
        {
            currentMomentum = tokens.Values.Sum() * TOKEN_TO_MOMENTUM_MULTIPLIER;
        }
    }

    /// <summary>
    /// Set the conversation session reference for state synchronization
    /// </summary>
    public void SetSession(ConversationSession session)
    {
        _session = session;
        SyncToSession(); // Initial sync
    }

    /// <summary>
    /// Sync internal state back to the conversation session
    /// </summary>
    private void SyncToSession()
    {
        if (_session != null)
        {
            _session.CurrentMomentum = currentMomentum;
            _session.CurrentDoubt = currentDoubt;
        }
    }

    /// <summary>
    /// Add momentum (always positive)
    /// </summary>
    /// <param name="amount">Amount of momentum to add</param>
    /// <param name="atmosphere">Current conversation atmosphere for modifiers</param>
    public void AddMomentum(int amount, AtmosphereType atmosphere = AtmosphereType.Neutral)
    {
        if (amount <= 0) return;

        // Apply atmosphere modifiers if needed
        int modified = ModifyByAtmosphere(amount, atmosphere);
        currentMomentum = Math.Max(0, currentMomentum + modified);
        SyncToSession();
    }

    /// <summary>
    /// Add doubt (penalty resource)
    /// </summary>
    /// <param name="amount">Amount of doubt to add</param>
    /// <param name="atmosphere">Current conversation atmosphere for modifiers</param>
    public void AddDoubt(int amount, AtmosphereType atmosphere = AtmosphereType.Neutral)
    {
        if (amount <= 0) return;

        int modified = ModifyByAtmosphere(amount, atmosphere);
        currentDoubt = Math.Clamp(currentDoubt + modified, 0, MAX_DOUBT);
        SyncToSession();
    }

    /// <summary>
    /// Reduce doubt (from Soothe effects)
    /// </summary>
    /// <param name="amount">Amount of doubt to reduce</param>
    /// <param name="atmosphere">Current conversation atmosphere for modifiers</param>
    public void ReduceDoubt(int amount, AtmosphereType atmosphere = AtmosphereType.Neutral)
    {
        if (amount <= 0) return;

        int modified = ModifyByAtmosphere(amount, atmosphere);
        currentDoubt = Math.Max(0, currentDoubt - modified);
        SyncToSession();
    }

    /// <summary>
    /// Get doubt penalty to success rates (5% per doubt point)
    /// </summary>
    /// <returns>Penalty percentage to apply to all cards</returns>
    public int GetDoubtPenalty()
    {
        return currentDoubt * 5; // 5% penalty per doubt point
    }

    /// <summary>
    /// Check if momentum reaches a specific threshold
    /// </summary>
    /// <param name="threshold">The threshold to check</param>
    /// <returns>True if current momentum >= threshold</returns>
    public bool CanReachThreshold(int threshold)
    {
        return currentMomentum >= threshold;
    }

    /// <summary>
    /// Get momentum needed to reach a threshold
    /// </summary>
    /// <param name="threshold">The target threshold</param>
    /// <returns>Amount of momentum still needed (0 if already reached)</returns>
    public int GetMomentumNeeded(int threshold)
    {
        return Math.Max(0, threshold - currentMomentum);
    }

    /// <summary>
    /// Double current momentum (State Manipulation effect)
    /// </summary>
    public void DoubleMomentum()
    {
        currentMomentum *= 2;
        SyncToSession();
    }

    /// <summary>
    /// Lose momentum on failure (from State Manipulation failures)
    /// </summary>
    /// <param name="amount">Amount of momentum to lose</param>
    public void LoseMomentum(int amount)
    {
        currentMomentum = Math.Max(0, currentMomentum - amount);
        SyncToSession();
    }

    /// <summary>
    /// Apply momentum erosion during LISTEN (current doubt reduces momentum)
    /// </summary>
    /// <param name="personalityEnforcer">Personality enforcer to check for doubling effects</param>
    public void ApplyMomentumErosion(PersonalityRuleEnforcer personalityEnforcer)
    {
        if (currentDoubt > 0)
        {
            int erosionAmount = currentDoubt;

            // Apply Devoted personality doubling if applicable
            if (personalityEnforcer.ShouldDoubleMomentumLoss())
            {
                erosionAmount *= 2;
            }

            currentMomentum = Math.Max(0, currentMomentum - erosionAmount);
            SyncToSession();
        }
    }

    /// <summary>
    /// Apply atmosphere modifiers to momentum/doubt changes
    /// </summary>
    /// <param name="baseChange">Base change amount</param>
    /// <param name="atmosphere">Current atmosphere</param>
    /// <returns>Modified change amount</returns>
    private int ModifyByAtmosphere(int baseChange, AtmosphereType atmosphere)
    {
        return atmosphere switch
        {
            AtmosphereType.Volatile => baseChange > 0 ? baseChange + 1 : baseChange,
            AtmosphereType.Exposed => baseChange * 2,
            AtmosphereType.Synchronized => baseChange * 2,
            _ => baseChange
        };
    }

    /// <summary>
    /// Get a visual representation of current momentum
    /// </summary>
    /// <returns>String representation for UI display</returns>
    public string GetMomentumDisplay()
    {
        return currentMomentum.ToString();
    }

    /// <summary>
    /// Get doubt effect description for UI
    /// </summary>
    /// <returns>Description of doubt penalty on success rates</returns>
    public string GetDoubtEffectDescription()
    {
        if (currentDoubt == 0)
            return "No penalty";

        return $"-{GetDoubtPenalty()}% success all cards";
    }

    /// <summary>
    /// Get current state for UI display
    /// </summary>
    /// <returns>Tuple of momentum, doubt, and doubt penalty</returns>
    public (int momentum, int doubt, int doubtPenalty) GetCurrentState()
    {
        return (currentMomentum, currentDoubt, GetDoubtPenalty());
    }
}