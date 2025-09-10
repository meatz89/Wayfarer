/// <summary>
/// Represents the type of exchange being performed.
/// Each type has different mechanical implications and UI presentation.
/// </summary>
public enum ExchangeType
{
    /// <summary>
    /// Standard trade - player offers resources for other resources.
    /// Both parties must agree to the exchange.
    /// </summary>
    Trade,

    /// <summary>
    /// Direct purchase - player buys goods or items for coins.
    /// Fixed prices, no negotiation.
    /// </summary>
    Purchase,

    /// <summary>
    /// Service exchange - player pays for services or actions.
    /// May involve time costs or other mechanics.
    /// </summary>
    Service
}