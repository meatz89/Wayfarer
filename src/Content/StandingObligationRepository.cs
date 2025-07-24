public class StandingObligationRepository
{
    private readonly GameWorld _gameWorld;

    public StandingObligationRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    // Get all obligation templates available in the game
    public List<StandingObligation> GetAllObligationTemplates()
    {
        return _gameWorld.WorldState.StandingObligationTemplates ?? new List<StandingObligation>();
    }

    // Get obligation template by ID
    public StandingObligation GetObligationTemplate(string obligationId)
    {
        return GetAllObligationTemplates()
            .FirstOrDefault(o => o.ID.Equals(obligationId, StringComparison.OrdinalIgnoreCase));
    }

    // Get all active player obligations
    public List<StandingObligation> GetPlayerObligations()
    {
        return _gameWorld.GetPlayer().StandingObligations
            .Where(o => o.IsActive)
            .ToList();
    }

    // Check if player has a specific obligation
    public bool HasObligation(string obligationId)
    {
        return GetPlayerObligations()
            .Any(o => o.ID.Equals(obligationId, StringComparison.OrdinalIgnoreCase));
    }

    // Get obligations for a specific token type
    public List<StandingObligation> GetObligationsForTokenType(ConnectionType tokenType)
    {
        return GetPlayerObligations()
            .Where(o => o.RelatedTokenType == tokenType)
            .ToList();
    }

    // Get obligations that affect letter entry position
    public List<StandingObligation> GetPositionAffectingObligations()
    {
        return GetPlayerObligations()
            .Where(o => o.BenefitEffects.Any(e =>
                e == ObligationEffect.NoblesPriority ||
                e == ObligationEffect.CommonFolksPriority ||
                e == ObligationEffect.PatronJumpToTop))
            .ToList();
    }

    // Get obligations that provide coin bonuses
    public List<StandingObligation> GetCoinBonusObligations()
    {
        return GetPlayerObligations()
            .Where(o => o.BenefitEffects.Any(e =>
                e == ObligationEffect.TradeBonus ||
                e == ObligationEffect.ShadowTriplePay))
            .ToList();
    }

    // Get obligations that require forced letter generation
    public List<StandingObligation> GetForcedLetterObligations()
    {
        return GetPlayerObligations()
            .Where(o => o.BenefitEffects.Any(e =>
                e == ObligationEffect.ShadowForced ||
                e == ObligationEffect.PatronMonthly))
            .ToList();
    }

    // Get obligations that restrict player actions
    public List<StandingObligation> GetRestrictiveObligations()
    {
        return GetPlayerObligations()
            .Where(o => o.ConstraintEffects.Any())
            .ToList();
    }
}