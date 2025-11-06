/// <summary>
/// STATELESS calculator for momentum and doubt mechanics.
/// All state lives in SocialSession. Methods accept session as parameter.
/// Manages momentum accumulation for request thresholds and doubt penalties.
/// </summary>
public class MomentumManager
{
    private const int MAX_DOUBT = 10;
    private const int TOKEN_TO_MOMENTUM_MULTIPLIER = 3;

    /// <summary>
    /// Parameterless constructor for DI
    /// </summary>
    public MomentumManager()
    {
    }

    /// <summary>
    /// Calculate initial momentum from tokens
    /// </summary>
    /// <param name="tokens">Dictionary of connection types and their token counts</param>
    /// <returns>Starting momentum value</returns>
    public int CalculateInitialMomentum(Dictionary<ConnectionType, int> tokens)
    {
        if (tokens == null) return 0;
        return tokens.Values.Sum() * TOKEN_TO_MOMENTUM_MULTIPLIER;
    }

    /// <summary>
    /// Add momentum (always positive)
    /// </summary>
    /// <param name="session">The conversation session to modify</param>
    /// <param name="amount">Amount of momentum to add</param>
    public void AddMomentum(SocialSession session, int amount)
    {
        if (amount <= 0) return;
        session.CurrentMomentum = Math.Max(0, session.CurrentMomentum + amount);
    }

    /// <summary>
    /// Add doubt (penalty resource)
    /// </summary>
    /// <param name="session">The conversation session to modify</param>
    /// <param name="amount">Amount of doubt to add</param>
    public void AddDoubt(SocialSession session, int amount)
    {
        if (amount <= 0) return;

        // Check if doubt is prevented by card effect
        if (session.PreventNextDoubtIncrease)
        {
            session.PreventNextDoubtIncrease = false; // Reset flag after use
            return; // Doubt increase prevented
        }

        session.CurrentDoubt = Math.Clamp(session.CurrentDoubt + amount, 0, session.MaxDoubt);
    }

    /// <summary>
    /// Reduce doubt (from Soothe effects)
    /// </summary>
    /// <param name="session">The conversation session to modify</param>
    /// <param name="amount">Amount of doubt to reduce</param>
    public void ReduceDoubt(SocialSession session, int amount)
    {
        if (amount <= 0) return;
        session.CurrentDoubt = Math.Max(0, session.CurrentDoubt - amount);
    }

    /// <summary>
    /// Get doubt penalty to success rates (5% per doubt point)
    /// </summary>
    /// <param name="session">The conversation session to read from</param>
    /// <returns>Penalty percentage to apply to all cards</returns>
    public int GetDoubtPenalty(SocialSession session)
    {
        return session.CurrentDoubt * 5; // 5% penalty per doubt point
    }

    /// <summary>
    /// Check if momentum reaches a specific threshold
    /// </summary>
    /// <param name="session">The conversation session to check</param>
    /// <param name="threshold">The threshold to check</param>
    /// <returns>True if current momentum >= threshold</returns>
    public bool CanReachThreshold(SocialSession session, int threshold)
    {
        return session.CurrentMomentum >= threshold;
    }

    /// <summary>
    /// Get momentum needed to reach a threshold
    /// </summary>
    /// <param name="session">The conversation session to check</param>
    /// <param name="threshold">The target threshold</param>
    /// <returns>Amount of momentum still needed (0 if already reached)</returns>
    public int GetMomentumNeeded(SocialSession session, int threshold)
    {
        return Math.Max(0, threshold - session.CurrentMomentum);
    }

    /// <summary>
    /// Double current momentum (State Manipulation effect)
    /// </summary>
    /// <param name="session">The conversation session to modify</param>
    public void DoubleMomentum(SocialSession session)
    {
        session.CurrentMomentum *= 2;
    }

    /// <summary>
    /// Lose momentum on failure (from State Manipulation failures)
    /// </summary>
    /// <param name="session">The conversation session to modify</param>
    /// <param name="amount">Amount of momentum to lose</param>
    public void LoseMomentum(SocialSession session, int amount)
    {
        session.CurrentMomentum = Math.Max(0, session.CurrentMomentum - amount);
    }

    /// <summary>
    /// Apply momentum erosion during LISTEN (current doubt reduces momentum)
    /// </summary>
    /// <param name="session">The conversation session to modify</param>
    /// <param name="personalityEnforcer">Personality enforcer to check for doubling effects</param>
    public void ApplyMomentumErosion(SocialSession session, PersonalityRuleEnforcer personalityEnforcer)
    {
        if (session.CurrentDoubt > 0)
        {
            int erosionAmount = session.CurrentDoubt;

            // Apply Devoted personality doubling if applicable
            if (personalityEnforcer.ShouldDoubleMomentumLoss())
            {
                erosionAmount *= 2;
            }

            session.CurrentMomentum = Math.Max(0, session.CurrentMomentum - erosionAmount);
        }
    }

    /// <summary>
    /// Get a visual representation of current momentum
    /// </summary>
    /// <param name="session">The conversation session to read from</param>
    /// <returns>String representation for UI display</returns>
    public string GetMomentumDisplay(SocialSession session)
    {
        return session.CurrentMomentum.ToString();
    }

    /// <summary>
    /// Get doubt effect description for UI
    /// </summary>
    /// <param name="session">The conversation session to read from</param>
    /// <returns>Description of doubt penalty on success rates</returns>
    public string GetDoubtEffectDescription(SocialSession session)
    {
        if (session.CurrentDoubt == 0)
            return "No penalty";

        return $"-{GetDoubtPenalty(session)}% success all cards";
    }

    /// <summary>
    /// Get current state for UI display
    /// </summary>
    /// <param name="session">The conversation session to read from</param>
    /// <returns>Current momentum state with momentum, doubt, and doubt penalty</returns>
    public MomentumState GetCurrentState(SocialSession session)
    {
        return new MomentumState(session.CurrentMomentum, session.CurrentDoubt, GetDoubtPenalty(session));
    }
}